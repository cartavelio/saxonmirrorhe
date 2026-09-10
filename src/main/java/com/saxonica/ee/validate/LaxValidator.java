package com.saxonica.ee.validate;

import com.saxonica.ee.schema.AttributeDecl;
import com.saxonica.ee.schema.ElementDecl;
import com.saxonica.ee.schema.UserAtomicType;
import com.saxonica.ee.schema.ValueConstraint;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.ValidationFailure;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Whitespace;

/**
 * This class performs lax validation on an element.
 *
 * @author Michael Kay
 */

public class LaxValidator extends ContentValidator {


    public LaxValidator(Receiver next) {
        super(next);
    }

    /**
     * Get the schema type against which we are validating, if any
     *
     * @return the schema type that this validator is validating against, or null
     */

    public SchemaType getSchemaType() {
        return null;
    }

    /**
     * Output element start tag for a child element
     */

    public void startElement(NodeName nodeName, SchemaType typeCode, Location location, int properties) throws XPathException {
        int fp = nodeName.obtainFingerprint(getNamePool());
        ElementDecl decl = (ElementDecl) getConfiguration().getElementDeclaration(fp);
        makeChildValidator(decl, nodeName.getStructuredQName(), location, Validation.LAX);
        checkNoChildrenWhenNil(location);
        typeCode = childValidator.getAnnotation();
        super.startElement(nodeName, typeCode, location, properties);
    }

    /*@Nullable*/
    public ContentValidator getChildValidator() {
        return childValidator;
    }

    /**
     * Handle an attribute
     *
     * @param attName    integer identifying the name of the attribute
     * @param typeCode   integer identifying the type annotation of the
     *                   attribute
     * @param value      the value of the attribute
     * @param locationId the location of the node in the source, or of the instruction that created it
     *@param properties bit-significant flags indicating special action to
     *                   be taken  @throws net.sf.saxon.trans.XPathException
     *          On any failure to write the attribute
     */

    public void attribute(NodeName attName, SimpleType typeCode, CharSequence value, Location locationId, int properties)
            throws XPathException {
        int fingerprint = attName.obtainFingerprint(getNamePool());
        if (fingerprint == StandardNames.XSI_NIL) {
            if (getNillability() == Nillability.DISALLOWED) {
                String message = "The element is not nillable, so xsi:nil must not be present";
                ValidationFailure ve = new ValidationFailure(message);
                ve.setConstraintReference(1, "cvc-elt", "3.1");
                ve.setSchemaType(getSchemaType());
                reportValidationError(ve, true, locationId);
            }
            String b = Whitespace.trim(value);
            if (getNillability() != Nillability.IGNORED && (b.equals("true") || b.equals("1"))) {
                nil = true;
            }
        }
        SimpleType annotation = BuiltInAtomicType.UNTYPED_ATOMIC;
        // During lax validation, if an attribute is found for which a global attribute declaration
        //  exists, the attribute must be valid against that declaration.
        AttributeDecl decl = (AttributeDecl) getConfiguration().getAttributeDeclaration(attName.getStructuredQName());
        if (decl != null) {
            SimpleType type = decl.getSimpleType();
            ValidationFailure err = type.validateContent(
                    value, getNamespaceResolver(), getConfiguration().getConversionRules());
            if (err != null) {
                err.setConstraintReference(1, "cvc-complex-type", "3");
                err.setSchemaType(type);
                reportValidationError(err, false, locationId);
            } else {
                // if the attribute declaration has an enumeration facet, economize on storage by
                // passing on the reference to the value already held in the schema.
                if (type instanceof UserAtomicType) {
                    value = ((UserAtomicType) type).getSharedInstance(value.toString());
                }

                annotation = type;

                // Check the value if it is fixed

                ValueConstraint constraint = decl.getFixedValueConstraint();
                if (constraint != null && !constraint.testFixedValue(
                        getConfiguration(), value, type, getNamespaceResolver())) {
                    AtomicSequence fixed = decl.getFixedValue();
                    String message = "The actual value " + Err.wrap(value, Err.VALUE) +
                            " of attribute " + Err.wrap(attName.getDisplayName(), Err.ATTRIBUTE) +
                            " does not match the required fixed value " +
                            (fixed instanceof AtomicValue ? Err.wrap(fixed.getStringValue(), Err.VALUE) : "");
                    ValidationFailure ve = new ValidationFailure(message);
                    ve.setConstraintReference(1, "cvc-au", "0");
                    ve.setSchemaType(type);
                    reportValidationError(ve, false, locationId);
                }
            }
        }
        super.attribute(attName, annotation, value, locationId, properties);
    }

    /**
     * Character data
     */

    public void characters(CharSequence chars, Location locationId, int properties) throws XPathException {
        checkNoCharactersWhenNil(locationId);
        super.characters(chars, locationId, properties);
    }

}

// Copyright (c) 2004-2018 Saxonica Limited. No warranty.

