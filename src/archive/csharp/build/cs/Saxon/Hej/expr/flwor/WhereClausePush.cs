using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class WhereClausePush : Saxon.Hej.expr.flwor.TuplePush {
        internal Saxon.Hej.expr.flwor.TuplePush destination;
        internal Saxon.Hej.expr.Expression predicate;
        public WhereClausePush(Saxon.Hej.@event.Outputter outputter, Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.expr.Expression predicate) : base (outputter) {
            this.destination = destination;
            this.predicate = predicate;
        }
        public override void processTuple(Saxon.Hej.expr.XPathContext context) {
            if (predicate.effectiveBooleanValue(context)) {
                destination.processTuple(context);
            }
        }
        public override void close() {
            destination.close();
        }
    
    }
}