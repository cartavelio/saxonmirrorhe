using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public abstract class BooleanElaborator : Saxon.Hej.expr.elab.Elaborator {
        public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
            Saxon.Hej.expr.elab.BooleanEvaluator b = elaborateForBoolean();
            return context => Saxon.Hej.tree.iter.SingletonIterator.makeIterator(Saxon.Hej.value.BooleanValue.get(/*Delegate.eval()*/b(context)));
        }
        public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
            Saxon.Hej.expr.elab.BooleanEvaluator b = elaborateForBoolean();
            return (@out, context) => {
                @out.append(Saxon.Hej.value.BooleanValue.get(/*Delegate.eval()*/b(context)));
                return null;
            } ;
        }
        public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
            Saxon.Hej.expr.elab.BooleanEvaluator b = elaborateForBoolean();
            return context => Saxon.Hej.value.BooleanValue.get(/*Delegate.eval()*/b(context));
        }
        public abstract override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean();
        public override Saxon.Hej.expr.elab.UnicodeStringEvaluator elaborateForUnicodeString(bool zeroLengthWhenAbsent) {
            Saxon.Hej.expr.elab.BooleanEvaluator b = elaborateForBoolean();
            return context => (/*Delegate.eval()*/b(context) ? Saxon.Hej.str.StringConstants.TRUE : Saxon.Hej.str.StringConstants.FALSE);
        }
    
    }
}