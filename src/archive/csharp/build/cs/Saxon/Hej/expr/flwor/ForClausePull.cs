using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class ForClausePull : Saxon.Hej.expr.flwor.TuplePull {
        protected internal Saxon.Hej.expr.flwor.TuplePull @base;
        protected internal Saxon.Hej.expr.flwor.ForClause forClause;
        protected internal Saxon.Hej.om.FocusIterator currentIteration;
        public ForClausePull(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.flwor.ForClause forClause)  {
            this.@base = @base;
            this.forClause = forClause;
        }
        public override bool nextTuple(Saxon.Hej.expr.XPathContext context) {
            while (true) {
                if ((currentIteration == null)) {
                    if (!(@base.nextTuple(context))) {
                        return false;
                    }
                    currentIteration = Saxon.Hej.om.SequenceTool.focusTracker(forClause.getIterator(context));
                }
                Saxon.Hej.om.Item next = currentIteration.next();
                if ((next != null)) {
                    context.setLocalVariable(forClause.getRangeVariable().getLocalSlotNumber(), variableValue(next));
                    if ((forClause.getPositionVariable() != null)) {
                        context.setLocalVariable(forClause.getPositionVariable().getLocalSlotNumber(), new Saxon.Hej.value.Int64Value(currentIteration.position()));
                    }
                    return true;
                } else {
                    currentIteration = null;
                }
            }
        }
        protected internal virtual Saxon.Hej.om.GroundedValue variableValue(Saxon.Hej.om.Item item) {
            return item;
        }
        public override void close() {
            @base.close();
            if ((currentIteration != null)) {
                currentIteration.close();
            }
        }
    
    }
}