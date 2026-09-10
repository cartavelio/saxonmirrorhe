package com.saxonica.ee.validate;

import com.saxonica.ee.schema.TypeReference;
import com.saxonica.ee.schema.UserUnionType;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.instruct.DummyNamespaceResolver;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.str.CodePoints;
import net.sf.saxon.trans.XPathException;

import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.value.Whitespace;

import java.util.*;


/**
 * This class validates that elements and attributes of type ENTITY or ENTITIES have values are
 * declared in the DOCTYPE declaration of the instance document as unparsed entities.
 * <p>This component of the validation pipeline is used during standalone schema validation, but
 * not during validation invoked from XSLT or XQuery, as constructed trees never contain
 * unparsed entities.</p>
 */


public class EntityValidator extends ValidatingFilter {

    private final Set<String> declaredEntities = new HashSet<String>(10);

    /*@Nullable*/ private EntityChecker currentElementChecker = null;
    private final UnicodeBuilder buffer = new UnicodeBuilder();
    private Location textLocationId = Loc.NONE;
    private final Map<SchemaType, EntityChecker> checkerMap = new HashMap<>(10);

    /**
     * Create a validator for attributes of type ENTITY or ENTITIES
     *
     * @param next the next receiver in the pipeline
     */

    public EntityValidator(Receiver next) {
        super(next);
    }

    /**
     * Set the URI for an unparsed entity in the document.
     */
    @Override
    public void setUnparsedEntity(String name, String uri, String publicId) throws XPathException {
        declaredEntities.add(name);
    }


    /**
     * Notify the start of an element
     */

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties)
            throws XPathException {
        currentElementChecker = checkerMap.get(type);
        if (currentElementChecker == null) {
            currentElementChecker = allocateChecker(type);
            checkerMap.put(type, currentElementChecker);
        }
        if (currentElementChecker == SINGLETON_NON_ENTITY_CHECKER) {
            currentElementChecker = null;
        }
        for (AttributeInfo att : attributes) {
            SimpleType typeCode = att.getType();
            EntityChecker checker = checkerMap.get(typeCode);
            if (checker == null) {
                checker = allocateChecker(typeCode);
                checkerMap.put(typeCode, checker);
            }
            checker.checkValue(StringView.of(att.getValue()).tidy(), att.getLocation());
        }
        nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
    }

    /**
     * Character data
     */

    @Override
    public void characters(CodePoints chars, Location locationId, int properties) throws XPathException {
        if (currentElementChecker != null) {
            buffer.accept(chars);
        }
        nextReceiver.characters(chars, locationId, properties);
        textLocationId = locationId.saveLocation();
    }

    /**
     * End of element
     */

    @Override
    public void endElement() throws XPathException {
        if (currentElementChecker != null) {
            currentElementChecker.checkValue(buffer.toUnicodeString(), textLocationId);
        }
        buffer.clear();
        currentElementChecker = null;
        nextReceiver.endElement();
    }
    
    /**
     * Classify a type as one of ID, IDREF, IDREFS, or other.
     * This is only done the first type a type is encountered, to avoid repeated searching of the
     * type hierarchy
     *
     * @param type the type to be classified
     * @return the classification, as one of xs:ID, xs:IDREF, or xs:IDREFS
     */

    private EntityChecker allocateChecker(SchemaType type) throws MissingComponentException {
        int fingerprint = type.getFingerprint();
        switch (fingerprint) {
            case StandardNames.XS_ENTITY:
                return new SimpleEntityChecker();
            case StandardNames.XS_ENTITIES:
                return new EntityListChecker();
            default:
                // no system-defined types are subtypes of these three
                if (fingerprint < 1023) {
                    return SINGLETON_NON_ENTITY_CHECKER;
                }
                // for user-defined types, we need to check whether it's a subtype of one of these
                Configuration config = getConfiguration();
                TypeHierarchy th = config.getTypeHierarchy();
                if (type.isComplexType()) {
                    return SINGLETON_NON_ENTITY_CHECKER;
                }
                SimpleType stype = (SimpleType) type;
                if (type.isAtomicType()) {
                    if (th.isSubType((AtomicType) type, BuiltInAtomicType.ENTITY)) {
                        return new SimpleEntityChecker();
                    } else {
                        return SINGLETON_NON_ENTITY_CHECKER;
                    }
                } else if (stype.isListType()) {
                    // See spec bug 10662: should a user-defined list of ENTITY be checked or not?
                    if (isEntityType(stype, th)) {
                        return new EntityListChecker();
                    } else {
                        return SINGLETON_NON_ENTITY_CHECKER;
                    }
                } else {
                    // type is a union type
                    if (isEntityType(stype, th)) {
                        return new UnionChecker(stype);
                    } else {
                        return SINGLETON_NON_ENTITY_CHECKER;
                    }
                }
        }
    }

    /**
     * Ask whether a given simple type contains references to DTD-defined entities
     * @param type the type in question
     * @param th the type hierarchy cache
     * @return true if the type is xs:ENTITY, or a restriction thereof, or a list or union type
     * derived from xs:ENTITY, including the built-in list type xs:ENTITIES
     */

    private static boolean isEntityType(SimpleType type, TypeHierarchy th) throws MissingComponentException {
        if (type.isAtomicType()) {
            return th.isSubType((AtomicType) type, BuiltInAtomicType.ENTITY);
        } else if (type.isListType()) {
            return isEntityType(((ListType) type).getItemType(), th);
        } else if (type.isUnionType()) {
            UserUnionType union = (UserUnionType) type;
            for (TypeReference ref : union.getMemberTypeReferences()) {
                SimpleType mem = (SimpleType) ref.getTarget();
                if (isEntityType(mem, th)) {
                    return true;
                }
            }
            return false;
        } else {
            // probably anySimpleType
            return false;
        }
    }

    /**
     * Ask whether this Receiver (or the downstream pipeline) makes any use of the type annotations
     * supplied on element and attribute events
     *
     * @return true if the Receiver makes any use of this information. If false, the caller
     *         may supply untyped nodes instead of supplying the type annotation
     */

    @Override
    public boolean usesTypeAnnotations() {
        return true;
    }

    /**
     * Process a string that has been identified as an entity value
     *
     * @param eref       the entity reference value
     * @param locationId identifies the location for error reporting purposes
     * @throws net.sf.saxon.trans.XPathException
     *          if the entity has not been declared
     */

    private void processEntityRef(String eref, Location locationId) throws XPathException {
        if (!declaredEntities.contains(eref)) {
            ValidationFailure ve = new ValidationFailure("Value '" + eref + "' is not declared in the DTD as an unparsed entity name");
            ve.setConstraintReference(1, "cvc-simple-type", "3");
            reportValidationError(ve, false, locationId);
            //throw ve.makeException();
        }
    }

    private interface EntityChecker {

        /**
         * Check an ID, IDREF, or IDREFS value
         *
         * @param value      the value to be checked
         * @param locationId the location where the value appears
         * @throws net.sf.saxon.trans.XPathException
         *          if the value is invalid
         */

        void checkValue(UnicodeString value, Location locationId) throws XPathException;
    }

    /**
     * IdValueChecker for simple values of type xs:ENTITY, or a type derived therefrom by restriction
     */

    private class SimpleEntityChecker implements EntityChecker {
        @Override
        public void checkValue(UnicodeString value, Location locationId) throws XPathException {
            String id = Whitespace.trim(value).toString();
            processEntityRef(id, locationId);
        }
    }

    /**
     * EntityListChecker for a list type whose values are all instances of xs:ENTITY or a type derived therefrom
     * by restriction
     */

    private class EntityListChecker implements EntityChecker {
        @Override
        public void checkValue(UnicodeString value, Location locationId) throws XPathException {
            String ids = value.toString();
            StringTokenizer tok = new StringTokenizer(ids, " \t\n\r", false);
            while (tok.hasMoreTokens()) {
                String id = tok.nextToken();
                processEntityRef(id, locationId);
            }
        }
    }


    /**
     * IdValueChecker for a general list or union type that includes a member type derived from xs:ENTITY,
     * and/or other member types
     */

    private class UnionChecker implements EntityChecker {
        private final SimpleType type;

        public UnionChecker(SimpleType type) {
            this.type = type;
        }

        @Override
        public void checkValue(UnicodeString value, Location locationId) throws XPathException {
            Configuration config = getConfiguration();
            TypeHierarchy th = config.getTypeHierarchy();
            SequenceIterator iter = type.getTypedValue(
                    value, DummyNamespaceResolver.getInstance(), config.getConversionRules()).iterate();
            AtomicValue val;
            while ((val = (AtomicValue)iter.next()) != null) {
                AtomicType itemType = val.getItemType();
                if (isEntityType(itemType, th)) {
                    processEntityRef(val.getStringValue(), locationId);
                }
            }
        }
    }

    /**
     * IdValueChecker for types that do not contain any ID or IDREF values
     */

    private static class NonIdChecker implements EntityChecker {
        @Override
        public void checkValue(UnicodeString value, Location locationId) throws XPathException {
            // do nothing
        }
    }

    private static final NonIdChecker SINGLETON_NON_ENTITY_CHECKER = new NonIdChecker();

}

// Copyright � 2020 Saxonica Limited No warranty.


