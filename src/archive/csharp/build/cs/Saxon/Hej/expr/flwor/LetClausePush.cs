using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class LetClausePush : Saxon.Hej.expr.flwor.TuplePush {
        internal Saxon.Hej.expr.flwor.TuplePush destination;
        internal Saxon.Hej.expr.flwor.LetClause letClause;
        public LetClausePush(Saxon.Hej.@event.Outputter outputter, Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.expr.flwor.LetClause letClause) : base (outputter) {
            this.destination = destination;
            this.letClause = letClause;
        }
        public override void processTuple(Saxon.Hej.expr.XPathContext context) {
            letClause.evaluateRangeVariable(context);
            destination.processTuple(context);
        }
        public override void close() {
            destination.close();
        }
    
    }
}