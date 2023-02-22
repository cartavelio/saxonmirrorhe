using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class OuterForExpression : Saxon.Hej.expr.ForExpression {
        protected internal override int getRangeVariableCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.Expression sequence0 = getSequence();
            getSequenceOp().optimize(visitor, contextItemType);
            Saxon.Hej.expr.Expression action0 = getAction();
            getActionOp().optimize(visitor, contextItemType);
            if (((sequence0 != getSequence()) || (action0 != getAction()))) {
                return optimize(visitor, contextItemType);
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.flwor.OuterForExpression forExp = new Saxon.Hej.expr.flwor.OuterForExpression();
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, forExp);
            forExp.setRequiredType(requiredType);
            forExp.setVariableQName(variableName);
            forExp.setSequence(getSequence().copy(rebindings));
            rebindings.put(this, forExp);
            Saxon.Hej.expr.Expression newAction = getAction().copy(rebindings);
            forExp.setAction(newAction);
            forExp.variableName = variableName;
            return forExp;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return /*Delegate.iterate()*/makeElaborator().elaborateForPull()(context);
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            dispatchTailCall(/*Delegate.processLeavingTail()*/makeElaborator().elaborateForPush()(output, context));
        }
        public override string getExpressionName() {
            return "outerFor";
        }
        protected internal override string allowingEmptyString() {
            return " allowing empty";
        }
        protected internal override void explainSpecializedAttributes(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.emitAttribute("outer", "true");
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.flwor.OuterForExpression.OuterForExprElaborator();
        }
        public class OuterForExprElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.ForExpression expr = (Saxon.Hej.expr.ForExpression)(getExpression());
                Saxon.Hej.expr.elab.PullEvaluator selectEval = expr.getSequence().makeElaborator().elaborateForPull();
                int actionCardinality = expr.getAction().getCardinality();
                int slot = expr.getLocalSlotNumber();
                Saxon.Hej.expr.elab.PullEvaluator actionEval = expr.getAction().makeElaborator().elaborateForPull();
                return context => {
                    Saxon.Hej.om.SequenceIterator @base = /*Delegate.iterate()*/selectEval(context);
                    Saxon.Hej.tree.iter.LookaheadIterator ahead = Saxon.Hej.tree.iter.LookaheadIteratorImpl.makeLookaheadIterator(@base);
                    if (ahead.hasNext()) {
                        return Saxon.Hej.expr.MappingIterator.map(ahead, item => {
                            context.setLocalVariable(slot, item);
                            return /*Delegate.iterate()*/actionEval(context);
                        } );
                    } else {
                        context.setLocalVariable(slot, Saxon.Hej.value.EmptySequence.getInstance());
                        return /*Delegate.iterate()*/actionEval(context);
                    }
                } ;
            }
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.ForExpression expr = (Saxon.Hej.expr.ForExpression)(getExpression());
                Saxon.Hej.expr.elab.PullEvaluator selectEval = expr.getSequence().makeElaborator().elaborateForPull();
                Saxon.Hej.expr.elab.PushEvaluator actionEval = expr.getAction().makeElaborator().elaborateForPush();
                int slot = expr.getLocalSlotNumber();
                return (output, context) => {
                    Saxon.Hej.om.SequenceIterator @base = /*Delegate.iterate()*/selectEval(context);
                    Saxon.Hej.tree.iter.LookaheadIterator ahead = Saxon.Hej.tree.iter.LookaheadIteratorImpl.makeLookaheadIterator(@base);
                    if (ahead.hasNext()) {
                        while (true) {
                            Saxon.Hej.om.Item item = ahead.next();
                            if ((item == null)) {
                                break ;
                            }
                            context.setLocalVariable(slot, item);
                            dispatchTailCall(/*Delegate.processLeavingTail()*/actionEval(output, context));
                        }
                    } else {
                        context.setLocalVariable(slot, Saxon.Hej.value.EmptySequence.getInstance());
                        dispatchTailCall(/*Delegate.processLeavingTail()*/actionEval(output, context));
                    }
                    return null;
                } ;
            }
            public override Saxon.Hej.expr.elab.UpdateEvaluator elaborateForUpdate() {
                Saxon.Hej.expr.ForExpression expr = (Saxon.Hej.expr.ForExpression)(getExpression());
                Saxon.Hej.expr.elab.PullEvaluator selectEval = expr.getSequence().makeElaborator().elaborateForPull();
                Saxon.Hej.expr.elab.UpdateEvaluator actionEval = expr.getAction().makeElaborator().elaborateForUpdate();
                int slot = expr.getLocalSlotNumber();
                return (context, pul) => {
                    Saxon.Hej.om.SequenceIterator @base = /*Delegate.iterate()*/selectEval(context);
                    Saxon.Hej.tree.iter.LookaheadIterator ahead = Saxon.Hej.tree.iter.LookaheadIteratorImpl.makeLookaheadIterator(@base);
                    if (ahead.hasNext()) {
                        while (true) {
                            Saxon.Hej.om.Item item = ahead.next();
                            if ((item == null)) {
                                break ;
                            }
                            context.setLocalVariable(slot, item);
                            /*Delegate.registerUpdates()*/actionEval(context, pul);
                        }
                    } else {
                        context.setLocalVariable(slot, Saxon.Hej.value.EmptySequence.getInstance());
                        /*Delegate.registerUpdates()*/actionEval(context, pul);
                    }
                } ;
            }
        
        }
    
    }
}