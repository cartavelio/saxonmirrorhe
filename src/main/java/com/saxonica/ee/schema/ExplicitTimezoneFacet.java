package com.saxonica.ee.schema;

import net.sf.saxon.expr.Callable;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.type.SchemaComponent;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.value.*;

/**
 * Represents an explicitTimezone facet on a schema-defined simple type derived from date, time, dateTime,
 * gYear, gYearMonth, gMonth, gMonthDay, or gDay
 */
public class ExplicitTimezoneFacet extends Facet {

    public static final int OPTIONAL = 0;
    public static final int REQUIRED = 1;
    public static final int PROHIBITED = 2;

    int facetValue;

    /**
     * Create an explicitTimezone facet
     */

    public ExplicitTimezoneFacet() {
    }

    public String getName() {
        return "explicitTimezone";
    }

    public String getValue() {
        switch (facetValue) {
            case REQUIRED:
                return "required";
            case OPTIONAL:
                return "optional";
            case PROHIBITED:
            default:
                return "prohibited";
        }
    }

    public void setValue(String value) throws SchemaException {
        switch (value) {
            case "required":
                this.facetValue = REQUIRED;
                break;
            case "optional":
                this.facetValue = OPTIONAL;
                break;
            case "prohibited":
                this.facetValue = PROHIBITED;
                break;
            default:
                throw new SchemaException("Value of explicitTimezone facet must be required, optional, or prohibited");
        }
    }

    /**
     * Check that this facet is legal when used on a type derived by restriction
     *
     * @param base     the type from which the restricted type is derived
     * @param compiler the schema compiler
     * @throws net.sf.saxon.type.SchemaException
     *          if the facet is not legal
     */

    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler) throws SchemaException {

        if (base instanceof UserAtomicType) {
            UserAtomicType ubase = (UserAtomicType) base;

            ExplicitTimezoneFacet baseFacet = (ExplicitTimezoneFacet) ubase.getFacet(ExplicitTimezoneFacet.class);
            if (baseFacet != null) {
                if (baseFacet.isFixed() && facetValue != baseFacet.facetValue) {
                    throw new SchemaException(
                            "The explicitTimezone facet is fixed in the base type and cannot be varied");
                }
                if (baseFacet.facetValue == PROHIBITED && facetValue != PROHIBITED) {
                    throw new SchemaException(
                            "The explicitTimezone is prohibited in the base type so it must be prohibited in the derived type");
                }
                if (baseFacet.facetValue == REQUIRED && facetValue != REQUIRED) {
                    throw new SchemaException(
                            "The explicitTimezone is required in the base type so it must be required in the derived type");
                }
            }
        } else if (base.getFingerprint() == StandardNames.XS_DATE_TIME_STAMP) {
            if (facetValue != REQUIRED) {
                throw new SchemaException(
                        "The explicitTimezone is required in the base type so it must be required in the derived type");
            }
        }
    }

    /**
     * Test whether an atomic value conforms to this facet
     *
     * @param value the value to be tested
     * @return true if the value conforms; false if it doesn't conform, or if the test
     *         fails
     */

    public boolean testAtomicValue(AtomicValue value) {
        if (facetValue != OPTIONAL) {
            if (value instanceof CalendarValue) {
                return (facetValue == REQUIRED) == ((CalendarValue) value).hasTimezone();
            } else {
                return false;
            }
        } else {
            return true;
        }
    }

    @Override
    public Function getFacetAsFunctionItem() {
        Callable callable = (context, arguments) -> {
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "class":
                    return new StringValue(getName());
                case "implementation":
                    return new ObjectValue<>(ExplicitTimezoneFacet.this);
                case "value":
                    return new StringValue(getValue());
                case "fixed":
                    return BooleanValue.get(isFixed());
                default:
                    return EmptySequence.getInstance();
            }
        };
        return new CallableFunction(1, callable, SchemaComponent.COMPONENT_FUNCTION_TYPE);
    }


}

// Copyright (c) 2008-2012 Saxonica Limited. No warranty.