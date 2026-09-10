using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class ReturnClausePush : Saxon.Hej.expr.flwor.TuplePush {
        private readonly Saxon.Hej.expr.elab.PushEvaluator returnExpr;
        public ReturnClausePush(Saxon.Hej.@event.Outputter outputter, Saxon.Hej.expr.elab.PushEvaluator returnExpr) : base (outputter) {
            this.returnExpr = returnExpr;
        }
        public override void processTuple(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.instruct.TailCall tc = /*Delegate.processLeavingTail()*/returnExpr(getOutputter(), context);
            Saxon.Hej.expr.Expression.dispatchTailCall(tc);
        }
        public override void close() {
        }
    
    }
}