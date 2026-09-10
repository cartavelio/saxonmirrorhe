package com.saxonica.ee.validate;

import com.saxonica.ee.schema.AttributeDecl;
import com.saxonica.ee.schema.ElementDecl;
import com.saxonica.ee.schema.UserAtomicType;
import com.saxonica.ee.schema.ValueConstraint;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.ValidationFailure;
import net.sf.saxon.value.AtomicValue;

import java.util.ArrayList;
import java.util.List;

/**
 * This class performs lax validation on an element.
 *
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

    @Override
    public SchemaType getSchemaType() {
        return null;
    }

    /**
     * Output element start tag for a child element
     */
    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) throws XPathException {
        int fp = elemName.obtainFingerprint(getNamePool());
        ElementDecl decl = (ElementDecl) getConfiguration().getElementDeclaration(fp);
        makeChildValidator(decl, elemName.getStructuredQName(), location, Validation.LAX);
        checkNoChildrenWhenNil(location);
        List<AttributeInfo> attList = new ArrayList<>(attributes.size());
        for (AttributeInfo att : attributes) {
            attList.add(processAttribute(att));
        }
        AttributeMap validatedAtts = SequenceTool.attributeMapFromList(attList);
        type = childValidator.getAnnotation();
        super.startElement(elemName, type, validatedAtts, namespaces, location, properties);
    }

    /*@Nullable*/
    @Override
    protected ContentValidator getChildValidator() {
        return childValidator;
    }

    /**
     * Lax-validate an attribute
     * @param att the attribute to be validated
     */

    protected AttributeInfo processAttribute(AttributeInfo att) throws XPathException {
        String value = att.getValue();
        SimpleType annotation = BuiltInAtomicType.UNTYPED_ATOMIC;
        // During lax validation, if an attribute is found for which a global attribute declaration
        //  exists, the attribute must be valid against that declaration.
        AttributeDecl decl = (AttributeDecl) getConfiguration().getAttributeDeclaration(att.getNodeName().getStructuredQName());
        if (decl != null) {
            SimpleType type = decl.getSimpleType();
            ValidationFailure err = type.validateContent(
                    StringView.of(att.getValue()).tidy(), getNamespaceResolver(), getConfiguration().getConversionRules());
            if (err != null) {
                err.setConstraintReference(1, "cvc-complex-type", "3");
                err.setSchemaType(type);
                reportValidationError(err, false, att.getLocation());
            } else {
                // if the attribute declaration has an enumeration facet, economize on storage by
                // passing on the reference to the value already held in the schema.
                if (type instanceof UserAtomicType) {
                    value = ((UserAtomicType) type).getSharedInstance(value);
                }

                annotation = type;

                // Check the value if it is fixed

                ValueConstraint constraint = decl.getFixedValueConstraint();
                if (constraint != null && !constraint.testFixedValue(
                        getConfiguration(), StringView.of(value).tidy(), type, getNamespaceResolver())) {
                    AtomicSequence fixed = decl.getFixedValue();
                    String message = "The actual value " + Err.wrap(value, Err.VALUE) +
                            " of attribute " + Err.wrap(att.getNodeName().getDisplayName(), Err.ATTRIBUTE) +
                            " does not match the required fixed value " +
                            (fixed instanceof AtomicValue ? Err.wrap(fixed.getUnicodeStringValue(), Err.VALUE) : "");
                    ValidationFailure ve = new ValidationFailure(message);
                    ve.setConstraintReference(1, "cvc-au", "0");
                    ve.setSchemaType(type);
                    reportValidationError(ve, false, att.getLocation());
                }
            }
        }
        return new AttributeInfo(att.getNodeName(), annotation, value, att.getLocation(), att.getProperties());
    }

    /**
     * Character data
     */

    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
        checkNoCharactersWhenNil(locationId);
        super.characters(chars, locationId, properties);
    }

}

// Copyright (c) 2004-2022 Saxonica Limited No warranty.

