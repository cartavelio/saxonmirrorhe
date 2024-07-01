using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class WhileClausePush : Saxon.Hej.expr.flwor.TuplePush {
        private readonly Saxon.Hej.expr.flwor.TuplePush destination;
        private readonly Saxon.Hej.expr.elab.BooleanEvaluator predicate;
        private bool finished = false;
        public WhileClausePush(Saxon.Hej.@event.Outputter outputter, Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.expr.elab.BooleanEvaluator predicate) : base (outputter) {
            this.destination = destination;
            this.predicate = predicate;
        }
        public override void processTuple(Saxon.Hej.expr.XPathContext context) {
            if (finished) {
                return ;
            }
            if (/*Delegate.eval()*/predicate(context)) {
                destination.processTuple(context);
            } else {
                finished = true;
            }
        }
        public override void close() {
            destination.close();
        }
    
    }
}