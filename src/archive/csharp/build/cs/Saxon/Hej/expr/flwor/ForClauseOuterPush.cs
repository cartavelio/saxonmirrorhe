using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class ForClauseOuterPush : Saxon.Hej.expr.flwor.TuplePush {
        protected internal Saxon.Hej.expr.flwor.TuplePush destination;
        protected internal Saxon.Hej.expr.flwor.ForClause forClause;
        public ForClauseOuterPush(Saxon.Hej.@event.Outputter outputter, Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.expr.flwor.ForClause forClause) : base (outputter) {
            this.destination = destination;
            this.forClause = forClause;
        }
        public override void processTuple(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.SequenceIterator iter = forClause.getSequence().iterate(context);
            int pos = 0;
            Saxon.Hej.om.Item next = iter.next();
            if ((next == null)) {
                context.setLocalVariable(forClause.getRangeVariable().getLocalSlotNumber(), Saxon.Hej.value.EmptySequence.getInstance());
                if ((forClause.getPositionVariable() != null)) {
                    context.setLocalVariable(forClause.getPositionVariable().getLocalSlotNumber(), Saxon.Hej.value.Int64Value.ZERO);
                }
                destination.processTuple(context);
            } else {
                while (true) {
                    context.setLocalVariable(forClause.getRangeVariable().getLocalSlotNumber(), next);
                    if ((forClause.getPositionVariable() != null)) {
                        context.setLocalVariable(forClause.getPositionVariable().getLocalSlotNumber(), new Saxon.Hej.value.Int64Value(++pos));
                    }
                    destination.processTuple(context);
                    next = iter.next();
                    if ((next == null)) {
                        break ;
                    }
                }
            }
        }
        public override void close() {
            destination.close();
        }
    
    }
}