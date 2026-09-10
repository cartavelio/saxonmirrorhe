package com.saxonica.ee.schema;

import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.ValidationException;

/**
 * Implements the maxInclusive facet on a simple type
 */
public class MaxInclusiveFacet extends ValueRangeFacet {

    public String getName() {
        return "maxInclusive";
    }

    protected boolean testComparisonOK(int comparisonResult) {
        return comparisonResult <= 0;
    }

    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler) throws SchemaException, ValidationException {
        super.checkFacetRestriction(type, base, compiler);
        ValueRangeFacet min = (ValueRangeFacet) type.getFacet(MinExclusiveFacet.class);
        if (min != null) {
            min.convertToTargetType(base, type.getConfiguration());
            if (requiredValue.getSchemaComparable().compareTo(
                    min.requiredValue.getSchemaComparable()) <= 0) {
                throw new SchemaException("maxInclusive facet is less than or equal to minExclusive");
            }
        }
        min = (ValueRangeFacet) type.getFacet(MinInclusiveFacet.class);
        if (min != null) {
            min.convertToTargetType(base, type.getConfiguration());
            if (requiredValue.getSchemaComparable().compareTo(
                    min.requiredValue.getSchemaComparable()) < 0) {
                throw new SchemaException("maxInclusive facet is less than or equal to minInclusive");
            }
        }
    }
}

// Copyright (c) Saxonica 2009. All rights reserved