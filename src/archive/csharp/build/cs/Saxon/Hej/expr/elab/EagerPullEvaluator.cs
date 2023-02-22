using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public class EagerPullEvaluator : Saxon.Hej.expr.elab.SequenceEvaluator {
        readonly Saxon.Hej.expr.elab.PullEvaluator puller;
        public EagerPullEvaluator(Saxon.Hej.expr.elab.PullEvaluator select)  {
            this.puller = select;
        }
        public Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.XPathContext context) {
            try {
                return Saxon.Hej.om.SequenceTool.toGroundedValue(/*Delegate.iterate()*/puller(context));
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
    
    }
}