package com.saxonica.ee.schema;

/**
 * Implements the minInclusive facet on a simple type
 */
public class MinInclusiveFacet extends ValueRangeFacet {

    public String getName() {
        return "minInclusive";
    }

    protected boolean testComparisonOK(int comparisonResult) {
        return comparisonResult >= 0;
    }
}

// Copyright (c) Saxonica 2009. All rights reserved