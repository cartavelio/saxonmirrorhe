package com.saxonica.ee.schema;

import net.sf.saxon.expr.sort.SimpleTypeComparison;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.value.BigIntegerValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.IntegerValue;

import java.math.BigInteger;

/**
 * Implements the maxExclusive facet on a simple type
 */
public class MaxExclusiveFacet extends ValueRangeFacet {

    @Override
    public String getName() {
        return "maxExclusive";
    }

    @Override
    protected boolean testComparisonOK(int comparisonResult) {
        return comparisonResult < 0;
    }

    @Override
    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler) throws SchemaException, ValidationException {
        super.checkFacetRestriction(type, base, compiler);
        if (isNewlyIntroduced(base)) {
            Facet other = type.getFacet(MaxInclusiveFacet.class);
            if (other != null && other.isNewlyIntroduced(base)) {
                throw new SchemaException("Cannot specify both maxExclusive and maxInclusive facets on the same type");
            }
        }
        ValueRangeFacet min = (ValueRangeFacet) type.getFacet(MinExclusiveFacet.class);
        if (min != null) {
            min.convertToTargetType(base, type.getConfiguration());
            if (SimpleTypeComparison.getInstance().compareItems(requiredValue, min.requiredValue) <= 0) {
                throw new SchemaException("maxExclusive facet is less than or equal to minExclusive");
            }
        }
        min = (ValueRangeFacet) type.getFacet(MinInclusiveFacet.class);
        if (min != null) {
            min.convertToTargetType(base, type.getConfiguration());
            if (SimpleTypeComparison.getInstance().compareItems(requiredValue, min.requiredValue) <= 0) {
                throw new SchemaException("maxExclusive facet is less than or equal to minInclusive");
            }
        }
        if (base instanceof UserSimpleType) {
            min = (ValueRangeFacet) ((UserSimpleType) base).getFacet(MinInclusiveFacet.class);
            if (min != null) {
                if (SimpleTypeComparison.getInstance().compareItems(requiredValue, min.requiredValue) <= 0) {
                    throw new SchemaException("The value of the maxExclusive facet is less than or equal to the minInclusive facet of the base type");
                }
            }
        } else {
            BuiltInAtomicType bbase = (BuiltInAtomicType) base;
            boolean ok = true;
            if (requiredValue instanceof Int64Value) {
                long v = ((Int64Value) requiredValue).longValue() - 1L;
                ok = IntegerValue.checkRange(v, bbase);
            } else if (requiredValue instanceof BigIntegerValue) {
                BigInteger v = ((BigIntegerValue) requiredValue).asBigInteger().subtract(BigInteger.ONE);
                ok = IntegerValue.checkBigRange(v, bbase);
            }
            if (!ok) {
                throw new SchemaException("The value of the maxExclusive facet is less than or equal to the minInclusive of the built-in base type");
            }
        }

    }
}

// Copyright (c) 2009-2023 Saxonica Limited

