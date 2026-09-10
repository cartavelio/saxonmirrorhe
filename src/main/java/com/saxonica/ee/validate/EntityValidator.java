package com.saxonica.ee.validate;

import com.saxonica.ee.schema.TypeReference;
import com.saxonica.ee.schema.UserUnionType;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.instruct.DummyNamespaceResolver;
import net.sf.saxon.expr.parser.ExplicitLocation;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.FastStringBuffer;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Whitespace;

import javax.xml.transform.ErrorListener;
import javax.xml.transform.TransformerException;
import java.util.*;


/**
 * This class validates that elements and attributes of type ENTITY or ENTITIES have values are
 * declared in the DOCTYPE declaration of the instance document as unparsed entities.
 * <p>This component of the validation pipeline is used during standalone schema validation, but
 * not during validation invoked from XSLT or XQuery, as constructed trees never contain
 * unparsed entities.</p>
 */


public class EntityValidator extends ValidatingFilter {

    private Set<String> declaredEntities = new HashSet<String>(10);

    /*@Nullable*/ private EntityChecker currentElementChecker = null;
    private FastStringBuffer buffer = new FastStringBuffer(FastStringBuffer.C64);
    private Location textLocationId = ExplicitLocation.UNKNOWN_LOCATION;
    private Map<SchemaType, EntityChecker> checkerMap = new HashMap<SchemaType, EntityChecker>(10);

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
     *  @param nameCode   integer code identifying the name of the element within the name pool.
     * @param typeCode   integer code identifying the element's type within the name pool.
     * @param location
     * @param properties for future use. Should be set to zero.
     */

    public void startElement(NodeName nameCode, SchemaType typeCode, Location location, int properties) throws XPathException {
        currentElementChecker = checkerMap.get(typeCode);
        if (currentElementChecker == null) {
            currentElementChecker = allocateChecker(typeCode);
            checkerMap.put(typeCode, currentElementChecker);
        }
        if (currentElementChecker == SINGLETON_NON_ENTITY_CHECKER) {
            currentElementChecker = null;
        }
        nextReceiver.startElement(nameCode, typeCode, location, properties);
    }

    /**
     * Character data
     */

    public void characters(CharSequence chars, Location locationId, int properties) throws XPathException {
        if (currentElementChecker != null) {
            buffer.append(chars);
        }
        nextReceiver.characters(chars, locationId, properties);
        textLocationId = locationId.saveLocation();
    }

    /**
     * End of element
     */

    public void endElement() throws XPathException {
        if (currentElementChecker != null) {
            currentElementChecker.checkValue(buffer, textLocationId);
        }
        buffer.setLength(0);
        currentElementChecker = null;
        nextReceiver.endElement();
    }

    /**
     * Handle an attribute
     *
     * @param nameCode   integer identifying the name of the attribute
     * @param typeCode   integer identifying the type annotation (ignored)
     * @param value      the value of the attribute
     * @param locationId the location of the node in the source, or of the instruction that created it
     *@param properties additional properties of the attribute  @throws net.sf.saxon.trans.XPathException
     *          On any failure to write the attribute
     */

    public void attribute(NodeName nameCode, SimpleType typeCode, CharSequence value, Location locationId, int properties)
            throws XPathException {
        EntityChecker checker = checkerMap.get(typeCode);
        if (checker == null) {
            checker = allocateChecker(typeCode);
            checkerMap.put(typeCode, checker);
        }
        checker.checkValue(value, locationId);
        nextReceiver.attribute(nameCode, typeCode, value, locationId, properties);
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

    public boolean usesTypeAnnotations() {
        return true;
    }

 /*   *//**
     * Report a validation error
     *
     * @param err        The validation exception
     * @param locationId identifies the location in the source document or stylesheet/query where the
     *                   validation error was detected
     * @throws net.sf.saxon.type.ValidationException
     *          if the error is considered fatal
     *//*
    protected void reportValidationError(ValidationException err, Location locationId)
            throws ValidationException {
        PipelineConfiguration pipe = getPipelineConfiguration();
        err.setLocation(locationId.saveLocation());
        ErrorListener listener = pipe.getErrorListener();
        try {
            listener.fatalError(err);
            err.setHasBeenReported(true);
        } catch (TransformerException e) {
            err.setHasBeenReported(true);
            if (e instanceof ValidationException) {
                throw (ValidationException) e;
            } else if (e.getException() instanceof ValidationException) {
                throw (ValidationException) e.getException();
            } else {
                throw new ValidationException(e);
            }
        }
    } */

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
            throw ve.makeException();
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

        void checkValue(CharSequence value, Location locationId) throws XPathException;
    }

    /**
     * IdValueChecker for simple values of type xs:ENTITY, or a type derived therefrom by restriction
     */

    private class SimpleEntityChecker implements EntityChecker {
        public void checkValue(CharSequence value, Location locationId) throws XPathException {
            String id = Whitespace.trimWhitespace(value).toString();
            processEntityRef(id, locationId);
        }
    }

    /**
     * EntityListChecker for a list type whose values are all instances of xs:ENTITY or a type derived therefrom
     * by restriction
     */

    private class EntityListChecker implements EntityChecker {
        public void checkValue(CharSequence value, Location locationId) throws XPathException {
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
        private SimpleType type;

        public UnionChecker(SimpleType type) {
            this.type = type;
        }

        public void checkValue(CharSequence value, Location locationId) throws XPathException {
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
        public void checkValue(CharSequence value, Location locationId) throws XPathException {
            // do nothing
        }
    }

    private static NonIdChecker SINGLETON_NON_ENTITY_CHECKER = new NonIdChecker();

}

// Copyright � 2010 Saxonica Limited. No warranty.


