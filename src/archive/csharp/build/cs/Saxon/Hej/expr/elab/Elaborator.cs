using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public abstract class Elaborator {
        private Saxon.Hej.expr.Expression expression;
        public Elaborator()  {
        }
        public Saxon.Hej.expr.Expression getExpression() {
            return expression;
        }
        public virtual void setExpression(Saxon.Hej.expr.Expression expr) {
            this.expression = expr;
        }
        protected internal Saxon.Hej.Configuration getConfiguration() {
            return expression.getConfiguration();
        }
        public virtual Saxon.Hej.expr.elab.SequenceEvaluator eagerly() {
            Saxon.Hej.expr.Expression expr = getExpression();
            int m = expr.getImplementationMethod();
            if (((((m&Saxon.Hej.expr.Expression.EVALUATE_METHOD)) != 0) && !(Saxon.Hej.value.Cardinality.allowsMany(expr.getCardinality())))) {
                Saxon.Hej.expr.elab.ItemEvaluator itemEvaluator = elaborateForItem();
                return new Saxon.Hej.expr.elab.SingleItemEvaluator(itemEvaluator);
            } else {
                if ((((m&Saxon.Hej.expr.Expression.ITERATE_METHOD)) != 0)) {
                    Saxon.Hej.expr.elab.PullEvaluator pullEvaluator = elaborateForPull();
                    return new Saxon.Hej.expr.elab.EagerPullEvaluator(pullEvaluator);
                } else {
                    Saxon.Hej.expr.elab.PushEvaluator pushEvaluator = elaborateForPush();
                    return new Saxon.Hej.expr.elab.EagerPushEvaluator(pushEvaluator);
                }
            }
        }
        public virtual Saxon.Hej.expr.elab.SequenceEvaluator lazily(bool repeatable) {
            Saxon.Hej.expr.Expression expr = getExpression();
            if (!(expr.supportsLazyEvaluation())) {
                return eagerly();
            } else {
                if (repeatable) {
                    return new Saxon.Hej.expr.elab.LearningEvaluator(expr, new Saxon.Hej.expr.elab.MemoClosureEvaluator(expr, elaborateForPull()));
                } else {
                    Saxon.Hej.expr.elab.PullEvaluator pullEvaluator = elaborateForPull();
                    return new Saxon.Hej.expr.elab.LazyPullEvaluator(pullEvaluator);
                }
            }
        }
        public abstract Saxon.Hej.expr.elab.PullEvaluator elaborateForPull();
        public abstract Saxon.Hej.expr.elab.PushEvaluator elaborateForPush();
        public abstract Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem();
        public abstract Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean();
        public abstract Saxon.Hej.expr.elab.UnicodeStringEvaluator elaborateForUnicodeString(bool zeroLengthWhenAbsent);
        public virtual Saxon.Hej.expr.elab.StringEvaluator elaborateForString(bool zeroLengthWhenAbsent) {
            return context => /*Delegate.eval()*/elaborateForUnicodeString(zeroLengthWhenAbsent)(context).ToString();
        }
        protected internal Saxon.Hej.str.UnicodeString handleNullUnicodeString(bool zeroLengthWhenAbsent) {
            return (zeroLengthWhenAbsent ? Saxon.Hej.str.EmptyUnicodeString.getInstance() : null);
        }
        protected internal Saxon.Hej.str.UnicodeString handlePossiblyNullUnicodeString(Saxon.Hej.str.UnicodeString str, bool zeroLengthWhenAbsent) {
            if (((str == null) && zeroLengthWhenAbsent)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            } else {
                return str;
            }
        }
        protected internal string handleNullString(bool zeroLengthWhenAbsent) {
            return (zeroLengthWhenAbsent ? "" : null);
        }
        protected internal string handlePossiblyNullString(string str, bool zeroLengthWhenAbsent) {
            if (((str == null) && zeroLengthWhenAbsent)) {
                return "";
            } else {
                return str;
            }
        }
        public virtual Saxon.Hej.expr.elab.UpdateEvaluator elaborateForUpdate() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Update not supported");
        }
    
    }
}