using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public class FallbackElaborator : Saxon.Hej.expr.elab.Elaborator {
        public FallbackElaborator()  {
        }
        public override Saxon.Hej.expr.elab.SequenceEvaluator eagerly() {
            return new Saxon.Hej.expr.elab.EagerPullEvaluator(elaborateForPull());
        }
        public override Saxon.Hej.expr.elab.SequenceEvaluator lazily(bool repeatable) {
            Saxon.Hej.expr.Expression expr = getExpression();
            if (repeatable) {
                return new Saxon.Hej.expr.elab.MemoClosureEvaluator(expr, elaborateForPull());
            } else {
                return new Saxon.Hej.expr.elab.LazyPullEvaluator(elaborateForPull());
            }
        }
        public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
            return context => getExpression().iterate(context);
        }
        public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
            return (output, context) => {
                getExpression().process(output, context);
                return null;
            } ;
        }
        public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
            return context => getExpression().evaluateItem(context);
        }
        public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
            return context => getExpression().effectiveBooleanValue(context);
        }
        public override Saxon.Hej.expr.elab.UnicodeStringEvaluator elaborateForUnicodeString(bool zeroLengthWhenAbsent) {
            if (zeroLengthWhenAbsent) {
                return context => getExpression().evaluateAsString(context);
            } else {
                return context => {
                    Saxon.Hej.om.Item item = getExpression().evaluateItem(context);
                    return ((item == null) ? null : item.getUnicodeStringValue());
                } ;
            }
        }
    
    }
}