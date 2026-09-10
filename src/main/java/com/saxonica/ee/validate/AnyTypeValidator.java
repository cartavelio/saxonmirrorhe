package com.saxonica.ee.validate;

import com.saxonica.ee.schema.ElementDecl;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;

import net.sf.saxon.type.AnyType;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.ValidationFailure;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.str.UnicodeBuilder;


/**
 * Validator to validate an element against the class xs:anyType
 */

public class AnyTypeValidator extends LaxValidator {

    // The fixed value, if any, converted to a string
    UnicodeString fixedValue = null;

    // The default value, if any
    UnicodeString defaultValue = null;

    // Flag to indicate that child elements have been found
    boolean foundChildren = false;

    // Buffer for accumulating character content (only used when there is a fixed value)
    UnicodeBuilder buffer;

    /**
     * Create an AnyTypeValidator
     *
     * @param next the next receiver in the pipeline
     */

    public AnyTypeValidator(Receiver next) {
        super(next);
    }

    /**
     * Create an AnyTypeValidator for a specific element declaration
     *
     * @param elementDecl the element declaration. Ignored if null.
     * @param next        the next receiver in the pipeline
     */

    public AnyTypeValidator(/*@Nullable*/ ElementDecl elementDecl, Receiver next) {
        super(next);
        if (elementDecl != null) {
            AtomicSequence fixed = elementDecl.getFixedValue();
            if (fixed != null) {
                //noinspection RedundantCast
                fixedValue = ((AtomicValue) fixed).getUnicodeStringValue();
                // cast needed to show there's no exception thrown
            }
            defaultValue = elementDecl.getDefaultValueLexicalForm();
        }
    }

    /**
     * Handle the start tag for a child element of the element being validated
     */

    @Override
    public void startElement(NodeName elemName, SchemaType type, AttributeMap attributes, NamespaceMap namespaces, Location location, int properties) throws XPathException {

        if (locallyInvalid) {
            // already found errors and warned about them
            childValidator = new SkipValidator(nextReceiver);
            nextReceiver.startElement(elemName, AnyType.getInstance(), attributes, namespaces, location, properties);
            return;
        }

        if (fixedValue != null) {
            String message = "Element " +
                    Err.wrap(elemName.getDisplayName(), Err.ELEMENT) +
                    " cannot appear here, because the containing " +
                    getContainingElementName() +
                    " element has fixed content";
            ValidationFailure ve = new ValidationFailure(message);
            ve.setConstraintReference(1, "cvc-elt", "5.2.2.1");
            ve.setSchemaType(getSchemaType());
            reportValidationError(ve, true, location);
        }

        foundChildren = true;
        super.startElement(elemName, type, attributes, namespaces, location, properties);
    }

    /**
     * Handle character data appearing as a child of the element being validated
     * @param chars      The character content
     * @param locationId the location of the node in the source, or of the instruction that created it
     * @param properties Additional properties  @throws XPathException If this element does not allow character data
     */

    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
        if (fixedValue != null || defaultValue != null) {
            if (buffer == null) {
                buffer = new UnicodeBuilder();
            }
            buffer.accept(chars);
        }
        super.characters(chars, locationId, properties);
    }

    /**
     * Validation at the end of the element being validated. (Note this is a different element from
     * the one passed to startElement).
     *
     * @throws XPathException if the element is invalid
     */

    @Override
    public void endElement() throws XPathException {
        if (fixedValue != null) {
            if (buffer == null || buffer.isEmpty()) {
                characters(fixedValue, Loc.NONE, ReceiverOption.NONE);
            } else if (!buffer.toUnicodeString().equals(fixedValue)) {
                String message = "The content of " +
                        getContainingElementName() +
                        " differs from the fixed value defined in the schema. Fixed value is " +
                        Err.wrap(fixedValue, Err.VALUE) +
                        ", actual value is " +
                        Err.wrap(buffer.toString(), Err.VALUE);
                ValidationFailure ve = new ValidationFailure(message);
                ve.setConstraintReference(1, "cvc-elt", "5.2.2.2");
                ve.setSchemaType(getSchemaType());
                reportValidationError(ve, true, getContainingElementLocationId());
            }
        }
        if (defaultValue != null && !foundChildren) {
            if (buffer == null || buffer.isEmpty()) {
                characters(defaultValue, Loc.NONE, ReceiverOption.NONE);
            }
        }

        super.endElement();
    }

}

// Copyright (c) 2004-2022 Saxonica Limited