using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class WhileClausePull : Saxon.Hej.expr.flwor.TuplePull {
        internal Saxon.Hej.expr.flwor.TuplePull @base;
        internal Saxon.Hej.expr.elab.BooleanEvaluator predicate;
        public WhileClausePull(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.elab.BooleanEvaluator predicate)  {
            this.@base = @base;
            this.predicate = predicate;
        }
        public override bool nextTuple(Saxon.Hej.expr.XPathContext context) {
            if (@base.nextTuple(context)) {
                return /*Delegate.eval()*/predicate(context);
            }
            return false;
        }
        public override void close() {
            @base.close();
        }
    
    }
}