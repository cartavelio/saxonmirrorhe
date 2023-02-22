using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public abstract class PullElaborator : Saxon.Hej.expr.elab.Elaborator {
        public abstract override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull();
        public override Saxon.Hej.expr.elab.SequenceEvaluator eagerly() {
            Saxon.Hej.expr.elab.PullEvaluator pull = elaborateForPull();
            return new Saxon.Hej.expr.elab.EagerPullEvaluator(pull);
        }
        public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
            Saxon.Hej.expr.elab.PullEvaluator pull = elaborateForPull();
            return (@out, context) => {
                try {
                    Saxon.Hej.om.SequenceIterator iter = /*Delegate.iterate()*/pull(context);
                    for (Saxon.Hej.om.Item it; ((it = iter.next()) != null); ) {
                        @out.append(it);
                    }
                    return null;
                } catch (Saxon.Hej.trans.UncheckedXPathException err) {
                    throw err.getXPathException();
                }
            } ;
        }
        public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
            Saxon.Hej.expr.elab.PullEvaluator pull = elaborateForPull();
            return context => {
                try {
                    return /*Delegate.iterate()*/pull(context).next();
                } catch (Saxon.Hej.trans.UncheckedXPathException err) {
                    throw err.getXPathException();
                }
            } ;
        }
        public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
            Saxon.Hej.expr.elab.PullEvaluator pull = elaborateForPull();
            return context => {
                try {
                    return Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(/*Delegate.iterate()*/pull(context));
                } catch (Saxon.Hej.trans.UncheckedXPathException err) {
                    throw err.getXPathException();
                }
            } ;
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