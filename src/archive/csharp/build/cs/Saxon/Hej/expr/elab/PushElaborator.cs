using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public abstract class PushElaborator : Saxon.Hej.expr.elab.Elaborator {
        public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
            Saxon.Hej.expr.Expression expr = getExpression();
            if (Saxon.Hej.value.Cardinality.allowsMany(expr.getCardinality())) {
                Saxon.Hej.expr.elab.PushEvaluator pushEval = elaborateForPush();
                return context => {
                    Saxon.Hej.Controller controller = context.getController();
                    System.Diagnostics.Debug.Assert((controller != null));
                    Saxon.Hej.@event.SequenceCollector seq = controller.allocateSequenceOutputter();
                    Saxon.Hej.expr.instruct.TailCall tc = /*Delegate.processLeavingTail()*/pushEval(new Saxon.Hej.@event.ComplexContentOutputter(seq), context);
                    Saxon.Hej.expr.Expression.dispatchTailCall(tc);
                    seq.close();
                    return seq.iterate();
                } ;
            } else {
                Saxon.Hej.expr.elab.ItemEvaluator itemEval = elaborateForItem();
                return context => {
                    Saxon.Hej.om.Item item = /*Delegate.eval()*/itemEval(context);
                    return ((item == null) ? Saxon.Hej.tree.iter.EmptyIterator.getInstance() : Saxon.Hej.tree.iter.SingletonIterator.makeIterator(item));
                } ;
            }
        }
        public override Saxon.Hej.expr.elab.SequenceEvaluator eagerly() {
            Saxon.Hej.expr.Expression expr = getExpression();
            if (Saxon.Hej.value.Cardinality.allowsMany(expr.getCardinality())) {
                Saxon.Hej.expr.elab.PushEvaluator pushEval = elaborateForPush();
                return new Saxon.Hej.expr.elab.EagerPushEvaluator(pushEval);
            } else {
                return new Saxon.Hej.expr.elab.OptionalItemEvaluator(elaborateForItem());
            }
        }
        public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
            Saxon.Hej.expr.elab.PushEvaluator pushEval = elaborateForPush();
            return context => {
                Saxon.Hej.Controller controller = context.getController();
                System.Diagnostics.Debug.Assert((controller != null));
                Saxon.Hej.@event.SequenceCollector seq = controller.allocateSequenceOutputter(1);
                Saxon.Hej.expr.instruct.TailCall tc = /*Delegate.processLeavingTail()*/pushEval(new Saxon.Hej.@event.ComplexContentOutputter(seq), context);
                Saxon.Hej.expr.Expression.dispatchTailCall(tc);
                seq.close();
                return seq.getFirstItem();
            } ;
        }
        public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
            Saxon.Hej.expr.elab.PullEvaluator pullEval = elaborateForPull();
            return context => {
                Saxon.Hej.om.SequenceIterator iter = /*Delegate.iterate()*/pullEval(context);
                return Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(iter);
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