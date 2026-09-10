package com.saxonica.ee.schema;

import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.str.BMPString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

/**
 * Represents either of the facets totalDigits or fractionDigits
 */
public abstract class DigitsFacet extends Facet {

    /**
     * The numeric value of the facet
     */
    private int requiredValue;

    /**
     * Set the value of this facet as a string
     *
     * @param value the string value of the facet
     * @throws net.sf.saxon.type.SchemaException
     *          if the value is invalid
     */

    public abstract void setValue(String value) throws SchemaException;

    /**
     * Get the value of this facet as a string
     *
     * @return the value of this facet as a string
     */

    @Override
    public UnicodeString getValue() {
        return BMPString.of(requiredValue + "");
    }

    /**
     * Get the value of this facet as an integer (used for integer facets such as minLength or totalDigits)
     *
     * @return the value of the facet expressed as an integer
     */

    public int getNumericValue() {
        return requiredValue;
    }

    /**
     * Set the value of this facet as an integer (used for integer facets such as minLength or totalDigits)
     *
     * @param value the value, as an integer
     */

    public void setNumericValue(int value) {
        requiredValue = value;
    }

    /**
     * Check that this facet is legal when used on a type derived by restriction
     *
     * @param base     the type from which the restricted type is derived
     * @param compiler the schema compiler
     * @throws SchemaException if the facet is not allowed for this simple type, or if
     *                         the combination of facets is inconsistent with values defined on the base type.
     */

    @Override
    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler) throws SchemaException, ValidationException {
        if (!(base.isAtomicType())) {
            throw new SchemaException("The " + getName() +
                    " facet is not available on the non-atomic type " +
                    type.getDescription());
        }

        int primitive = ((AtomicType) base).getPrimitiveType();

        if (primitive == StandardNames.XS_INTEGER || primitive == StandardNames.XS_DECIMAL) {
            // all is well
        } else {
            throw new SchemaException("The " + getName() +
                    " facet is not allowed on a type derived from the type " +
                    base.getDescription());
        }
    }

    /**
     * Test whether an atomic value conforms to this facet
     *
     * @param value the value to be tested
     * @return true if the value conforms; false if it doesn't conform
     */

    @Override
    public boolean testAtomicValue(AtomicValue value) {
//        if (value instanceof PrecisionDecimalValue) {
//            return ((PrecisionDecimalValue)value).checkTotalDigits(requiredValue);
//        }

        if (!(value instanceof DecimalValue)) {
            return false;
        }

        String s = value.getStringValue();
        if (s.charAt(0) == '-') {
            s = s.substring(1);
        }
        int before, after;
        int dot = s.indexOf('.');
        if (dot < 0) {
            before = s.length();
            if (s.charAt(0) == '0') {
                before--;
            }
            after = 0;
        } else {
            before = dot;
            after = s.length() - before - 1;
            if (s.charAt(0) == '0') {
                before--;
            }
        }
        if (this instanceof TotalDigitsFacet) {
            return (before + after) <= requiredValue;
        } else {
            return after <= requiredValue;
        }

    }

    @Override
    public FunctionItem getFacetAsFunctionItem() {
        DigitsFacet facet = this;
        CallableDelegate callable = new CallableDelegate((context, arguments) -> {
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "class":
                    return new StringValue(getName());
                case "implementation":
                    return new ObjectValue<>(facet);
                case "value":
                    BuiltInAtomicType type = DigitsFacet.this instanceof TotalDigitsFacet ?
                                                      BuiltInAtomicType.POSITIVE_INTEGER : BuiltInAtomicType.NON_NEGATIVE_INTEGER;
                    return new Int64Value(requiredValue, type, false);
                case "fixed":
                    return BooleanValue.get(isFixed());
                default:
                    return EmptySequence.getInstance();
            }
        });
        return new CallableFunction(1, callable, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
    }


}

// Copyright (c) 2004-2023 Saxonica Limited See Terms and Conditions.