using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class MaxInclusiveFacet : Saxon.Eej.ee.schema.ValueRangeFacet {
        public override string getName() {
            return "maxInclusive";
        }
        protected internal override bool testComparisonOK(int comparisonResult) {
            return (comparisonResult<=0);
        }
        public override void checkFacetRestriction(Saxon.Eej.ee.schema.UserSimpleType type, Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            base.checkFacetRestriction(type, @base, compiler);
            Saxon.Eej.ee.schema.ValueRangeFacet min = (Saxon.Eej.ee.schema.ValueRangeFacet)(type.getFacet(typeof(Saxon.Eej.ee.schema.MinExclusiveFacet)));
            if ((min != null)) {
                min.convertToTargetType(@base, type.getConfiguration());
                if ((Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().compareItems(requiredValue, min.requiredValue)<=0)) {
                    throw new Saxon.Hej.type.SchemaException("maxInclusive facet is less than or equal to minExclusive");
                }
            }
            min = (Saxon.Eej.ee.schema.ValueRangeFacet)(type.getFacet(typeof(Saxon.Eej.ee.schema.MinInclusiveFacet)));
            if ((min != null)) {
                min.convertToTargetType(@base, type.getConfiguration());
                if ((Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().compareItems(requiredValue, min.requiredValue)<0)) {
                    throw new Saxon.Hej.type.SchemaException("maxInclusive facet is less than or equal to minInclusive");
                }
            }
        }
    
    }
}