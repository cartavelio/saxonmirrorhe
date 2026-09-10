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
 * Implements the minExclusive facet on a simple type
 */
public class MinExclusiveFacet extends ValueRangeFacet {

    @Override
    public String getName() {
        return "minExclusive";
    }

    @Override
    protected boolean testComparisonOK(int comparisonResult) {
        return comparisonResult > 0;
    }

    @Override
    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler) throws SchemaException, ValidationException {
        super.checkFacetRestriction(type, base, compiler);
        if (isNewlyIntroduced(base)) {
            Facet other = type.getFacet(MinInclusiveFacet.class);
            if (other != null && other.isNewlyIntroduced(base)) {
                throw new SchemaException("Cannot specify both minExclusive and minInclusive facets on the same type");
            }
        }

        if (base instanceof UserSimpleType) {
            ValueRangeFacet max = (ValueRangeFacet) ((UserSimpleType) base).getFacet(MaxInclusiveFacet.class);
            if (max != null) {
                if (SimpleTypeComparison.getInstance().compareItems(requiredValue, max.requiredValue) >= 0) {
                    throw new SchemaException("The value of the minExclusive facet is greater than or equal to the maxInclusive facet of the base type");
                }
            }
        } else {
            BuiltInAtomicType bbase = (BuiltInAtomicType) base;
            boolean ok = true;
            if (requiredValue instanceof Int64Value) {
                long v = ((Int64Value) requiredValue).longValue() + 1L;
                ok = IntegerValue.checkRange(v, bbase);
            } else if (requiredValue instanceof BigIntegerValue) {
                BigInteger v = ((BigIntegerValue) requiredValue).asBigInteger().add(BigInteger.ONE);
                ok = IntegerValue.checkBigRange(v, bbase);
            }
            if (!ok) {
                throw new SchemaException("The value of the minExclusive facet is greater than or equal to the maxInclusive of the built-in base type");
            }
        }
    }
}

// Copyright (c) 2009-2023 Saxonica Limited