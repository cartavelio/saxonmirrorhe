using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class MinInclusiveFacet : Saxon.Eej.ee.schema.ValueRangeFacet {
        public override string getName() {
            return "minInclusive";
        }
        protected internal override bool testComparisonOK(int comparisonResult) {
            return (comparisonResult>=0);
        }
    
    }
}