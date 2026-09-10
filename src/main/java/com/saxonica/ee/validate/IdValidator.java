package com.saxonica.ee.validate;

import net.sf.saxon.Configuration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.instruct.DummyNamespaceResolver;
import net.sf.saxon.expr.number.AbstractNumberer;
import net.sf.saxon.expr.number.Numberer_en;
import net.sf.saxon.expr.parser.ExplicitLocation;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.FastStringBuffer;
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

    private Stack<Set<String>> stack = new Stack<>();
    private int level = 0;
    private HashSet<String> idValues = new HashSet<>(100);
    private HashSet<String> unresolvedRefs = new HashSet<>(100);
    /*@Nullable*/ private IdValueChecker currentElementChecker = null;
    private FastStringBuffer buffer = new FastStringBuffer(FastStringBuffer.C64);
    private Location textLocationId = ExplicitLocation.UNKNOWN_LOCATION;
    private Map<SchemaType, IdValueChecker> checkerMap = new HashMap<>(10);

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
     * @param nameCode  the name of the element.
     * @param typeCode the element's type.
     * @param location  the element's location (or the location of the instruction that created it)
     * @param properties for future use. Should be set to zero.
     */

    public void startElement(NodeName nameCode, SchemaType typeCode, Location location, int properties) throws XPathException {
        currentElementChecker = checkerMap.get(typeCode);
        if (currentElementChecker == null) {
            currentElementChecker = allocateChecker(typeCode);
            checkerMap.put(typeCode, currentElementChecker);
        }
        if (currentElementChecker == SINGLETON_NON_ID_CHECKER) {
            currentElementChecker = null;
        }
        stack.push(new HashSet<>(4));
        level++;
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
        textLocationId = locationId;
    }

    /**
     * End of element
     */

    public void endElement() throws XPathException {
        level--;
        stack.pop();
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
     * @param type       the type annotation
     * @param value      the value of the attribute
     * @param locationId the location of the node in the source, or of the instruction that created it
     *@param properties additional properties of the attribute  @throws XPathException On any failure to write the attribute
     */

    public void attribute(NodeName nameCode, SimpleType type, CharSequence value, Location locationId, int properties)
            throws XPathException {
        IdValueChecker checker = checkerMap.get(type);
        if (checker == null) {
            checker = allocateChecker(type);
            checkerMap.put(type, checker);
        }
        checker.checkValue(value, locationId);
        nextReceiver.attribute(nameCode, type, value, locationId, properties);
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
                return new SimpleIdChecker();
            case StandardNames.XS_IDREF:
                return new SimpleIdrefChecker();
            case StandardNames.XS_IDREFS:
                return new IdrefListChecker();
            default:
                // no system-defined types are subtypes of these three
                if (fingerprint < 1023) {
                    return SINGLETON_NON_ID_CHECKER;
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
                        return SINGLETON_NON_ID_CHECKER;
                    }
                } else {
                    stype = (SimpleType) type;
                }
                if (type.isAtomicType()) {
                    if (type.isIdType()) {
                        return new SimpleIdChecker();
                    } else if (type.isIdRefType()) {
                        return new SimpleIdrefChecker();
                    } else {
                        return SINGLETON_NON_ID_CHECKER;
                    }
                } else if (stype.isListType()) {
                    SimpleType componentType = ((ListType) stype).getItemType();
                    if (componentType instanceof AtomicType && th.isSubType((AtomicType) componentType, BuiltInAtomicType.ID)) {
                        return new IdListChecker();
                    } else if (componentType instanceof AtomicType && th.isSubType((AtomicType) componentType, BuiltInAtomicType.IDREF)) {
                        return new IdrefListChecker();
                    } else if (type.isIdType() || type.isIdRefType()) {
                        return new UnionChecker(stype);   // this handles anything
                    } else {
                        return SINGLETON_NON_ID_CHECKER;
                    }
                } else {
                    if (type.isIdType() || type.isIdRefType()) {
                        return new UnionChecker(stype);
                    } else {
                        return SINGLETON_NON_ID_CHECKER;
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
            int hostLanguage = getPipelineConfiguration().getHostLanguage();
            if (hostLanguage == Configuration.XSLT) {
                ve.setErrorCode("XTTE1555");
            } else if (hostLanguage == Configuration.XQUERY) {
                ve.setErrorCode("XQDY0027");
            }
            ve.setConstraintReference(1, "cvc-id", "1");
            reportValidationError(ve, ExplicitLocation.UNKNOWN_LOCATION);
            throw ve.makeException();
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
        if (err.getErrorCodeQName() == null) {
            int language = pipe.getHostLanguage();
            if (language == Configuration.XSLT) {
                err.setErrorCode("XTTE1555");
            } else if (language == Configuration.XQUERY) {
                err.setErrorCode("XQDY0027");
            }
        }
        super.reportValidationError(err, false, locationId);

        /*err.setLocator(locationId.saveLocation());
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
        } */
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
            int hostLanguage = getPipelineConfiguration().getHostLanguage();
            if (hostLanguage == Configuration.XSLT) {
                ve.setErrorCode("XTTE1555");
            } else if (hostLanguage == Configuration.XQUERY) {
                ve.setErrorCode("XQDY0027");
            }
            ve.setConstraintReference(1, "cvc-id", "2");
            reportValidationError(ve, locationId);
            throw ve.makeException();
        }
        if (idValues.contains(id)) {
            // check that this isn't a second occurrence of the same ID for the same element
            Set<String> idsForElement = stack.get(level - 1);
            if (getConfiguration().getXsdVersion() == Configuration.XSD10 || !idsForElement.contains(id)) {
                ValidationFailure ve = new ValidationFailure("ID value '" + id + "' is not unique");
                int hostLanguage = getPipelineConfiguration().getHostLanguage();
                if (hostLanguage == Configuration.XSLT) {
                    ve.setErrorCode("XTTE1555");
                } else if (hostLanguage == Configuration.XQUERY) {
                    ve.setErrorCode("XQDY0027");
                }
                ve.setConstraintReference(1, "cvc-id", "2");
                reportValidationError(ve, locationId);
                throw ve.makeException();
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

    private interface IdValueChecker {

        /**
         * Check an ID, IDREF, or IDREFS value
         *
         * @param value      the value to be checked
         * @param locationId the location where the value appears
         * @throws XPathException if the value is invalid
         */

        void checkValue(CharSequence value, Location locationId) throws XPathException;
    }

    /**
     * IdValueChecker for simple values of type xs:ID, or a type derived therefrom by restriction
     */

    private class SimpleIdChecker implements IdValueChecker {
        public void checkValue(CharSequence value, Location locationId) throws XPathException {
            String id = Whitespace.trimWhitespace(value).toString();
            processId(id, locationId);
        }
    }

    /**
     * IdValueChecker for simple values of type xs:IDREF, or a type derived therefrom by restriction
     */

    private class SimpleIdrefChecker implements IdValueChecker {
        public void checkValue(CharSequence value, Location locationId) {
            String id = Whitespace.trimWhitespace(value).toString();
            // If the value is empty, it must be nilled: see bug #2301
            if (!id.isEmpty() && !idValues.contains(id)) {
                unresolvedRefs.add(id);
            }
        }
    }

    /**
     * IdValueChecker for a list type whose values are all instances of xs:ID or a type derived therefrom
     * by restriction
     */

    private class IdListChecker implements IdValueChecker {
        public void checkValue(CharSequence value, Location locationId) throws XPathException {
            String ids = value.toString();
            StringTokenizer tok = new StringTokenizer(ids, " \t\n\r", false);
            while (tok.hasMoreTokens()) {
                String id = tok.nextToken();
                processId(id, locationId);
            }
        }
    }

    /**
     * IdValueChecker for a list type whose values are all instances of xs:IDREF or a type derived therefrom
     * by restriction. This includes the built-in type xs:IDREFS.
     */

    private class IdrefListChecker implements IdValueChecker {
        public void checkValue(CharSequence value, Location locationId) {
            String ids = value.toString();
            StringTokenizer tok = new StringTokenizer(ids, " \t\n\r", false);
            while (tok.hasMoreTokens()) {
                String id = tok.nextToken();
                if (!idValues.contains(id)) {
                    unresolvedRefs.add(id);
                }
            }
        }
    }

    /**
     * IdValueChecker for a general list or union type that includes a member type derived from xs:ID, and/or
     * a member type derived form xs:IDREF, and/or other member types
     */

    private class UnionChecker implements IdValueChecker {
        private SimpleType type;

        public UnionChecker(SimpleType type) {
            this.type = type;
        }

        public void checkValue(CharSequence value, Location locationId) throws XPathException {
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
        }
    }

    /**
     * IdValueChecker for types that do not contain any ID or IDREF values
     */

    private static class NonIdChecker implements IdValueChecker {
        public void checkValue(CharSequence value, Location locationId) {
            // do nothing
        }
    }

    private static NonIdChecker SINGLETON_NON_ID_CHECKER = new NonIdChecker();

}

// Copyright (c) 2004-2014 Saxonica Limited.


