using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public class LearningEvaluator : Saxon.Hej.expr.elab.SequenceEvaluator {
        private const int EVAL_LIMIT = 20;
        private readonly Saxon.Hej.expr.Expression expression;
        private Saxon.Hej.expr.elab.SequenceEvaluator evaluator;
        private int completed = 0;
        private int count = 0;
        private bool flipped;
        public LearningEvaluator(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.elab.SequenceEvaluator lazy)  {
            this.expression = expr;
            this.evaluator = lazy;
        }
        public Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.XPathContext context) {
            if ((count>EVAL_LIMIT)) {
                return evaluator.evaluate(context);
            } else {
                Saxon.Hej.om.Sequence result = evaluator.evaluate(context);
                if ((result is Saxon.Hej.value.Closure)) {
                    ((Saxon.Hej.value.Closure)(result)).setLearningEvaluator(this, count);
                } else {
                    flipped = true;
                }
                count++;
                return result;
            }
        }
        public void reportCompletion(int serialNumber) {
            if (((completed++>=EVAL_LIMIT) && (completed == count))) {
                evaluator = (expression.makeElaborator().eagerly());
                count = System.Int32.MaxValue;
                flipped = true;
            }
        }
        public bool hasDecidedToEvaluateEagerly() {
            return flipped;
        }
    
    }
}