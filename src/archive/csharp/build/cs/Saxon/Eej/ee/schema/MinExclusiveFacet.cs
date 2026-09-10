using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class MinExclusiveFacet : Saxon.Eej.ee.schema.ValueRangeFacet {
        public override string getName() {
            return "minExclusive";
        }
        protected internal override bool testComparisonOK(int comparisonResult) {
            return (comparisonResult>0);
        }
        public override void checkFacetRestriction(Saxon.Eej.ee.schema.UserSimpleType type, Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            base.checkFacetRestriction(type, @base, compiler);
            if (isNewlyIntroduced(@base)) {
                Saxon.Eej.ee.schema.Facet other = type.getFacet(typeof(Saxon.Eej.ee.schema.MinInclusiveFacet));
                if (((other != null) && other.isNewlyIntroduced(@base))) {
                    throw new Saxon.Hej.type.SchemaException("Cannot specify both minExclusive and minInclusive facets on the same type");
                }
            }
            if ((@base is Saxon.Eej.ee.schema.UserSimpleType)) {
                Saxon.Eej.ee.schema.ValueRangeFacet max = (Saxon.Eej.ee.schema.ValueRangeFacet)(((Saxon.Eej.ee.schema.UserSimpleType)(@base)).getFacet(typeof(Saxon.Eej.ee.schema.MaxInclusiveFacet)));
                if ((max != null)) {
                    if ((Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().compareItems(requiredValue, max.requiredValue)>=0)) {
                        throw new Saxon.Hej.type.SchemaException("The value of the minExclusive facet is greater than or equal to the maxInclusive facet of the base type");
                    }
                }
            } else {
                Saxon.Hej.type.BuiltInAtomicType bbase = (Saxon.Hej.type.BuiltInAtomicType)(@base);
                bool ok = true;
                if ((requiredValue is Saxon.Hej.value.Int64Value)) {
                    long v = (((Saxon.Hej.value.Int64Value)(requiredValue)).longValue()+1L);
                    ok = Saxon.Hej.value.IntegerValue.checkRange(v, bbase);
                } else {
                    if ((requiredValue is Saxon.Hej.value.BigIntegerValue)) {
                        System.Numerics.BigInteger v = System.Numerics.BigInteger.Add(((Saxon.Hej.value.BigIntegerValue)(requiredValue)).asBigInteger(), System.Numerics.BigInteger.One);
                        ok = Saxon.Hej.value.IntegerValue.checkBigRange(v, bbase);
                    }
                }
                if (!(ok)) {
                    throw new Saxon.Hej.type.SchemaException("The value of the minExclusive facet is greater than or equal to the maxInclusive of the built-in base type");
                }
            }
        }
    
    }
}