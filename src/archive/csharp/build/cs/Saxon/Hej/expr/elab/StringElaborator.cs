using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public abstract class StringElaborator : Saxon.Hej.expr.elab.Elaborator {
        public virtual bool returnZeroLengthWhenAbsent() {
            return false;
        }
        public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
            Saxon.Hej.expr.elab.UnicodeStringEvaluator strEval = elaborateForUnicodeString(returnZeroLengthWhenAbsent());
            return context => {
                Saxon.Hej.str.UnicodeString value = /*Delegate.eval()*/strEval(context);
                if ((value == null)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
                } else {
                    return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(Saxon.Hej.value.StringValue.makeUStringValue(value));
                }
            } ;
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
        public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
            Saxon.Hej.expr.elab.UnicodeStringEvaluator strEval = elaborateForUnicodeString(returnZeroLengthWhenAbsent());
            return context => {
                Saxon.Hej.str.UnicodeString value = /*Delegate.eval()*/strEval(context);
                if ((value == null)) {
                    return null;
                } else {
                    return Saxon.Hej.value.StringValue.makeUStringValue(value);
                }
            } ;
        }
        public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
            Saxon.Hej.expr.elab.UnicodeStringEvaluator strEval = elaborateForUnicodeString(returnZeroLengthWhenAbsent());
            return context => {
                Saxon.Hej.str.UnicodeString value = /*Delegate.eval()*/strEval(context);
                return ((value != null) && !(value.isEmpty()));
            } ;
        }
        public abstract override Saxon.Hej.expr.elab.UnicodeStringEvaluator elaborateForUnicodeString(bool zeroLengthWhenAbsent);
    
    }
}