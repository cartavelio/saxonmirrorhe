package com.saxonica.ee.schema;

/**
 * Implements the minInclusive facet on a simple type
 */
public class MinInclusiveFacet extends ValueRangeFacet {

    @Override
    public String getName() {
        return "minInclusive";
    }

    @Override
    protected boolean testComparisonOK(int comparisonResult) {
        return comparisonResult >= 0;
    }
}

// Copyright (c) 2009-2023 Saxonica Limited.