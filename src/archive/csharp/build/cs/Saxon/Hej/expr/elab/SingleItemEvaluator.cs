using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public class SingleItemEvaluator : Saxon.Hej.expr.elab.SequenceEvaluator {
        readonly Saxon.Hej.expr.elab.ItemEvaluator evaluator;
        public SingleItemEvaluator(Saxon.Hej.expr.elab.ItemEvaluator eval)  {
            this.evaluator = eval;
        }
        public Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.XPathContext context) {
            try {
                return /*Delegate.eval()*/evaluator(context);
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
    
    }
}