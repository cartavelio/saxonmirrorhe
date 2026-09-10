package com.saxonica.ee.validate;

import com.saxonica.ee.schema.ElementDecl;
import com.saxonica.ee.schema.UserComplexType;
import com.saxonica.ee.schema.ValueConstraint;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.str.UnicodeBuilder;

/**
 * This class is a filter that is used to validate that an element has simple content
 * conforming to a given simple type. This filter accumulates the character data in
 * a buffer and invokes the validation method of the simple type at the time that
 * endElement() is called. Child elements are rejected as invalid. This validator
 * can handle simple types, or complex types with simple content: the attributes
 * are validated by the superclass, Validator.
 */


public class SimpleContentValidator extends AttributeValidator {

    // The schema type that we are validating against: either a simple type, or a complex type with
    // simple content
    private SchemaType schemaType;
    // The simple type: the same as the schemaType if simple, or the simple content type otherwise.
    private SimpleType simpleType;
    // The fixed value, if any
    private ValueConstraint fixedValue = null;
    // The canonical lexical representation of the fixed value, if any
    private UnicodeString fixedValueLexicalForm = null;
    // The Comparable corresponding to the fixed value, if any
    //private Comparable fixedValueComparable = null;
    // The default value, if any
    private ValueConstraint defaultValue = null;
    // Flag indicating that the fixed/default value needs to be validated
    private boolean needToValidateConstraint = false;
    // Buffer to capture element content
    private final UnicodeBuilder buffer = new UnicodeBuilder();

    /**
     * Create a simple content validator
     *
     * @param declaration The element declaration (may be null). Used to check fixed/default values
     * @param type        The schema type of the element
     * @param next        The next receiver in the pipeline
     * @throws net.sf.saxon.type.MissingComponentException
     *          if a validator cannot be constructed because the schema is incomplete
     */

    public SimpleContentValidator(/*@Nullable*/ ElementDecl declaration, SchemaType type, Receiver next) throws MissingComponentException {
        super(next);
        setSchemaType(type);
        if (declaration != null) {
            defaultValue = declaration.getDefaultValueConstraint();
            fixedValue = declaration.getFixedValueConstraint();
            if (fixedValue != null) {
                fixedValueLexicalForm = fixedValue.getLexicalForm();
            }
            setNillability(declaration.isNillable() ? Nillability.ALLOWED : Nillability.DISALLOWED);
            needToValidateConstraint = type != declaration.getType();
        } else {
            needToValidateConstraint = false;
        }
    }

    /**
     * Get the schema type against which we are validating
     *
     * @return the schema type
     */

    @Override
    public SchemaType getSchemaType() {
        return schemaType;
    }

    /**
     * Set the schema type against which this validator is to perform validation
     *
     * @param type the schema type
     * @throws MissingComponentException if an unresolved reference is present
     */

    public void setSchemaType(SchemaType type) throws MissingComponentException {
        schemaType = type;
        if (type.isSimpleType()) {
            simpleType = (SimpleType) type;
        } else {
            // it must be a complex type with simple content
            UserComplexType ctype = (UserComplexType) type;
            simpleType = ctype.getSimpleContentType();
            setAttributeGroup(ctype.getCombinedAttributeGroup());
        }
        setAnnotation(schemaType);
        needToValidateConstraint = true;
    }

    /**
     * Output element start tag (for a child of the element being validated)
     */

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties)
            throws XPathException {
        String message =
                "Invalid child element " +
                        Err.wrap(elemName.getDisplayName(), Err.ELEMENT) +
                        ", " + getContainingElementName() + " must have simple content";
        ValidationFailure ve = new ValidationFailure(message);
        ve.setConstraintReference(1, "cvc-type", "3.1.2");
        ve.setSchemaType(schemaType);
        reportValidationError(ve, true, location);
        super.startElement(elemName, AnyType.getInstance(), attributes, namespaces, location, properties);
    }

    /**
     * Return a validator to handle the children of the element just started.
     * This will only be called during error recovery, because an exception would have been thrown when
     * the element was started
     */

    @Override
    protected ContentValidator getChildValidator() {
        // called on error paths only
        return new AnyTypeValidator(getNextReceiver());
    }

    /**
     * Handle character data appearing as a child of the element being validated
     * @param chars      The character content
     * @param locationId the location of the node in the source, or of the instruction that created it
     * @param properties Additional properties  @throws net.sf.saxon.trans.XPathException
     */


    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
        checkNoCharactersWhenNil(locationId);
        buffer.accept(chars);
        if (getNillability() == Nillability.ALLOWED) {
            properties |= (ReceiverOption.NILLABLE_ELEMENT);
        }
        nextReceiver.characters(chars, locationId, properties);
    }

    /**
     * End of element being validated
     */

    @Override
    public void endElement() throws XPathException {

        // Expand the fixed/default value if appropriate
        if (isNilled()) {
            if (!buffer.isEmpty()) {
                String message = "The " + getContainingElementName() +
                        " has xsi:nil='true' so it cannot have character content";
                ValidationFailure ve = new ValidationFailure(message);
                ve.setSchemaType(schemaType);
                ve.setConstraintReference(1, "cvc-elt", "3.2.1");
                reportValidationError(ve, true, getContainingElementLocationId());
                super.endElement();
                return;
            }
            if (fixedValue != null) {
                String message = "The " + getContainingElementName() +
                        " has a fixed value so it must not be nil";
                ValidationFailure ve = new ValidationFailure(message);
                ve.setConstraintReference(1, "cvc-elt", "3.2.2");
                ve.setSchemaType(schemaType);
                reportValidationError(ve, true, getContainingElementLocationId());
                super.endElement();
                return;
            }
        } else {
            if (defaultValue != null && buffer.isEmpty()) {
                if (needToValidateConstraint) {
                    ValidationFailure err = simpleType.validateContent(
                            defaultValue.getLexicalForm(), getNamespaceResolver(), getConfiguration().getConversionRules());
                    if (err != null) {
                        String message = "The default value " +
                                Err.wrap(defaultValue.getLexicalForm(), Err.VALUE) + " for " + getContainingElementName() +
                                " does not match the type " + simpleType.getDescription() + ": " + err.getMessage();
                        err.setMessage(message);
                        err.setSchemaType(simpleType);
                        err.setConstraintReference(2, "cvc-datatype-valid", "1");
                        reportValidationError(err, true, getContainingElementLocationId());
                        super.endElement();
                        return;
                    }
                }
                characters(defaultValue.getLexicalForm(), Loc.NONE,
                           ReceiverOption.DEFAULTED_VALUE);
            } else if (fixedValue != null) {
                if (buffer.isEmpty()) {
                    // if no value was supplied, "fixed" is treated as "default"
                    if (needToValidateConstraint) {
                        ValidationFailure err = simpleType.validateContent(
                                fixedValueLexicalForm,
                            getNamespaceResolver(), getConfiguration().getConversionRules());
                        if (err != null) {
                            String message = "The fixed value " +
                                Err.wrap(fixedValueLexicalForm, Err.VALUE) + " for " +
                                getContainingElementName() + " does not match the type " +
                                simpleType.getDescription() + ": " + err.getMessage();
                            ValidationFailure ve = new ValidationFailure(message);
                            ve.setSchemaType(simpleType);
                            ve.setConstraintReference(2, "cvc-datatype-valid", "1");
                            reportValidationError(ve, true, getContainingElementLocationId());
                            super.endElement();
                            return;
                        }
                    }
                    characters(fixedValueLexicalForm, Loc.NONE, ReceiverOption.NONE);
                } else {
                    // if a value was supplied check that it matches the fixed value
                    AtomicSequence actual =
                        simpleType.getTypedValue(buffer.toUnicodeString(), getNamespaceResolver(), getConfiguration().getConversionRules());
                    if (!fixedValue.matches(actual)) {
                        String message = "The content " + Err.wrap(buffer.toString(), Err.VALUE) + " of " +
                            getContainingElementName() + " does not match the fixed value " +
                            Err.wrap(fixedValueLexicalForm, Err.VALUE);
                        ValidationFailure ve = new ValidationFailure(message);
                        ve.setSchemaType(simpleType);
                        ve.setConstraintReference(1, "cvc-elt", "5.2.2.2");
                        reportValidationError(ve, true, getContainingElementLocationId());
                    }
                }
            } else {
                // there's no fixed value and no applicable default value: do the validation
                ConversionRules rules = getValidationContext();
                ValidationFailure err = simpleType.validateContent(buffer.toUnicodeString(), getNamespaceResolver(), rules);
                if (err != null) {
                    String message = "The content " + Err.wrap(buffer.toString(), Err.VALUE) + " of " +
                            getContainingElementName() + " does not match the required simple type. " +
                            err.getMessage();
                    ValidationFailure ve = new ValidationFailure(message);
                    ve.setSchemaType(simpleType);
                    ve.setConstraintReference(2, "cvc-datatype-valid", "1");
                    reportValidationError(ve, true, getContainingElementLocationId());
                    super.endElement();
                    return;
                }
            }
        }
        super.endElement();
    }

}

// Copyright (c) 2004-2022 Saxonica Limited No warranty.

