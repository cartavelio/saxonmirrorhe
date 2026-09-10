using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public abstract class ItemElaborator : Saxon.Hej.expr.elab.Elaborator {
        public override Saxon.Hej.expr.elab.SequenceEvaluator eagerly() {
            bool maybeEmpty = Saxon.Hej.value.Cardinality.allowsZero(getExpression().getCardinality());
            Saxon.Hej.expr.elab.ItemEvaluator ie = elaborateForItem();
            if (maybeEmpty) {
                return new Saxon.Hej.expr.elab.OptionalItemEvaluator(ie);
            } else {
                return new Saxon.Hej.expr.elab.SingleItemEvaluator(ie);
            }
        }
        public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
            Saxon.Hej.expr.elab.ItemEvaluator ie = elaborateForItem();
            return context => Saxon.Hej.tree.iter.SingletonIterator.makeIterator(/*Delegate.eval()*/ie(context));
        }
        public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
            Saxon.Hej.expr.elab.ItemEvaluator ie = elaborateForItem();
            return (@out, context) => {
                Saxon.Hej.om.Item it = /*Delegate.eval()*/ie(context);
                if ((it != null)) {
                    @out.append(it);
                }
                return null;
            } ;
        }
        public abstract override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem();
        public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
            Saxon.Hej.expr.elab.ItemEvaluator ie = elaborateForItem();
            return context => Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(/*Delegate.eval()*/ie(context));
        }
        public override Saxon.Hej.expr.elab.UnicodeStringEvaluator elaborateForUnicodeString(bool zeroLengthWhenAbsent) {
            Saxon.Hej.expr.elab.ItemEvaluator ie = elaborateForItem();
            return context => {
                Saxon.Hej.om.Item item = /*Delegate.eval()*/ie(context);
                return ((item == null) ? handleNullUnicodeString(zeroLengthWhenAbsent) : item.getUnicodeStringValue());
            } ;
        }
    
    }
}