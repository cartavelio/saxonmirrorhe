using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class WhereClausePush : Saxon.Hej.expr.flwor.TuplePush {
        private readonly Saxon.Hej.expr.flwor.TuplePush destination;
        private readonly Saxon.Hej.expr.elab.BooleanEvaluator predicate;
        public WhereClausePush(Saxon.Hej.@event.Outputter outputter, Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.expr.elab.BooleanEvaluator predicate) : base (outputter) {
            this.destination = destination;
            this.predicate = predicate;
        }
        public override void processTuple(Saxon.Hej.expr.XPathContext context) {
            if (/*Delegate.eval()*/predicate(context)) {
                destination.processTuple(context);
            }
        }
        public override void close() {
            destination.close();
        }
    
    }
}