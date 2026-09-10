using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class MaxExclusiveFacet : Saxon.Eej.ee.schema.ValueRangeFacet {
        public override string getName() {
            return "maxExclusive";
        }
        protected internal override bool testComparisonOK(int comparisonResult) {
            return (comparisonResult<0);
        }
        public override void checkFacetRestriction(Saxon.Eej.ee.schema.UserSimpleType type, Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            base.checkFacetRestriction(type, @base, compiler);
            if (isNewlyIntroduced(@base)) {
                Saxon.Eej.ee.schema.Facet other = type.getFacet(typeof(Saxon.Eej.ee.schema.MaxInclusiveFacet));
                if (((other != null) && other.isNewlyIntroduced(@base))) {
                    throw new Saxon.Hej.type.SchemaException("Cannot specify both maxExclusive and maxInclusive facets on the same type");
                }
            }
            Saxon.Eej.ee.schema.ValueRangeFacet min = (Saxon.Eej.ee.schema.ValueRangeFacet)(type.getFacet(typeof(Saxon.Eej.ee.schema.MinExclusiveFacet)));
            if ((min != null)) {
                min.convertToTargetType(@base, type.getConfiguration());
                if ((Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().compareItems(requiredValue, min.requiredValue)<=0)) {
                    throw new Saxon.Hej.type.SchemaException("maxExclusive facet is less than or equal to minExclusive");
                }
            }
            min = (Saxon.Eej.ee.schema.ValueRangeFacet)(type.getFacet(typeof(Saxon.Eej.ee.schema.MinInclusiveFacet)));
            if ((min != null)) {
                min.convertToTargetType(@base, type.getConfiguration());
                if ((Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().compareItems(requiredValue, min.requiredValue)<=0)) {
                    throw new Saxon.Hej.type.SchemaException("maxExclusive facet is less than or equal to minInclusive");
                }
            }
            if ((@base is Saxon.Eej.ee.schema.UserSimpleType)) {
                min = (Saxon.Eej.ee.schema.ValueRangeFacet)(((Saxon.Eej.ee.schema.UserSimpleType)(@base)).getFacet(typeof(Saxon.Eej.ee.schema.MinInclusiveFacet)));
                if ((min != null)) {
                    if ((Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().compareItems(requiredValue, min.requiredValue)<=0)) {
                        throw new Saxon.Hej.type.SchemaException("The value of the maxExclusive facet is less than or equal to the minInclusive facet of the base type");
                    }
                }
            } else {
                Saxon.Hej.type.BuiltInAtomicType bbase = (Saxon.Hej.type.BuiltInAtomicType)(@base);
                bool ok = true;
                if ((requiredValue is Saxon.Hej.value.Int64Value)) {
                    long v = (((Saxon.Hej.value.Int64Value)(requiredValue)).longValue()-1L);
                    ok = Saxon.Hej.value.IntegerValue.checkRange(v, bbase);
                } else {
                    if ((requiredValue is Saxon.Hej.value.BigIntegerValue)) {
                        System.Numerics.BigInteger v = System.Numerics.BigInteger.Subtract(((Saxon.Hej.value.BigIntegerValue)(requiredValue)).asBigInteger(), System.Numerics.BigInteger.One);
                        ok = Saxon.Hej.value.IntegerValue.checkBigRange(v, bbase);
                    }
                }
                if (!(ok)) {
                    throw new Saxon.Hej.type.SchemaException("The value of the maxExclusive facet is less than or equal to the minInclusive of the built-in base type");
                }
            }
        }
    
    }
}