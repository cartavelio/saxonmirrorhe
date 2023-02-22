using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class ForMemberClausePush : Saxon.Hej.expr.flwor.TuplePush {
        protected internal Saxon.Hej.expr.flwor.TuplePush destination;
        protected internal Saxon.Hej.expr.flwor.ForMemberClause forMemberClause;
        public ForMemberClausePush(Saxon.Hej.@event.Outputter outputter, Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.expr.flwor.ForMemberClause forClause) : base (outputter) {
            this.destination = destination;
            this.forMemberClause = forClause;
        }
        public override void processTuple(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.ma.arrays.ArrayItem arrayItem = forMemberClause.evaluateArrayExpression(context);
            int pos = 0;
            foreach (Saxon.Hej.om.GroundedValue member in arrayItem.members()) {
                context.setLocalVariable(forMemberClause.getRangeVariable().getLocalSlotNumber(), member);
                if ((forMemberClause.getPositionVariable() != null)) {
                    context.setLocalVariable(forMemberClause.getPositionVariable().getLocalSlotNumber(), new Saxon.Hej.value.Int64Value(++pos));
                }
                destination.processTuple(context);
            }
        }
        public override void close() {
            destination.close();
        }
    
    }
}