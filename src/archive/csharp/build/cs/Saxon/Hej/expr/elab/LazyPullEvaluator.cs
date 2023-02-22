using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public class LazyPullEvaluator : Saxon.Hej.expr.elab.SequenceEvaluator {
        readonly Saxon.Hej.expr.elab.PullEvaluator puller;
        public LazyPullEvaluator(Saxon.Hej.expr.elab.PullEvaluator select)  {
            this.puller = select;
        }
        public Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.XPathContext context) {
            try {
                return new Saxon.Hej.om.LazySequence(/*Delegate.iterate()*/puller(context));
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
    
    }
}