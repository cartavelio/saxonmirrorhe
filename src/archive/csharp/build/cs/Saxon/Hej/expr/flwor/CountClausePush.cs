using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class CountClausePush : Saxon.Hej.expr.flwor.TuplePush {
        internal Saxon.Hej.expr.flwor.TuplePush destination;
        internal int slot;
        internal int count = 0;
        public CountClausePush(Saxon.Hej.@event.Outputter outputter, Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.expr.flwor.CountClause countClause) : base (outputter) {
            this.destination = destination;
            this.slot = countClause.getRangeVariable().getLocalSlotNumber();
        }
        public override void processTuple(Saxon.Hej.expr.XPathContext context) {
            context.setLocalVariable(slot, new Saxon.Hej.value.Int64Value(++count));
            destination.processTuple(context);
        }
        public override void close() {
            destination.close();
        }
    
    }
}