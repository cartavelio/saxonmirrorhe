package com.saxonica.ee.schema;

import net.sf.saxon.om.StandardNames;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.ValidationException;

/**
 * Represents the fraction-digits facet in a simple type restriction
 */
public class FractionDigitsFacet extends DigitsFacet {

    @Override
    public String getName() {
        return "fractionDigits";
    }

    /**
     * Set the value of this facet. For internal use only.
     *
     * @param value the value of the facet. This must be a string that parses as an integer.
     * @throws net.sf.saxon.type.SchemaException
     *          if the value is invalid (not an integer)
     */
    @Override
    public void setValue(String value) throws SchemaException {
        int val;
        try {
            val = Integer.parseInt(value);
        } catch (NumberFormatException err) {
            throw new SchemaException("Value of fractionDigits must be an integer");
        }
        if (val < 0) {
            throw new SchemaException("Value of fractionDigits must not be negative");
        }
        setNumericValue(val);
    }

    @Override
    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler) throws SchemaException, ValidationException {
        super.checkFacetRestriction(type, base, compiler);
        int primitive = ((AtomicType) base).getPrimitiveType();
        if (primitive == StandardNames.XS_INTEGER && getNumericValue() != 0) {
            throw new SchemaException("The fractionDigits facet on an integer type must be zero");
        }
        TotalDigitsFacet total = (TotalDigitsFacet) type.getFacet(TotalDigitsFacet.class);
        if (total != null && total.getNumericValue() < getNumericValue()) {
            throw new SchemaException("The fractionDigits facet exceeds the totalDigits");
        }
        if (base instanceof UserSimpleType) {
            UserSimpleType ubase = (UserSimpleType) base;
            DigitsFacet f = (DigitsFacet) ubase.getFacet(FractionDigitsFacet.class);
            if (f != null) {
                if (getNumericValue() > f.getNumericValue()) {
                    throw new SchemaException("The fractionDigits facet must not exceed the value on the base type");
                }
                if (f.isFixed() && getNumericValue() != f.getNumericValue()) {
                    throw new SchemaException("The fractionDigits facet is fixed in the base type and cannot be varied");
                }
            }
        }

    }
}

// Copyright (c) 2009-2022 Saxonica Limited



