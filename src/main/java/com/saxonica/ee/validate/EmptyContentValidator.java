package com.saxonica.ee.validate;

import com.saxonica.ee.schema.ElementDecl;
import com.saxonica.ee.schema.UserComplexType;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceMap;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AnyType;
import net.sf.saxon.type.MissingComponentException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.ValidationFailure;
import net.sf.saxon.str.UnicodeBuilder;

/**
 * This class is a filter that is used to validate that an element has empty content.
 * The element is allowed to have attributes, these are validated by the Validator
 * superclass.
 */


public class EmptyContentValidator extends AttributeValidator {

    private final boolean allowTextNodes;
    private Location locationId;
    /*@Nullable*/ private UnicodeBuilder buffer;
    private final SchemaType schemaType;

    /**
     * Create a validator for an empty content model
     *
     * @param type the empty content type
     * @param next the next receiver in the pipeline
     * @throws MissingComponentException if the schema is incomplete or incorrect
     */

    public EmptyContentValidator(UserComplexType type, Receiver next) throws MissingComponentException {
        super(next);
        schemaType = type;
        setAttributeGroup(type.getCombinedAttributeGroup());
        allowTextNodes = type.isMixedContent();
        setAnnotation(type);
    }

    /**
     * Get the schema type against which we are validating, if any
     *
     * @return the schema type that this validator is validating against, or null
     */

    @Override
    public SchemaType getSchemaType() {
        return schemaType;
    }

    @Override
    protected ContentValidator getChildValidator() {
        // error paths only
        return new AnyTypeValidator(getNextReceiver());
    }

    /**
     * Output element start tag (for a child of the element being validated)
     */

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) throws XPathException {
        this.locationId = location.saveLocation();
        String message = "Invalid child element " +
                Err.wrap(elemName.getDisplayName(), Err.ELEMENT) +
                ", " + getContainingElementName() + " has an empty content model";
        ValidationFailure ve = new ValidationFailure(message);
        ve.setConstraintReference(1, "cvc-complex-type", "2.1");
        ve.setSchemaType(getSchemaType());
        reportValidationError(ve, true, location);
        buffer = null;
        super.startElement(elemName, AnyType.getInstance(), attributes, namespaces, location, properties);
    }

    /**
     * Character data
     */

    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
        checkNoCharactersWhenNil(locationId);
        if (allowTextNodes) {
            ElementDecl decl = getElementDeclaration();
            if (decl != null && (decl.getFixedValue() != null || decl.getDefaultValueLexicalForm() != null)) {
                if (buffer == null) {
                    buffer = new UnicodeBuilder();
                }
                buffer.accept(chars);
            }
        } else {
            String message = "Character data is not allowed: " + getContainingElementName() +
                    " has an empty content model";
            ValidationFailure ve = new ValidationFailure(message);
            ve.setConstraintReference(1, "cvc-complex-type", "2.1");
            ve.setSchemaType(getSchemaType());
            reportValidationError(ve, true, locationId);
        }
        nextReceiver.characters(chars, locationId, properties);
    }

    /**
     * End of element
     */

    @Override
    public void endElement() throws XPathException {
        if (isNilled()) {
            ElementDecl decl = getElementDeclaration();
            if (decl != null && decl.getFixedValue() != null) {
                String message = "An element with xsi:nil='true' must not have a fixed value";
                ValidationFailure ve = new ValidationFailure(message);
                ve.setConstraintReference(1, "cvc-elt", "3.2.2");
                ve.setSchemaType(getSchemaType());
                reportValidationError(ve, true, locationId);
            }
        }
        if (allowTextNodes) {
            ElementDecl decl = getElementDeclaration();
            if (decl != null) {
                UnicodeString defaultValue = decl.getDefaultValueLexicalForm();
                UnicodeString fixedValue = decl.getFixedValueLexicalForm();
                if (defaultValue != null && (buffer == null || buffer.isEmpty())) {
                    characters(defaultValue, Loc.NONE, ReceiverOption.NONE);
                } else if (fixedValue != null) {
                    UnicodeBuilder fsb = buffer;
                    if (fsb == null || fsb.isEmpty()) {
                        // if no value was supplied, "fixed" is treated as "default"
                        characters(fixedValue, Loc.NONE, ReceiverOption.NONE);
                    } else if (!fsb.toUnicodeString().equals(fixedValue)) {
                        String message = "The content " + Err.wrap(buffer.toString(), Err.VALUE) + " of " +
                                getContainingElementName() +
                                " does not match the fixed value " +
                                Err.wrap(fixedValue, Err.VALUE);
                        ValidationFailure ve = new ValidationFailure(message);
                        ve.setConstraintReference(1, "cvc-elt", "5.2.2.2");
                        ve.setSchemaType(getSchemaType());
                        reportValidationError(ve, true, getContainingElementLocationId());
                    }
                }
            }
        }
        super.endElement();
    }

}

// Copyright (c) 2004-2022 Saxonica Limited
