using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public class EagerPushEvaluator : Saxon.Hej.expr.elab.SequenceEvaluator {
        readonly Saxon.Hej.expr.elab.PushEvaluator pusher;
        public EagerPushEvaluator(Saxon.Hej.expr.elab.PushEvaluator select)  {
            this.pusher = select;
        }
        public Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.XPathContext context) {
            try {
                Saxon.Hej.Controller controller = context.getController();
                Saxon.Hej.@event.SequenceCollector seq = controller.allocateSequenceOutputter();
                Saxon.Hej.@event.ComplexContentOutputter @out = new Saxon.Hej.@event.ComplexContentOutputter(seq);
                @out.open();
                Saxon.Hej.expr.instruct.TailCall tail = /*Delegate.processLeavingTail()*/pusher(@out, context);
                Saxon.Hej.expr.Expression.dispatchTailCall(tail);
                @out.close();
                return seq.getSequence();
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
    
    }
}