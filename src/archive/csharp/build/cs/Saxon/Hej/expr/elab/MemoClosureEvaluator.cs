using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public class MemoClosureEvaluator : Saxon.Hej.expr.elab.SequenceEvaluator {
        readonly Saxon.Hej.expr.Expression input;
        readonly Saxon.Hej.expr.elab.PullEvaluator inputEvaluator;
        readonly bool singleton;
        public MemoClosureEvaluator(Saxon.Hej.expr.Expression input, Saxon.Hej.expr.elab.PullEvaluator inputEvaluator)  {
            this.input = input;
            this.inputEvaluator = inputEvaluator;
            this.singleton = !(Saxon.Hej.value.Cardinality.allowsMany(input.getCardinality()));
        }
        public Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.XPathContext context) {
            try {
                return (singleton ? new Saxon.Hej.value.SingletonClosure(input, inputEvaluator, context) : new Saxon.Hej.value.MemoClosure(input, inputEvaluator, context));
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
    
    }
}