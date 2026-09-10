using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class ForClauseOuterPull : Saxon.Hej.expr.flwor.ForClausePull {
        public ForClauseOuterPull(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.flwor.ForClause forClause) : base (@base, forClause) {
        }
        public override bool nextTuple(Saxon.Hej.expr.XPathContext context) {
            while (true) {
                Saxon.Hej.om.Item next;
                if ((currentIteration == null)) {
                    if (!(@base.nextTuple(context))) {
                        return false;
                    }
                    currentIteration = Saxon.Hej.om.SequenceTool.focusTracker(/*Delegate.iterate()*/forClause.getSequence().makeElaborator().elaborateForPull()(context));
                    next = currentIteration.next();
                    if ((next == null)) {
                        context.setLocalVariable(forClause.getRangeVariable().getLocalSlotNumber(), Saxon.Hej.value.EmptySequence.getInstance());
                        if ((forClause.getPositionVariable() != null)) {
                            context.setLocalVariable(forClause.getPositionVariable().getLocalSlotNumber(), Saxon.Hej.value.Int64Value.ZERO);
                        }
                        currentIteration = null;
                        return true;
                    }
                } else {
                    next = currentIteration.next();
                }
                if ((next != null)) {
                    context.setLocalVariable(forClause.getRangeVariable().getLocalSlotNumber(), next);
                    if ((forClause.getPositionVariable() != null)) {
                        context.setLocalVariable(forClause.getPositionVariable().getLocalSlotNumber(), new Saxon.Hej.value.Int64Value(currentIteration.position()));
                    }
                    return true;
                } else {
                    currentIteration = null;
                }
            }
        }
        public override void close() {
            @base.close();
            if ((currentIteration != null)) {
                currentIteration.close();
            }
        }
    
    }
}