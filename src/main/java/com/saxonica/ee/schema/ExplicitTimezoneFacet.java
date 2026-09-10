package com.saxonica.ee.schema;

import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.str.BMPString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.value.*;

import static com.saxonica.ee.schema.SchemaStructure.accessorFunction;

/**
 * Represents an explicitTimezone facet on a schema-defined simple type derived from date, time, dateTime,
 * gYear, gYearMonth, gMonth, gMonthDay, or gDay
 */
public class ExplicitTimezoneFacet extends Facet {

    private Optionality facetValue;

    /**
     * Create an explicitTimezone facet
     */

    public ExplicitTimezoneFacet() {
    }

    @Override
    public String getName() {
        return "explicitTimezone";
    }

    @Override
    public UnicodeString getValue() {
        switch (facetValue) {
            case REQUIRED:
                return BMPString.of("required");
            case OPTIONAL:
                return BMPString.of("optional");
            case PROHIBITED:
            default:
                return BMPString.of("prohibited");
        }
    }

    public void setValue(String value) throws SchemaException {
        switch (value) {
            case "required":
                this.facetValue = Optionality.REQUIRED;
                break;
            case "optional":
                this.facetValue = Optionality.OPTIONAL;
                break;
            case "prohibited":
                this.facetValue = Optionality.PROHIBITED;
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

    @Override
    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler) throws SchemaException {

        if (base instanceof UserAtomicType) {
            UserAtomicType ubase = (UserAtomicType) base;

            ExplicitTimezoneFacet baseFacet = (ExplicitTimezoneFacet) ubase.getFacet(ExplicitTimezoneFacet.class);
            if (baseFacet != null) {
                if (baseFacet.isFixed() && facetValue != baseFacet.facetValue) {
                    throw new SchemaException(
                            "The explicitTimezone facet is fixed in the base type and cannot be varied");
                }
                if (baseFacet.facetValue == Optionality.PROHIBITED && facetValue != Optionality.PROHIBITED) {
                    throw new SchemaException(
                            "The explicitTimezone is prohibited in the base type so it must be prohibited in the derived type");
                }
                if (baseFacet.facetValue == Optionality.REQUIRED && facetValue != Optionality.REQUIRED) {
                    throw new SchemaException(
                            "The explicitTimezone is required in the base type so it must be required in the derived type");
                }
            }
        } else if (base.getFingerprint() == StandardNames.XS_DATE_TIME_STAMP) {
            if (facetValue != Optionality.REQUIRED) {
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

    @Override
    public boolean testAtomicValue(AtomicValue value) {
        if (facetValue != Optionality.OPTIONAL) {
            if (value instanceof CalendarValue) {
                return (facetValue == Optionality.REQUIRED) == ((CalendarValue) value).hasTimezone();
            } else {
                return false;
            }
        } else {
            return true;
        }
    }

    @Override
    public FunctionItem getFacetAsFunctionItem() {
        ExplicitTimezoneFacet facet = this;
        return accessorFunction(arg -> {
            String key = arg.head().getStringValue();
            switch (key) {
                case "class":
                    return new StringValue(getName());
                case "implementation":
                    return new ObjectValue<>(facet);
                case "value":
                    return new StringValue(getValue());
                case "fixed":
                    return BooleanValue.get(isFixed());
                default:
                    return EmptySequence.getInstance();
            }
        });
    }


}

// Copyright (c) 2008-2023 Saxonica Limited No warranty.