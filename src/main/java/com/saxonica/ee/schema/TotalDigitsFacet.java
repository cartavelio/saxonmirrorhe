package com.saxonica.ee.schema;

import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.ValidationException;

/**
 * Eepresents the value of the totalDigits facet
 */
public class TotalDigitsFacet extends DigitsFacet {

    @Override
    public String getName() {
        return "totalDigits";
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
            throw new SchemaException("Value of " + getName() + " facet must be an integer");
        }
        if (val <= 0) {
            throw new SchemaException("Value of totalDigits must be greater than zero");
        }
        setNumericValue(val);
    }

    @Override
    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler) throws SchemaException, ValidationException {
        super.checkFacetRestriction(type, base, compiler);
        if (base instanceof UserSimpleType) {
            UserSimpleType ubase = (UserSimpleType) base;
            DigitsFacet f = (DigitsFacet) ubase.getFacet(TotalDigitsFacet.class);
            if (f != null) {
                if (getNumericValue() > f.getNumericValue()) {
                    throw new SchemaException("The totalDigits facet must not exceed the value on the base type");
                }
                if (f.isFixed() && getNumericValue() != f.getNumericValue()) {
                    throw new SchemaException("The totalDigits facet is fixed in the base type and cannot be varied");
                }
            }
        }
    }
}

// Copyright (c) 2009-2023 Saxonica Limited



