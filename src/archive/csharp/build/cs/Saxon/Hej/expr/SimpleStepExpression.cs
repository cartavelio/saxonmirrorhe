using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public sealed class SimpleStepExpression : Saxon.Hej.expr.SlashExpression {
        public SimpleStepExpression(Saxon.Hej.expr.Expression start, Saxon.Hej.expr.Expression step) : base (start, step) {
            if (!(((step is Saxon.Hej.expr.AxisExpression)))) {
                throw new Saxon.Ejava.lang.IllegalArgumentException();
            }
        }
        private static readonly Saxon.Hej.expr.OperandRole STEP_ROLE = new Saxon.Hej.expr.OperandRole((Saxon.Hej.expr.OperandRole.USES_NEW_FOCUS|Saxon.Hej.expr.OperandRole.HIGHER_ORDER), Saxon.Hej.expr.OperandUsage.TRANSMISSION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole(int arg) {
            return ((arg == 0) ? Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLING_SELECT : STEP_ROLE);
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getLhs().typeCheck(visitor, contextInfo);
            Saxon.Hej.type.ItemType selectType = getStart().getItemType();
            if ((selectType == Saxon.Hej.type.ErrorType.getInstance())) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = visitor.getConfiguration().makeContextItemStaticInfo(selectType, false);
            cit.setContextSettingExpression(getStart());
            getRhs().typeCheck(visitor, cit);
            if (!(((getStep() is Saxon.Hej.expr.AxisExpression)))) {
                if (Saxon.Hej.expr.Literal.isEmptySequence(getStep())) {
                    return getStep();
                }
                Saxon.Hej.expr.SlashExpression se = new Saxon.Hej.expr.SlashExpression(getStart(), getStep());
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, se);
                return se;
            }
            if (((getStart() is Saxon.Hej.expr.ContextItemExpression) && Saxon.Hej.om.AxisInfo.isForwards[((Saxon.Hej.expr.AxisExpression)(getStep())).getAxis()])) {
                return getStep();
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            return this;
        }
        public override Saxon.Hej.expr.SimpleStepExpression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.SimpleStepExpression exp = new Saxon.Hej.expr.SimpleStepExpression(getStart().copy(rebindings), getStep().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.NodeInfo origin = null;
            try {
                origin = (Saxon.Hej.om.NodeInfo)(getStart().evaluateItem(context));
            } catch (Saxon.Hej.trans.XPathException e) {
                if (("XPDY0002".Equals(e.getErrorCodeLocalPart()) && !(e.hasBeenReported()))) {
                    throw new Saxon.Hej.trans.XPathException((("The context item for axis step "+toShortString())+" is absent"), "XPDY0002", getLocation());
                } else {
                    throw;
                }
            }
            if ((origin == null)) {
                return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
            }
            return ((Saxon.Hej.expr.AxisExpression)(getStep())).iterate(origin);
        }
        public override string getExpressionName() {
            return "simpleStep";
        }
    
    }
}