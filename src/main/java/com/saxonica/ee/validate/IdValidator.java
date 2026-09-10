package com.saxonica.ee.validate;

import net.sf.saxon.Configuration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.instruct.DummyNamespaceResolver;
import net.sf.saxon.expr.number.AbstractNumberer;
import net.sf.saxon.expr.number.Numberer_en;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpDelegate;
import net.sf.saxon.tree.util.IndexedStack;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Whitespace;

import java.util.*;


/**
 * This class validates that ID elements and attributes have unique values and that
 * IDREF and IDREFS elements and attributes refer to an ID in the same document. The
 * class assumes that these elements and attributes have already been locally validated
 * (so, for example, a element of type ID will not have child elements).
 */


public class IdValidator extends ValidatingFilter {

    // An element is allowed to have multiple ID values (in XSD 1.0, via multiple child ID elements;
    // in XSD 1.1, also via multiple ID attributes. It is also permissible for several ID values on
    // the same element to be the same value. So duplicate ID values are not an error if they relate
    // to the same element. Therefore, we don't just maintain the set of ID values encountered, we
    // maintain (conceptually) a mapping from ID values to element nodes. However, the only elements
    // we are interested in are those that are currently "on the stack". For these elements we maintain
    // the set of ID values associated with the element, and we check this list to ensure that two
    // IDs associated with the same element are not considered to conflict.

    private final IndexedStack<Set<String>> stack = new IndexedStack<>();
    private int level = 0;
    private final HashSet<String> idValues = new HashSet<>(100);
    private final HashSet<String> unresolvedRefs = new HashSet<>(100);
    /*@Nullable*/ private IdValueChecker currentElementChecker = null;
    private final UnicodeBuilder buffer = new UnicodeBuilder();
    private Location textLocationId = Loc.NONE;
    private final Map<SchemaType, IdValueChecker> checkerMap = new HashMap<>(10);

    /**
     * Create a validator for ID/IDREF constraints
     *
     * @param next the next receiver in the pipeline
     */

    public IdValidator(Receiver next) {
        super(next);
    }

    /**
     * Notify the start of an element
     */

    @Override
    public void startElement(NodeName elemName, SchemaType typeCode,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties)
            throws XPathException {
        currentElementChecker = checkerMap.get(typeCode);
        if (currentElementChecker == null) {
            currentElementChecker = allocateChecker(typeCode);
            checkerMap.put(typeCode, currentElementChecker);
        }
        if (currentElementChecker == NonIdChecker) {
            currentElementChecker = null;
        }
        stack.push(new HashSet<String>(4));
        level++;

        for (AttributeInfo att : attributes) {
            SimpleType type = att.getType();
            IdValueChecker checker = checkerMap.get(type);
            if (checker == null) {
                checker = allocateChecker(type);
                checkerMap.put(type, checker);
            }
            checker.checkValue(this, StringView.of(att.getValue()).tidy(), att.getLocation());
        }

        nextReceiver.startElement(elemName, typeCode, attributes, namespaces, location, properties);
    }

    /**
     * Character data
     */

    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
        if (currentElementChecker != null) {
            buffer.accept(chars);
        }
        nextReceiver.characters(chars, locationId, properties);
        textLocationId = locationId;
    }

    /**
     * End of element
     */

    @Override
    public void endElement() throws XPathException {
        level--;
        stack.pop();
        if (currentElementChecker != null) {
            currentElementChecker.checkValue(this, buffer.toUnicodeString(), textLocationId);
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

    private IdValueChecker allocateChecker(SchemaType type) throws MissingComponentException {
        int fingerprint = type.getFingerprint();
        switch (fingerprint) {
            case StandardNames.XS_ID:
                return SimpleIdChecker;
            case StandardNames.XS_IDREF:
                return SimpleIdRefChecker;
            case StandardNames.XS_IDREFS:
                return IdRefListChecker;
            default:
                // no system-defined types are subtypes of these three
                if (fingerprint < 1023) {
                    return NonIdChecker;
                }
                // for user-defined types, we need to check whether it's a subtype of one of these
                Configuration config = getConfiguration();
                TypeHierarchy th = config.getTypeHierarchy();
                SimpleType stype;
                if (type.isComplexType()) {
                    if (((ComplexType) type).isSimpleContent()) {
                        // See bug 12014. XSD 1.1 explicitly allows ID values for a complex type with simple content;
                        // there is WG consensus that XSD 1.0 intended to allow this, even though the wording states
                        // that it must be a simple type.
                        stype = ((ComplexType) type).getSimpleContentType();
                        assert stype != null;
                    } else {
                        return NonIdChecker;
                    }
                } else {
                    stype = (SimpleType) type;
                }
                if (type.isAtomicType()) {
                    if (type.isIdType()) {
                        return SimpleIdChecker;
                    } else if (type.isIdRefType()) {
                        return SimpleIdRefChecker;
                    } else {
                        return NonIdChecker;
                    }
                } else if (stype.isListType()) {
                    SimpleType componentType = ((ListType) stype).getItemType();
                    if (componentType instanceof AtomicType && th.isSubType((AtomicType) componentType, BuiltInAtomicType.ID)) {
                        return IdListChecker;
                    } else if (componentType instanceof AtomicType && th.isSubType((AtomicType) componentType, BuiltInAtomicType.IDREF)) {
                        return IdRefListChecker;
                    } else if (type.isIdType() || type.isIdRefType()) {
                        return getUnionChecker(stype);   // this handles anything
                    } else {
                        return NonIdChecker;
                    }
                } else {
                    if (type.isIdType() || type.isIdRefType()) {
                        return getUnionChecker(stype);
                    } else {
                        return NonIdChecker;
                    }
                }
        }
    }

    /**
     * End of document
     */

    @Override
    public void endDocument() throws XPathException {
        checkUnresolvedRefs();  // bug 2403
        nextReceiver.endDocument();
    }


    @Override
    public void close() throws XPathException {
        //checkUnresolvedRefs();
        nextReceiver.close();
    }

    private void checkUnresolvedRefs() throws XPathException {
        if (!unresolvedRefs.isEmpty()) {
            int count = unresolvedRefs.size();
            String howMany;
            String value;
            if (count == 1) {
                howMany = " is one";
                value = "value";
            } else {
                Numberer_en num = new Numberer_en();
                num.setTensUnitsSeparatorCardinal("-");
                howMany = " are " + num.toWords(count, AbstractNumberer.LOWER_CASE);
                value = "values";
            }
            StringBuilder err =
                    new StringBuilder("There" + howMany + " IDREF " + value + " with no corresponding ID");
            if (count > 10) {
                err.append(". First ten:");
            } else {
                err.append(':');
            }
            int i = 0;
            for (String id : unresolvedRefs) {
                err.append("\n    ");
                err.append(id);
                if (i++ > 9) {
                    break;
                }
            }
            ValidationFailure ve = new ValidationFailure(err.toString());
            ve.setErrorCode(getPipelineConfiguration().isXSLT() ? "XTTE1555" : "XQDY0027");
            ve.setConstraintReference(1, "cvc-id", "1");
            reportValidationError(ve, Loc.NONE);
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
     * Report a validation error
     *
     * @param err        The validation exception
     * @param locationId identifies the location in the source document or stylesheet/query where the
     *                   validation error was detected
     * @throws ValidationException if the error is determined to be fatal
     */
    protected void reportValidationError(ValidationFailure err, Location locationId)
            throws XPathException {
        PipelineConfiguration pipe = getPipelineConfiguration();
        err.setErrorCode(pipe.isXSLT() ? "XTTE1555" : "XQDY0027");
        super.reportValidationError(err, false, locationId);

    }

    /**
     * Process a string that has been identified as an ID value
     *
     * @param id         the ID value
     * @param locationId the location for error reporting purposes
     * @throws XPathException if an error is detected
     */

    private void processId(String id, Location locationId) throws XPathException {
        if (level == 0) {
            // we have a parentless element classed as an xs:ID value. Under the resolution of
            // bug 9922, this is considered to be invalid.
            ValidationFailure ve = new ValidationFailure(
                    "An element of type xs:ID must have a parent element within the tree being validated");
            ve.setErrorCode(getPipelineConfiguration().isXSLT() ? "XTTE1555" : "XQDY0027");
            ve.setConstraintReference(1, "cvc-id", "2");
            reportValidationError(ve, locationId);
            throw ve.makeException();
        }
        if (idValues.contains(id)) {
            // check that this isn't a second occurrence of the same ID for the same element
            Set<String> idsForElement = stack.get(level - 1);
            if (getConfiguration().getXsdVersion() == Configuration.XSD10 || !idsForElement.contains(id)) {
                ValidationFailure ve = new ValidationFailure("ID value '" + id + "' is not unique");
                ve.setErrorCode(getPipelineConfiguration().isXSLT() ? "XTTE1555" : "XQDY0027");
                ve.setConstraintReference(1, "cvc-id", "2");
                reportValidationError(ve, locationId);
                //throw ve.makeException();
            }
        } else {
            idValues.add(id);
            if (level >= 1) {
                // See spec bug 9922 concerning a parentless xs:ID element
                stack.get(level-1).add(id);
            }
        }
        unresolvedRefs.remove(id);
    }

    @CSharpDelegate(true)
    @FunctionalInterface
    private interface IdValueChecker {

        /**
         * Check an ID, IDREF, or IDREFS value
         *
         *
         * @param validator  the containing validator
         * @param value      the value to be checked
         * @param locationId the location where the value appears
         * @throws XPathException if the value is invalid
         */

        void checkValue(IdValidator validator, UnicodeString value, Location locationId) throws XPathException;
    }

    /**
     * IdValueChecker for simple values of type xs:ID, or a type derived therefrom by restriction
     */

    private final static IdValueChecker SimpleIdChecker = (validator, value, locationId) ->
            validator.processId(Whitespace.trim(value).toString(), locationId);

    /**
     * IdValueChecker for simple values of type xs:IDREF, or a type derived therefrom by restriction
     */

    private final static IdValueChecker SimpleIdRefChecker = (validator, value, locationId) -> {
            String id = Whitespace.trim(value).toString();
            // If the value is empty, it must be nilled: see bug #2301
            if (!id.isEmpty() && !validator.idValues.contains(id)) {
                validator.unresolvedRefs.add(id);
            }
    };

    /**
     * IdValueChecker for a list type whose values are all instances of xs:ID or a type derived therefrom
     * by restriction
     */
    private final static IdValueChecker IdListChecker = (validator, value, locationId) -> {
            String ids = value.toString();
            StringTokenizer tok = new StringTokenizer(ids, " \t\n\r", false);
            while (tok.hasMoreTokens()) {
                String id = tok.nextToken();
                validator.processId(id, locationId);
            }
    };

    /**
     * IdValueChecker for a list type whose values are all instances of xs:IDREF or a type derived therefrom
     * by restriction. This includes the built-in type xs:IDREFS.
     */

    private final static IdValueChecker IdRefListChecker = (validator, value, locationId) -> {
            String ids = value.toString();
            StringTokenizer tok = new StringTokenizer(ids, " \t\n\r", false);
            while (tok.hasMoreTokens()) {
                String id = tok.nextToken();
                if (!validator.idValues.contains(id)) {
                    validator.unresolvedRefs.add(id);
                }
            }
    };

    /**
     * IdValueChecker for a general list or union type that includes a member type derived from xs:ID, and/or
     * a member type derived form xs:IDREF, and/or other member types
     */

    private IdValueChecker getUnionChecker(SimpleType type) {
        return (validator, value, locationId) -> {
            Configuration config = getConfiguration();
            TypeHierarchy th = config.getTypeHierarchy();
            AtomicSequence typedValue = type.getTypedValue(
                    value, DummyNamespaceResolver.getInstance(), config.getConversionRules());
            for (AtomicValue val: typedValue) {
                AtomicType itemType = val.getItemType();
                if (th.isSubType(itemType, BuiltInAtomicType.ID)) {
                    processId(val.getStringValue(), locationId);
                } else if (th.isSubType(itemType, BuiltInAtomicType.IDREF)) {
                    String idref = val.getStringValue();
                    if (!idValues.contains(idref)) {
                        unresolvedRefs.add(idref);
                    }
                }
            }
        };
    }

    /**
     * IdValueChecker for types that do not contain any ID or IDREF values
     */

    private static final IdValueChecker NonIdChecker = (validator, value, locationId) -> {};

}

// Copyright (c) 2004-2022 Saxonica Limited


