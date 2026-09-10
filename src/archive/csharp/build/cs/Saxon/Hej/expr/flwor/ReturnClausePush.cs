using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class ReturnClausePush : Saxon.Hej.expr.flwor.TuplePush {
        private readonly Saxon.Hej.expr.Expression returnExpr;
        public ReturnClausePush(Saxon.Hej.@event.Outputter outputter, Saxon.Hej.expr.Expression returnExpr) : base (outputter) {
            this.returnExpr = returnExpr;
        }
        public override void processTuple(Saxon.Hej.expr.XPathContext context) {
            returnExpr.process(getOutputter(), context);
        }
        public override void close() {
        }
    
    }
}