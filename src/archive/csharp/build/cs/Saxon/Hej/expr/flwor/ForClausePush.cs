using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class ForClausePush : Saxon.Hej.expr.flwor.TuplePush {
        protected internal Saxon.Hej.expr.flwor.TuplePush destination;
        protected internal Saxon.Hej.expr.flwor.ForClause forClause;
        public ForClausePush(Saxon.Hej.@event.Outputter outputter, Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.expr.flwor.ForClause forClause) : base (outputter) {
            this.destination = destination;
            this.forClause = forClause;
        }
        public override void processTuple(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.SequenceIterator iter = forClause.getIterator(context);
            int pos = 0;
            Saxon.Hej.om.Item next;
            while (((next = iter.next()) != null)) {
                context.setLocalVariable(forClause.getRangeVariable().getLocalSlotNumber(), next);
                if ((forClause.getPositionVariable() != null)) {
                    context.setLocalVariable(forClause.getPositionVariable().getLocalSlotNumber(), new Saxon.Hej.value.Int64Value(++pos));
                }
                destination.processTuple(context);
            }
        }
        public override void close() {
            destination.close();
        }
    
    }
}