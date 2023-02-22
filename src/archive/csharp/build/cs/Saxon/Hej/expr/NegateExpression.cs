using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class NegateExpression : Saxon.Hej.expr.UnaryExpression {
        private bool backwardsCompatible;
        public NegateExpression(Saxon.Hej.expr.Expression @base) : base (@base) {
        }
        public void setBackwardsCompatible(bool compatible) {
            backwardsCompatible = compatible;
        }
        public bool isBackwardsCompatible() {
            return backwardsCompatible;
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().typeCheck(visitor, contextInfo);
            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.UNARY_EXPR, "-", 0);
            Saxon.Hej.expr.Expression operand = visitor.getConfiguration().getTypeChecker(backwardsCompatible).staticTypeCheck(getBaseExpression(), Saxon.Hej.value.SequenceType.OPTIONAL_NUMERIC, role, visitor);
            setBaseExpression(operand);
            if ((operand is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.om.GroundedValue v = ((Saxon.Hej.expr.Literal)(operand)).getGroundedValue();
                if ((v is Saxon.Hej.value.NumericValue)) {
                    return Saxon.Hej.expr.Literal.makeLiteral(((Saxon.Hej.value.NumericValue)(v)).negate(), this);
                }
            }
            return this;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getBaseExpression().getItemType().getPrimitiveItemType();
        }
        protected internal override int computeCardinality() {
            return (getBaseExpression().getCardinality()&~(Saxon.Hej.expr.StaticProperty.ALLOWS_MANY));
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public override Saxon.Hej.value.NumericValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.value.NumericValue v1 = (Saxon.Hej.value.NumericValue)(getBaseExpression().evaluateItem(context));
            if ((v1 == null)) {
                return (backwardsCompatible ? Saxon.Hej.value.DoubleValue.NaN : null);
            }
            return v1.negate();
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.NegateExpression exp = new Saxon.Hej.expr.NegateExpression(getBaseExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        protected internal override string displayOperator(Saxon.Hej.Configuration config) {
            return "-";
        }
        public override string getExpressionName() {
            return "minus";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("minus", this);
            if (backwardsCompatible) {
                @out.emitAttribute("vn", "1");
            }
            getBaseExpression().export(@out);
            @out.endElement();
        }
    
    }
}