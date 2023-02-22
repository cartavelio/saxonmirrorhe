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
            Saxon.Hej.om.SequenceIterator @base = getSequence().iterate(context);
            Saxon.Hej.tree.iter.LookaheadIterator ahead = Saxon.Hej.tree.iter.LookaheadIteratorImpl.makeLookaheadIterator(@base);
            if (ahead.hasNext()) {
                return Saxon.Hej.expr.MappingIterator.map(ahead, item => {
                    context.setLocalVariable(slotNumber, item);
                    return getAction().iterate(context);
                } );
            } else {
                context.setLocalVariable(getLocalSlotNumber(), Saxon.Hej.value.EmptySequence.getInstance());
                return getAction().iterate(context);
            }
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.SequenceIterator @base = getSequence().iterate(context);
            int slot = getLocalSlotNumber();
            Saxon.Hej.tree.iter.LookaheadIterator ahead = Saxon.Hej.tree.iter.LookaheadIteratorImpl.makeLookaheadIterator(@base);
            if (ahead.hasNext()) {
                while (true) {
                    Saxon.Hej.om.Item item = ahead.next();
                    if ((item == null)) {
                        break ;
                    }
                    context.setLocalVariable(slot, item);
                    getAction().process(output, context);
                }
            } else {
                context.setLocalVariable(getLocalSlotNumber(), Saxon.Hej.value.EmptySequence.getInstance());
                getAction().process(output, context);
            }
        }
        public override string getExpressionName() {
            return "outerFor";
        }
        public override void evaluatePendingUpdates(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.PendingUpdateList pul) {
            Saxon.Hej.om.SequenceIterator @base = getSequence().iterate(context);
            int slot = getLocalSlotNumber();
            Saxon.Hej.tree.iter.LookaheadIterator ahead = Saxon.Hej.tree.iter.LookaheadIteratorImpl.makeLookaheadIterator(@base);
            if (ahead.hasNext()) {
                while (true) {
                    Saxon.Hej.om.Item item = ahead.next();
                    if ((item == null)) {
                        break ;
                    }
                    context.setLocalVariable(slot, item);
                    getAction().evaluatePendingUpdates(context, pul);
                }
            } else {
                context.setLocalVariable(getLocalSlotNumber(), Saxon.Hej.value.EmptySequence.getInstance());
                getAction().evaluatePendingUpdates(context, pul);
            }
        }
        protected internal override void explainSpecializedAttributes(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.emitAttribute("outer", "true");
        }
    
    }
}