using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class ForExpression : Saxon.Hej.expr.Assignation {
        private int actionCardinality = Saxon.Hej.expr.StaticProperty.ALLOWS_MANY;
        public ForExpression()  {
        }
        public override string getExpressionName() {
            return "for";
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getSequenceOp().typeCheck(visitor, contextInfo);
            if ((Saxon.Hej.expr.Literal.isEmptySequence(getSequence()) && !(((this is Saxon.Hej.expr.flwor.OuterForExpression))))) {
                return getSequence();
            }
            if ((requiredType != null)) {
                Saxon.Hej.value.SequenceType decl = requiredType;
                Saxon.Hej.value.SequenceType sequenceType = Saxon.Hej.value.SequenceType.makeSequenceType(decl.getPrimaryType(), Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE);
                Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.VARIABLE, variableName.getDisplayName(), 0);
                setSequence(Saxon.Hej.expr.parser.TypeChecker.strictTypeCheck(getSequence(), sequenceType, role, visitor.getStaticContext()));
                Saxon.Hej.type.ItemType actualItemType = getSequence().getItemType();
                refineTypeInformation(actualItemType, getRangeVariableCardinality(), null, getSequence().getSpecialProperties(), this);
            }
            if (Saxon.Hej.expr.Literal.isEmptySequence(getAction())) {
                return getAction();
            }
            getActionOp().typeCheck(visitor, contextInfo);
            actionCardinality = getAction().getCardinality();
            return this;
        }
        protected internal virtual int getRangeVariableCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.expr.parser.Optimizer opt = visitor.obtainOptimizer();
            bool debug = config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.TRACE_OPTIMIZER_DECISIONS);
            if (Saxon.Hej.expr.instruct.Choose.isSingleBranchChoice(getAction())) {
                getActionOp().optimize(visitor, contextItemType);
            }
            Saxon.Hej.expr.Expression p = promoteWhereClause();
            if ((p != null)) {
                if (debug) {
                    opt.trace(("Promoted where clause in for $"+getVariableName()), p);
                }
                return p.optimize(visitor, contextItemType);
            }
            Saxon.Hej.expr.Expression seq0 = getSequence();
            getSequenceOp().optimize(visitor, contextItemType);
            if ((seq0 != getSequence())) {
                return optimize(visitor, contextItemType);
            }
            if ((Saxon.Hej.expr.Literal.isEmptySequence(getSequence()) && !(((this is Saxon.Hej.expr.flwor.OuterForExpression))))) {
                return getSequence();
            }
            Saxon.Hej.expr.Expression act0 = getAction();
            getActionOp().optimize(visitor, contextItemType);
            if ((act0 != getAction())) {
                return optimize(visitor, contextItemType);
            }
            if (Saxon.Hej.expr.Literal.isEmptySequence(getAction())) {
                return getAction();
            }
            if (((getSequence() is Saxon.Hej.expr.SlashExpression) && (getAction() is Saxon.Hej.expr.SlashExpression))) {
                Saxon.Hej.expr.SlashExpression path2 = (Saxon.Hej.expr.SlashExpression)(getAction());
                Saxon.Hej.expr.Expression start2 = path2.getSelectExpression();
                Saxon.Hej.expr.Expression step2 = path2.getActionExpression();
                if (((((start2 is Saxon.Hej.expr.VariableReference) && (((Saxon.Hej.expr.VariableReference)(start2)).getBinding() == this)) && (Saxon.Hej.expr.parser.ExpressionTool.getReferenceCount(getAction(), this, false) == 1)) && ((((step2.getDependencies()&((Saxon.Hej.expr.StaticProperty.DEPENDS_ON_POSITION|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LAST)))) == 0)))) {
                    Saxon.Hej.expr.Expression newPath = new Saxon.Hej.expr.SlashExpression(getSequence(), path2.getActionExpression());
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, newPath);
                    newPath = newPath.simplify().typeCheck(visitor, contextItemType);
                    if ((newPath is Saxon.Hej.expr.SlashExpression)) {
                        if (debug) {
                            opt.trace((("Collapsed return clause of for $"+getVariableName())+" into path expression"), newPath);
                        }
                        return newPath.optimize(visitor, contextItemType);
                    }
                }
            }
            if (((getAction() is Saxon.Hej.expr.VariableReference) && (((Saxon.Hej.expr.VariableReference)(getAction())).getBinding() == this))) {
                if (debug) {
                    opt.trace(("Collapsed redundant for expression $"+getVariableName()), getSequence());
                }
                return getSequence();
            }
            if ((getSequence().getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) {
                Saxon.Hej.expr.LetExpression let = new Saxon.Hej.expr.LetExpression();
                let.setVariableQName(variableName);
                let.setRequiredType(Saxon.Hej.value.SequenceType.makeSequenceType(getSequence().getItemType(), Saxon.Hej.expr.StaticProperty.EXACTLY_ONE));
                let.setSequence(getSequence());
                let.setAction(getAction());
                let.setSlotNumber(slotNumber);
                let.setRetainedStaticContextLocally(getRetainedStaticContext());
                Saxon.Hej.expr.parser.ExpressionTool.rebindVariableReferences(getAction(), this, let);
                return let.typeCheck(visitor, contextItemType).optimize(visitor, contextItemType);
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression unordered(bool retainAllNodes, bool forStreaming) {
            setSequence(getSequence().unordered(retainAllNodes, forStreaming));
            setAction(getAction().unordered(retainAllNodes, forStreaming));
            return this;
        }
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            return getAction().getIntegerBounds();
        }
        private Saxon.Hej.expr.Expression promoteWhereClause() {
            if (Saxon.Hej.expr.instruct.Choose.isSingleBranchChoice(getAction())) {
                Saxon.Hej.expr.Expression condition = ((Saxon.Hej.expr.instruct.Choose)(getAction())).getCondition(0);
                Saxon.Hej.expr.Binding[] bindingList = new Saxon.Hej.expr.Binding[]{this};
                System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(5);
                Saxon.Hej.expr.Expression promotedCondition = null;
                Saxon.Hej.expr.BooleanExpression.listAndComponents(condition, list);
                for (int i = (list.Count-1); (i>=0); i--) {
                    Saxon.Hej.expr.Expression term = list[i];
                    if (!(Saxon.Hej.expr.parser.ExpressionTool.dependsOnVariable(term, bindingList))) {
                        if ((promotedCondition == null)) {
                            promotedCondition = term;
                        } else {
                            promotedCondition = new Saxon.Hej.expr.AndExpression(term, promotedCondition);
                        }
                        Saxon.Impl.Helpers.ListUtils.Remove(list, i);
                    }
                }
                if ((promotedCondition != null)) {
                    if (list.Count==0) {
                        Saxon.Hej.expr.Expression oldThen = ((Saxon.Hej.expr.instruct.Choose)(getAction())).getAction(0);
                        setAction(oldThen);
                        return Saxon.Hej.expr.instruct.Choose.makeConditional(promotedCondition, this);
                    } else {
                        Saxon.Hej.expr.Expression retainedCondition = list[0];
                        for (int i = 1; (i<list.Count); i++) {
                            retainedCondition = new Saxon.Hej.expr.AndExpression(retainedCondition, list[i]);
                        }
                        ((Saxon.Hej.expr.instruct.Choose)(getAction())).setCondition(0, retainedCondition);
                        Saxon.Hej.expr.Expression newIf = Saxon.Hej.expr.instruct.Choose.makeConditional(promotedCondition, this, Saxon.Hej.expr.Literal.makeEmptySequence());
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, newIf);
                        return newIf;
                    }
                }
            }
            return null;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.ForExpression forExp = new Saxon.Hej.expr.ForExpression();
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, forExp);
            forExp.setRequiredType(requiredType);
            forExp.setVariableQName(variableName);
            forExp.setSequence(getSequence().copy(rebindings));
            rebindings.put(this, forExp);
            Saxon.Hej.expr.Expression newAction = getAction().copy(rebindings);
            forExp.setAction(newAction);
            forExp.variableName = variableName;
            forExp.slotNumber = slotNumber;
            return forExp;
        }
        public override int markTailFunctionCalls(Saxon.Hej.om.StructuredQName qName, int arity) {
            if (!(Saxon.Hej.value.Cardinality.allowsMany(getSequence().getCardinality()))) {
                return Saxon.Hej.expr.parser.ExpressionTool.markTailFunctionCalls(getAction(), qName, arity);
            } else {
                return Saxon.Hej.expr.UserFunctionCall.NOT_TAIL_CALL;
            }
        }
        public override bool isVacuousExpression() {
            return getAction().isVacuousExpression();
        }
        public override int getImplementationMethod() {
            return (ITERATE_METHOD|PROCESS_METHOD);
        }
        public override void checkPermittedContents(Saxon.Hej.type.SchemaType parentType, bool whole) {
            getAction().checkPermittedContents(parentType, false);
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.SequenceIterator @base = getSequence().iterate(context);
            Saxon.Hej.expr.ForExpression.MappingAction map = new Saxon.Hej.expr.ForExpression.MappingAction(context, getLocalSlotNumber(), getAction());
            switch (actionCardinality) {
              case Saxon.Hej.expr.StaticProperty.EXACTLY_ONE:
                return new Saxon.Hej.expr.ItemMappingIterator(@base, map, true);
              case Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE:
                return new Saxon.Hej.expr.ItemMappingIterator(@base, map, false);
              default:
                return new Saxon.Hej.expr.MappingIterator(@base, map);
            }
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            int slot = getLocalSlotNumber();
            Saxon.Hej.om.SequenceTool.supply(getSequence().iterate(context), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => {
                context.setLocalVariable(slot, item);
                getAction().process(output, context);
            } )));
        }
        public override void evaluatePendingUpdates(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.PendingUpdateList pul) {
            try {
                int slot = getLocalSlotNumber();
                Saxon.Hej.om.SequenceTool.supply(getSequence().iterate(context), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => {
                    context.setLocalVariable(slot, item);
                    getAction().evaluatePendingUpdates(context, pul);
                } )));
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getAction().getItemType();
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return getAction().getStaticUType(contextItemType);
        }
        protected internal override int computeCardinality() {
            int c1 = getSequence().getCardinality();
            int c2 = getAction().getCardinality();
            return Saxon.Hej.value.Cardinality.multiply(c1, c2);
        }
        public override string ToString() {
            return ((((("for $"+getVariableEQName())+" in ")+(((getSequence() == null) ? "(...)" : getSequence().ToString())))+" return ")+(((getAction() == null) ? "(...)" : Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getAction()))));
        }
        public override string toShortString() {
            return ((((("for $"+getVariableQName().getDisplayName())+" in ")+(((getSequence() == null) ? "(...)" : getSequence().toShortString())))+" return ")+(((getAction() == null) ? "(...)" : getAction().toShortString())));
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("for", this);
            explainSpecializedAttributes(@out);
            @out.emitAttribute("var", getVariableQName());
            Saxon.Hej.type.ItemType varType = getSequence().getItemType();
            if ((varType != Saxon.Hej.type.AnyItemType.getInstance())) {
                @out.emitAttribute("as", Saxon.Hej.type.AlphaCode.fromItemType(varType));
            }
            @out.emitAttribute("slot", (""+getLocalSlotNumber()));
            @out.setChildRole("in");
            getSequence().export(@out);
            @out.setChildRole("return");
            getAction().export(@out);
            @out.endElement();
        }
        protected internal virtual void explainSpecializedAttributes(Saxon.Hej.trace.ExpressionPresenter @out) {
        }
        public class MappingAction : Saxon.Hej.expr.MappingFunction, Saxon.Hej.expr.ItemMappingFunction {
            protected internal Saxon.Hej.expr.XPathContext context;
            private readonly int slotNumber;
            private readonly Saxon.Hej.expr.Expression action;
            public MappingAction(Saxon.Hej.expr.XPathContext context, int slotNumber, Saxon.Hej.expr.Expression action)  {
                this.context = context;
                this.slotNumber = slotNumber;
                this.action = action;
            }
            public Saxon.Hej.om.SequenceIterator map(Saxon.Hej.om.Item item) {
                context.setLocalVariable(slotNumber, item);
                return action.iterate(context);
            }
            public Saxon.Hej.om.Item mapItem(Saxon.Hej.om.Item item) {
                context.setLocalVariable(slotNumber, item);
                return action.evaluateItem(context);
            }
        
        }
        public override string getStreamerName() {
            return "ForExpression";
        }
    
    }
}