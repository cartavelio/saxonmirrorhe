package com.saxonica.ee.schema;

import net.sf.saxon.expr.sort.SimpleTypeComparison;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.ValidationException;

/**
 * Implements the maxInclusive facet on a simple type
 */
public class MaxInclusiveFacet extends ValueRangeFacet {

    @Override
    public String getName() {
        return "maxInclusive";
    }

    @Override
    protected boolean testComparisonOK(int comparisonResult) {
        return comparisonResult <= 0;
    }

    @Override
    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler) throws SchemaException, ValidationException {
        super.checkFacetRestriction(type, base, compiler);
        ValueRangeFacet min = (ValueRangeFacet) type.getFacet(MinExclusiveFacet.class);
        if (min != null) {
            min.convertToTargetType(base, type.getConfiguration());
            if (SimpleTypeComparison.getInstance().compareItems(requiredValue, min.requiredValue) <= 0) {
                throw new SchemaException("maxInclusive facet is less than or equal to minExclusive");
            }
        }
        min = (ValueRangeFacet) type.getFacet(MinInclusiveFacet.class);
        if (min != null) {
            min.convertToTargetType(base, type.getConfiguration());
            if (SimpleTypeComparison.getInstance().compareItems(requiredValue, min.requiredValue) < 0) {
                throw new SchemaException("maxInclusive facet is less than or equal to minInclusive");
            }
        }
    }
}

// Copyright (c) 2009-2022 Saxonica Limited