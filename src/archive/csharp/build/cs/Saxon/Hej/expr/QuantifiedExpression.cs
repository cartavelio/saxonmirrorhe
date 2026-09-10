using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class QuantifiedExpression : Saxon.Hej.expr.Assignation {
        private int @operator;
        public override string getExpressionName() {
            return Saxon.Hej.expr.parser.Token.tokens[@operator];
        }
        public void setOperator(int @operator) {
            this.@operator = @operator;
        }
        public int getOperator() {
            return @operator;
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getSequenceOp().typeCheck(visitor, contextInfo);
            if (Saxon.Hej.expr.Literal.isEmptySequence(getSequence())) {
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.get((@operator != Saxon.Hej.expr.parser.Token.SOME)), this);
            }
            setSequence(getSequence().unordered(false, false));
            Saxon.Hej.value.SequenceType decl = getRequiredType();
            if ((decl.getCardinality() == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO)) {
                Saxon.Hej.trans.XPathException errNotEmpty = new Saxon.Hej.trans.XPathException("Range variable will never satisfy the type empty-sequence()", "XPTY0004");
                errNotEmpty.setIsTypeError(true);
                errNotEmpty.setLocation(getLocation());
                throw errNotEmpty;
            }
            Saxon.Hej.value.SequenceType sequenceType = Saxon.Hej.value.SequenceType.makeSequenceType(decl.getPrimaryType(), Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE);
            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.VARIABLE, getVariableQName().getDisplayName(), 0);
            setSequence(Saxon.Hej.expr.parser.TypeChecker.strictTypeCheck(getSequence(), sequenceType, role, visitor.getStaticContext()));
            Saxon.Hej.type.ItemType actualItemType = getSequence().getItemType();
            refineTypeInformation(actualItemType, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE, null, getSequence().getSpecialProperties(), this);
            getActionOp().typeCheck(visitor, contextInfo);
            Saxon.Hej.trans.XPathException err = Saxon.Hej.expr.parser.TypeChecker.ebvError(getAction(), visitor.getConfiguration().getTypeHierarchy());
            if ((err != null)) {
                err.setLocation(getLocation());
                throw err;
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            getSequenceOp().optimize(visitor, contextItemType);
            getActionOp().optimize(visitor, contextItemType);
            Saxon.Hej.expr.Expression ebv = Saxon.Hej.functions.BooleanFn.rewriteEffectiveBooleanValue(getAction(), visitor, contextItemType);
            if ((ebv != null)) {
                setAction(ebv);
                adoptChildExpression(ebv);
            }
            if (Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(ebv, true)) {
                if ((getOperator() == Saxon.Hej.expr.parser.Token.SOME)) {
                    return Saxon.Hej.functions.SystemFunction.makeCall("exists", getRetainedStaticContext(), getSequence());
                } else {
                    Saxon.Hej.expr.Expression e2 = new Saxon.Hej.expr.Literal(Saxon.Hej.value.BooleanValue.TRUE);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, e2);
                    return e2;
                }
            } else {
                if (Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(ebv, false)) {
                    if ((getOperator() == Saxon.Hej.expr.parser.Token.SOME)) {
                        Saxon.Hej.expr.Expression e2 = new Saxon.Hej.expr.Literal(Saxon.Hej.value.BooleanValue.FALSE);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, e2);
                        return e2;
                    } else {
                        return Saxon.Hej.functions.SystemFunction.makeCall("empty", getRetainedStaticContext(), getSequence());
                    }
                }
            }
            if ((getSequence() is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.om.GroundedValue seq = ((Saxon.Hej.expr.Literal)(getSequence())).getGroundedValue();
                int len = seq.getLength();
                if ((len == 0)) {
                    Saxon.Hej.expr.Expression e2 = new Saxon.Hej.expr.Literal(Saxon.Hej.value.BooleanValue.get((getOperator() == Saxon.Hej.expr.parser.Token.EVERY)));
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, e2);
                    return e2;
                } else {
                    if ((len == 1)) {
                        if (((getAction() is Saxon.Hej.expr.VariableReference) && (((Saxon.Hej.expr.VariableReference)(getAction())).getBinding() == this))) {
                            return Saxon.Hej.functions.SystemFunction.makeCall("boolean", getRetainedStaticContext(), getSequence());
                        } else {
                            replaceVariable(getSequence());
                            return getAction();
                        }
                    }
                }
            }
            if (visitor.isOptimizeForStreaming()) {
                Saxon.Hej.expr.Expression e3 = visitor.obtainOptimizer().optimizeQuantifiedExpressionForStreaming(this);
                if (((e3 != null) && (e3 != this))) {
                    return e3.optimize(visitor, contextItemType);
                }
            }
            return this;
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public override void checkForUpdatingSubexpressions() {
            getSequence().checkForUpdatingSubexpressions();
            getAction().checkForUpdatingSubexpressions();
        }
        public override bool isUpdatingExpression() {
            return false;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.QuantifiedExpression qe = new Saxon.Hej.expr.QuantifiedExpression();
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, qe);
            qe.setOperator(@operator);
            qe.setVariableQName(variableName);
            qe.setRequiredType(requiredType);
            qe.setSequence(getSequence().copy(rebindings));
            rebindings.put(this, qe);
            Saxon.Hej.expr.Expression newAction = getAction().copy(rebindings);
            qe.setAction(newAction);
            qe.variableName = variableName;
            qe.slotNumber = slotNumber;
            return qe;
        }
        protected internal override int computeSpecialProperties() {
            int p = base.computeSpecialProperties();
            return (p|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
        }
        public override Saxon.Hej.value.BooleanValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.value.BooleanValue.get(effectiveBooleanValue(context));
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.SequenceIterator @base = getSequence().iterate(context);
            bool some = (@operator == Saxon.Hej.expr.parser.Token.SOME);
            int slot = getLocalSlotNumber();
            Saxon.Hej.om.Item it;
            while (((it = @base.next()) != null)) {
                context.setLocalVariable(slot, it);
                if ((some == getAction().effectiveBooleanValue(context))) {
                    @base.close();
                    return some;
                }
            }
            return !(some);
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.BOOLEAN;
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return Saxon.Hej.type.UType.BOOLEAN;
        }
        public override string ToString() {
            return (((((((((@operator == Saxon.Hej.expr.parser.Token.SOME) ? "some" : "every"))+" $")+getVariableEQName())+" in ")+getSequence())+" satisfies ")+Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getAction()));
        }
        public override string toShortString() {
            return ((((((((@operator == Saxon.Hej.expr.parser.Token.SOME) ? "some" : "every"))+" $")+getVariableName())+" in ")+getSequence().toShortString())+" satisfies ...");
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement(Saxon.Hej.expr.parser.Token.tokens[@operator], this);
            @out.emitAttribute("var", getVariableQName());
            @out.emitAttribute("slot", (""+slotNumber));
            getSequence().export(@out);
            getAction().export(@out);
            @out.endElement();
        }
    
    }
}