using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class CountClausePull : Saxon.Hej.expr.flwor.TuplePull {
        internal Saxon.Hej.expr.flwor.TuplePull @base;
        internal int slot;
        internal int count = 0;
        public CountClausePull(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.flwor.CountClause countClause)  {
            this.@base = @base;
            this.slot = countClause.getRangeVariable().getLocalSlotNumber();
        }
        public override bool nextTuple(Saxon.Hej.expr.XPathContext context) {
            if (!(@base.nextTuple(context))) {
                count = 0;
                context.setLocalVariable(slot, Saxon.Hej.value.Int64Value.ZERO);
                return false;
            }
            context.setLocalVariable(slot, new Saxon.Hej.value.Int64Value(++count));
            return true;
        }
    
    }
}