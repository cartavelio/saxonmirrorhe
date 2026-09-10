using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public class StreamingArgumentEvaluator : Saxon.Hej.expr.elab.SequenceEvaluator {
        readonly Saxon.Hej.expr.Expression arg;
        public StreamingArgumentEvaluator(Saxon.Hej.expr.Expression arg)  {
            this.arg = arg;
        }
        public Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.XPathContext context) {
            try {
                return context.getConfiguration().obtainOptimizer().evaluateStreamingArgument(arg, context);
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
    
    }
}