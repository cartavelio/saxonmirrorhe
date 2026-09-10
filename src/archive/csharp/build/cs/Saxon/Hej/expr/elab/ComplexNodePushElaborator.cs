using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public abstract class ComplexNodePushElaborator : Saxon.Hej.expr.elab.FallbackElaborator {
        public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
            Saxon.Hej.expr.elab.PushEvaluator pushEval = this.elaborateForPush();
            return context => {
                Saxon.Hej.Controller controller = context.getController();
                if ((controller == null)) {
                    throw new Saxon.Hej.trans.NoDynamicContextException("No controller available");
                }
                Saxon.Hej.@event.SequenceCollector seq = controller.allocateSequenceOutputter(1);
                Saxon.Hej.expr.instruct.TailCall tc = /*Delegate.processLeavingTail()*/pushEval(new Saxon.Hej.@event.ComplexContentOutputter(seq), context);
                Saxon.Hej.expr.Expression.dispatchTailCall(tc);
                seq.close();
                Saxon.Hej.om.SequenceIterator result = seq.iterate();
                seq.reset();
                return result;
            } ;
        }
        public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
            Saxon.Hej.expr.elab.PushEvaluator pushEval = this.elaborateForPush();
            return context => {
                Saxon.Hej.Controller controller = context.getController();
                if ((controller == null)) {
                    throw new Saxon.Hej.trans.NoDynamicContextException("No controller available");
                }
                Saxon.Hej.@event.SequenceCollector seq = controller.allocateSequenceOutputter(1);
                Saxon.Hej.expr.instruct.TailCall tc = /*Delegate.processLeavingTail()*/pushEval(new Saxon.Hej.@event.ComplexContentOutputter(seq), context);
                Saxon.Hej.expr.Expression.dispatchTailCall(tc);
                seq.close();
                Saxon.Hej.om.Item result = seq.getFirstItem();
                seq.reset();
                return result;
            } ;
        }
        public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
    
    }
}