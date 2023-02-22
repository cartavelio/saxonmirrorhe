using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class ForMemberClausePush : Saxon.Hej.expr.flwor.TuplePush {
        protected internal Saxon.Hej.expr.flwor.TuplePush destination;
        protected internal Saxon.Hej.expr.flwor.ForClause forClause;
        public ForMemberClausePush(Saxon.Hej.@event.Outputter outputter, Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.expr.flwor.ForClause forClause) : base (outputter) {
            this.destination = destination;
            this.forClause = forClause;
        }
        public override void processTuple(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.Expression sequence = forClause.getSequence();
            Saxon.Hej.ma.arrays.ArrayItem arrayItem = (Saxon.Hej.ma.arrays.ArrayItem)(sequence.evaluateItem(context));
            int pos = 0;
            foreach (Saxon.Hej.om.GroundedValue member in arrayItem.members()) {
                context.setLocalVariable(forClause.getRangeVariable().getLocalSlotNumber(), member);
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