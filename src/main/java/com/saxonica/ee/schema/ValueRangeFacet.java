package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.sdoc.SimpleTypeDefinition;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.expr.sort.SimpleTypeComparison;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

import java.math.BigInteger;

import static net.sf.saxon.type.SchemaValidationStatus.INVALID;

/**
 * Represents any of the facets minInclusive, maxInclusive, minExclusive, maxExclusive
 */
public abstract class ValueRangeFacet extends Facet {

    boolean isConverted = false;
    AtomicValue requiredValue;    // Initially a string value; converted to the target type when that is known


    @Override
    public UnicodeString getValue() {
        return requiredValue.getUnicodeStringValue();
    }

    public void setValue(String value) throws SchemaException {
        requiredValue = StringValue.makeStringValue(value);
    }

    public void setRequiredValue(AtomicValue value) {
        requiredValue = value;
        isConverted = true;
    }

    public AtomicValue getRequiredValue() {
        return requiredValue;
    }

    /**
     * Check that this facet is legal when used on a type derived by restriction
     *
     * @param base     the type from which the restricted type is derived
     * @param compiler the schema compiler
     * @throws net.sf.saxon.type.SchemaException
     *          if the facet is not legal
     */

    @Override
    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler) throws SchemaException, ValidationException {
        if (!(base.isAtomicType())) {
            throw new SchemaException("The " + getName() +
                    " facet is not available on the non-atomic type " +
                    base.getDescription());
        }

        int primitive;
        if (base instanceof UserAtomicType) {
            primitive = ((UserAtomicType) base).getPrimitiveType();
        } else if (base instanceof BuiltInAtomicType) {
            primitive = ((BuiltInAtomicType) base).getPrimitiveType();
        } else if (base instanceof SimpleTypeDefinition) {
            if ((base.getValidationStatus() == INVALID) ||
                    (((SimpleTypeDefinition) base).getFixupStatus() == INVALID)) {
                return;
            } else {
                throw new IllegalStateException();
            }
        } else {
            throw new IllegalStateException("Unknown atomic type representation: " + base.getClass());
        }
        boolean OK = primitive == StandardNames.XS_INTEGER ||
                primitive == StandardNames.XS_DOUBLE ||
                primitive == StandardNames.XS_FLOAT ||
                primitive == StandardNames.XS_DECIMAL ||
                primitive == StandardNames.XS_DURATION ||
                primitive == StandardNames.XS_DATE ||
                primitive == StandardNames.XS_DATE_TIME ||
                primitive == StandardNames.XS_TIME ||
                primitive == StandardNames.XS_G_YEAR ||
                primitive == StandardNames.XS_G_YEAR_MONTH ||
                primitive == StandardNames.XS_G_MONTH ||
                primitive == StandardNames.XS_G_MONTH_DAY ||
                primitive == StandardNames.XS_G_DAY ||
                primitive == StandardNames.XS_DAY_TIME_DURATION ||
                primitive == StandardNames.XS_YEAR_MONTH_DURATION;
        if (!OK) {
            throw new SchemaException("The " + getName() +
                    " facet is not allowed on the unordered type " +
                    base.getDescription());
        }

        EnterpriseConfiguration config = type.getConfiguration();
        try {
            convertToTargetType((AtomicType) type.getBaseType(), config);
        } catch (SchemaException e1) {
            // normally the value of the restriction must be valid for the base type. But there's
            // an exception for the maxExclusive value - it can be equal to the value on the base type.
            boolean ok;
            try {
                convertToTargetType((AtomicType) type.getBuiltInBaseType(), config);
                ok = true;
            } catch (SchemaException e2) {
                ok = false;
            }
            if (!ok) {
                // the logic here is written to enable a "rethrow" in C#
                throw e1;
            }
        }

        if (base instanceof UserSimpleType) {
            UserSimpleType ubase = (UserSimpleType) base;

            Facet baseFacet = ubase.getFacet(getClass());
            if (baseFacet != null && baseFacet.isFixed()) {
                if (!SimpleTypeComparison.getInstance().equal(requiredValue, ((ValueRangeFacet) baseFacet).requiredValue)) {
                    throw new SchemaException("The " + getName() + " facet is fixed in the base type and cannot be varied");
                }
            }

            // The rules in the spec for cross-checking a min|max Inclusive|Exclusive facet against the facets
            // of the base type are rather complicated. However, what they amount to is that the value of the
            // facet on the subtype must be a valid value of the base type, or, in the case of an exclusive facet,
            // the value must be equal to the same facet on the base type.

            ValidationFailure err = ubase.checkAgainstFacets(requiredValue, null, ubase.getExtendedFacetList(), null);
            if (err != null) {
                if (getName().endsWith("Exclusive")) {
                    if (baseFacet != null) {
                        ((ValueRangeFacet) baseFacet).convertToTargetType(base, config);
                        if (SimpleTypeComparison.getInstance().equal(requiredValue, ((ValueRangeFacet) baseFacet).requiredValue)) {
                            // suppress the error
                            err = null;
                        }
                    }
                }
                if (err != null) {
                    throw new SchemaException("In a type derived by restriction, the " + getName() +
                            " facet is not a valid value of the base type");
                }
            }

        } else {
            // Base type is a built-in type; so we just need to check the built-in integer subtype ranges
            BuiltInAtomicType bbase = (BuiltInAtomicType) base;
            boolean ok = true;
            if (requiredValue instanceof Int64Value) {
                long v = ((Int64Value) requiredValue).longValue();
                ok = IntegerValue.checkRange(v, bbase);
            } else if (requiredValue instanceof BigIntegerValue) {
                BigInteger v = ((BigIntegerValue) requiredValue).asBigInteger();
                ok = IntegerValue.checkBigRange(v, bbase);
            }
            if (!ok) {
                throw new SchemaException("The value of the " + getName() +
                        " facet is out of range for the base type");
            }
        }
    }

    protected void convertToTargetType(SimpleType base, Configuration config) throws SchemaException {
        if (!isConverted) {
            StringConverter converter = ((AtomicType) base).getStringConverter(config.getConversionRules());
            ConversionResult v = converter.convertString(requiredValue.getUnicodeStringValue());
            if (v instanceof ValidationFailure) {
                String s = Err.wrap(requiredValue.getUnicodeStringValue(), Err.VALUE);
                throw new SchemaException("The value " + s + " of the " + getName() +
                        " facet is not valid for the type " +
                        base.getDescription() + ". " +
                        ((ValidationFailure) v).getMessage());
            }
            requiredValue = (AtomicValue) v;
            isConverted = true;
        }
    }

    /**
     * Test whether an atomic value conforms to this facet
     *
     * @param value the value to be tested
     * @return true if the value conforms; false if it doesn't conform, or if the test
     *         fails
     */

    @Override
    public boolean testAtomicValue(AtomicValue value) {
        if (!isConverted) {
            throw new AssertionError("Value used in facet has not been converted to required type");
        }
        if (value == null || requiredValue == null) {
            return false;
        }

        int c = SimpleTypeComparison.getInstance().compareItems(value, requiredValue);
        return c != SequenceTool.INDETERMINATE_ORDERING && testComparisonOK(c);
    }

    /**
     * Test whether the comparison with the min or max value is a valid result
     *
     * @param comparisonResult the result of comparing the actual value with the specified min or max value,
     *                         as defined using the compareTo() method
     * @return true if the comparison is OK, that is, if the value conforms to the facet
     */

    protected abstract boolean testComparisonOK(int comparisonResult);

    @Override
    public FunctionItem getFacetAsFunctionItem() {
        ValueRangeFacet thisFacet = this;
        CallableDelegate callable = new CallableDelegate((context, arguments) -> {
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "class":
                    return StringValue.bmp(getName());
                case "implementation":
                    return new ObjectValue<>(thisFacet);
                case "value":
                    return requiredValue;
                case "fixed":
                    return BooleanValue.get(isFixed());
                default:
                    return EmptySequence.getInstance();
            }
        });
        return new CallableFunction(1, callable, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
    }

}

// Copyright (c) 2004-2023 Saxonica Limited