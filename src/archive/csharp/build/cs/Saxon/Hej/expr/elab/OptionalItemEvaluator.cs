using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public class OptionalItemEvaluator : Saxon.Hej.expr.elab.SequenceEvaluator {
        readonly Saxon.Hej.expr.elab.ItemEvaluator evaluator;
        public OptionalItemEvaluator(Saxon.Hej.expr.elab.ItemEvaluator eval)  {
            this.evaluator = eval;
        }
        public Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.XPathContext context) {
            try {
                Saxon.Hej.om.Item result = /*Delegate.eval()*/evaluator(context);
                return ((result == null) ? Saxon.Hej.value.EmptySequence.getInstance() : result);
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
    
    }
}