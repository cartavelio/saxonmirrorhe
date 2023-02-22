using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.optim {
    #pragma warning disable 659
    public sealed class IndexedFilterExpression : Saxon.Hej.expr.Expression {
        private readonly Saxon.Hej.expr.Operand baseOp;
        private readonly Saxon.Hej.expr.Operand useOp;
        private readonly Saxon.Hej.expr.Operand searchOp;
        private readonly Saxon.Hej.expr.sort.AtomicComparer comparer;
        private readonly bool convertUntypedToOther;
        public IndexedFilterExpression(Saxon.Hej.expr.VariableReference start, Saxon.Hej.expr.ComparisonExpression filter, bool indexFirstOperand)  {
            baseOp = new Saxon.Hej.expr.Operand(this, start, Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLING_SELECT);
            if (indexFirstOperand) {
                useOp = new Saxon.Hej.expr.Operand(this, filter.getLhsExpression(), Saxon.Hej.expr.FilterExpression.FILTER_PREDICATE);
                searchOp = new Saxon.Hej.expr.Operand(this, filter.getRhsExpression(), Saxon.Hej.expr.OperandRole.ATOMIC_SEQUENCE);
            } else {
                useOp = new Saxon.Hej.expr.Operand(this, filter.getRhsExpression(), Saxon.Hej.expr.FilterExpression.FILTER_PREDICATE);
                searchOp = new Saxon.Hej.expr.Operand(this, filter.getLhsExpression(), Saxon.Hej.expr.OperandRole.ATOMIC_SEQUENCE);
            }
            this.comparer = filter.getAtomicComparer();
            this.convertUntypedToOther = filter.convertsUntypedToOther();
            if (((start.getBinding() is Saxon.Hej.expr.instruct.GlobalVariable) && !(((Saxon.Hej.expr.instruct.GlobalVariable)(start.getBinding())).isIndexedVariable()))) {
                ((Saxon.Hej.expr.instruct.GlobalVariable)(start.getBinding())).setIndexedVariable();
            }
        }
        public IndexedFilterExpression(Saxon.Hej.expr.Expression @base, Saxon.Hej.expr.Expression use, Saxon.Hej.expr.Expression search, Saxon.Hej.expr.sort.AtomicComparer comparer, bool convertUntypedToOther)  {
            baseOp = new Saxon.Hej.expr.Operand(this, @base, Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLING_SELECT);
            useOp = new Saxon.Hej.expr.Operand(this, use, Saxon.Hej.expr.FilterExpression.FILTER_PREDICATE);
            searchOp = new Saxon.Hej.expr.Operand(this, search, Saxon.Hej.expr.OperandRole.ATOMIC_SEQUENCE);
            this.comparer = comparer;
            this.convertUntypedToOther = convertUntypedToOther;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(baseOp, useOp, searchOp);
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getBaseExpression().getItemType();
        }
        public Saxon.Hej.expr.Expression getBaseExpression() {
            return baseOp.getChildExpression();
        }
        public Saxon.Hej.expr.Expression getUseExpression() {
            return useOp.getChildExpression();
        }
        public Saxon.Hej.expr.Expression getSearchExpression() {
            return searchOp.getChildExpression();
        }
        public bool isConvertUntypedToOther() {
            return convertUntypedToOther;
        }
        public Saxon.Hej.expr.sort.AtomicComparer getComparer() {
            return comparer;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            baseOp.typeCheck(visitor, contextInfo);
            Saxon.Hej.type.ItemType type = getBaseExpression().getItemType();
            Saxon.Hej.expr.parser.ContextItemStaticInfo cisi = visitor.getConfiguration().makeContextItemStaticInfo(type, false);
            useOp.typeCheck(visitor, cisi);
            searchOp.typeCheck(visitor, contextInfo);
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            if (!(((getBaseExpression() is Saxon.Hej.expr.VariableReference)))) {
                if (Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(getSearchExpression())) {
                    Saxon.Hej.expr.LetExpression let = new Saxon.Hej.expr.LetExpression();
                    let.setVariableQName(new Saxon.Hej.om.StructuredQName("vv", Saxon.Hej.om.NamespaceUri.SAXON_GENERATED_VARIABLE, ("vv"+computeHashCode())));
                    let.setSequence(getSearchExpression());
                    let.setRequiredType(getSearchExpression().getStaticType());
                    Saxon.Hej.expr.Expression filter = new Saxon.Eej.ee.optim.GeneralEqualityEE(getUseExpression(), Saxon.Hej.expr.parser.Token.EQUALS, new Saxon.Hej.expr.LocalVariableReference(let));
                    filter = filter.typeCheck(visitor, contextItemType);
                    let.setAction(new Saxon.Hej.expr.FilterExpression(getBaseExpression(), filter));
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, let);
                    return let.optimize(visitor, contextItemType);
                } else {
                    Saxon.Hej.expr.Expression filter = new Saxon.Eej.ee.optim.GeneralEqualityEE(getUseExpression(), Saxon.Hej.expr.parser.Token.EQUALS, getSearchExpression());
                    Saxon.Hej.type.ItemType type = getBaseExpression().getItemType();
                    Saxon.Hej.expr.parser.ContextItemStaticInfo cisi = visitor.getConfiguration().makeContextItemStaticInfo(type, false);
                    filter = filter.typeCheck(visitor, cisi);
                    Saxon.Hej.expr.Expression e2 = new Saxon.Hej.expr.FilterExpression(getBaseExpression(), filter);
                    return e2.optimize(visitor, contextItemType);
                }
            }
            return this;
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Eej.ee.optim.IndexedFilterExpression ife = new Saxon.Eej.ee.optim.IndexedFilterExpression(getBaseExpression().copy(rebindings), getUseExpression().copy(rebindings), getSearchExpression().copy(rebindings), comparer, convertUntypedToOther);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, ife);
            return ife;
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        protected internal override int computeSpecialProperties() {
            return getBaseExpression().getSpecialProperties();
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet target = getBaseExpression().addToPathMap(pathMap, pathMapNodeSet);
            getUseExpression().addToPathMap(pathMap, target);
            getSearchExpression().addToPathMap(pathMap, target);
            return target;
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Eej.ee.optim.IndexedFilterExpression)) {
                Saxon.Eej.ee.optim.IndexedFilterExpression f = (Saxon.Eej.ee.optim.IndexedFilterExpression)(other);
                return ((((getBaseExpression().isEqual(f.getBaseExpression()) && getUseExpression().isEqual(f.getUseExpression())) && getSearchExpression().isEqual(f.getSearchExpression())) && comparer.Equals(f.comparer)) && (convertUntypedToOther == f.convertUntypedToOther));
            }
            return false;
        }
        protected internal override int computeHashCode() {
            return ((("IndexedFilterExpression".GetHashCode()+getBaseExpression().GetHashCode())^getUseExpression().GetHashCode())^getSearchExpression().GetHashCode());
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return /*Delegate.iterate()*/makeElaborator().elaborateForPull()(context);
        }
        public override int computeDependencies() {
            int b = (getBaseExpression().getDependencies()|getSearchExpression().getDependencies());
            int f = getUseExpression().getDependencies();
            return (b|((f&(((Saxon.Hej.expr.StaticProperty.DEPENDS_ON_XSLT_CONTEXT|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LOCAL_VARIABLES)|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_USER_FUNCTIONS)))));
        }
        public override string getExpressionName() {
            return "indexedFilter";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("indexedFilter2", this);
            @out.emitAttribute("comp", comparer.save());
            if (convertUntypedToOther) {
                @out.emitAttribute("flags", "u");
            }
            @out.setChildRole("base");
            getBaseExpression().export(@out);
            @out.setChildRole("use");
            getUseExpression().export(@out);
            @out.setChildRole("search");
            getSearchExpression().export(@out);
            @out.endElement();
        }
        public override string toShortString() {
            return (((((getBaseExpression().toShortString()+"[(:indexed:)")+getUseExpression().toShortString())+"=")+getSearchExpression().toShortString())+"]");
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Eej.ee.optim.IndexedFilterExpression.IndexedFilterExpressionElaborator();
        }
        private class IndexedFilterExpressionElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Eej.ee.optim.IndexedFilterExpression expr = (Saxon.Eej.ee.optim.IndexedFilterExpression)(getExpression());
                Saxon.Hej.expr.Expression baseExp = expr.getBaseExpression();
                Saxon.Hej.expr.elab.PullEvaluator basePullEvaluator = baseExp.makeElaborator().elaborateForPull();
                Saxon.Hej.expr.elab.PullEvaluator searchPullEvaluator = expr.getSearchExpression().makeElaborator().elaborateForPull();
                return context => {
                    Saxon.Eej.ee.optim.SearchableValue indexedValue;
                    if ((baseExp is Saxon.Hej.expr.VariableReference)) {
                        Saxon.Hej.om.Sequence @base = ((Saxon.Hej.expr.VariableReference)(baseExp)).evaluateVariable(context);
                        if ((@base is Saxon.Hej.value.EmptySequence)) {
                            return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
                        } else {
                            if ((@base is Saxon.Eej.ee.optim.SearchableValue)) {
                                indexedValue = (Saxon.Eej.ee.optim.SearchableValue)(@base);
                            } else {
                                indexedValue = /*134*/Saxon.Eej.ee.optim.SearchableValue.makeSearchableValue(@base.iterate());
                                Saxon.Hej.expr.Binding binding = ((Saxon.Hej.expr.VariableReference)(baseExp)).getBinding();
                                if ((binding is Saxon.Hej.expr.LocalBinding)) {
                                    context.setLocalVariable(((Saxon.Hej.expr.LocalBinding)(binding)).getLocalSlotNumber(), indexedValue);
                                } else {
                                    if ((binding is Saxon.Hej.expr.instruct.GlobalVariable)) {
                                        context.getController().getBindery(((Saxon.Hej.expr.instruct.GlobalVariable)(binding)).getPackageData()).setGlobalVariable((Saxon.Hej.expr.instruct.GlobalVariable)(binding), indexedValue);
                                    }
                                }
                            }
                        }
                    } else {
                        indexedValue = /*134*/Saxon.Eej.ee.optim.SearchableValue.makeSearchableValue(/*Delegate.iterate()*/basePullEvaluator(context));
                    }
                    if ((indexedValue.getLength() == 0)) {
                        return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
                    }
                    Saxon.Hej.expr.sort.AtomicComparer comparer = expr.comparer.provideContext(context);
                    Saxon.Hej.om.SequenceIterator values = /*Delegate.iterate()*/searchPullEvaluator(context);
                    return indexedValue.findItems(expr.getUseExpression(), values, expr.convertUntypedToOther, false, comparer, context);
                } ;
            }
        
        }
    
    }
}