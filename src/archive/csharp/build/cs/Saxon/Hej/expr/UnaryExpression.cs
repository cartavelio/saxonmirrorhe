using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public abstract class UnaryExpression : Saxon.Hej.expr.Expression {
        private readonly Saxon.Hej.expr.Operand operand;
        public UnaryExpression(Saxon.Hej.expr.Expression p0)  {
            operand = new Saxon.Hej.expr.Operand(this, p0, getOperandRole());
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(p0, this);
        }
        public Saxon.Hej.expr.Expression getBaseExpression() {
            return operand.getChildExpression();
        }
        public void setBaseExpression(Saxon.Hej.expr.Expression child) {
            operand.setChildExpression(child);
        }
        public Saxon.Hej.expr.Operand getOperand() {
            return operand;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operand;
        }
        protected internal abstract Saxon.Hej.expr.OperandRole getOperandRole();
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            operand.typeCheck(visitor, contextInfo);
            try {
                if ((getBaseExpression() is Saxon.Hej.expr.Literal)) {
                    Saxon.Hej.expr.Expression e2 = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.om.SequenceTool.toGroundedValue(iterate(visitor.getStaticContext().makeEarlyEvaluationContext())), this);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, e2);
                    return e2;
                }
            } catch (System.Exception err) {
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            operand.optimize(visitor, contextInfo);
            Saxon.Hej.expr.Expression @base = getBaseExpression();
            try {
                if ((@base is Saxon.Hej.expr.Literal)) {
                    return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.om.SequenceTool.toGroundedValue(iterate(visitor.getStaticContext().makeEarlyEvaluationContext())), this);
                }
            } catch (Exception err) when (err is Saxon.Hej.trans.XPathException || err is Saxon.Hej.trans.UncheckedXPathException) {
            }
            return this;
        }
        protected internal override int computeSpecialProperties() {
            return getBaseExpression().getSpecialProperties();
        }
        protected internal override int computeCardinality() {
            return getBaseExpression().getCardinality();
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getBaseExpression().getItemType();
        }
        public override bool Equals(System.Object other) {
            return (((other != null) && this.GetType().Equals(other.GetType())) && this.getBaseExpression().isEqual(((Saxon.Hej.expr.UnaryExpression)(other)).getBaseExpression()));
        }
        protected internal override int computeHashCode() {
            return ((("UnaryExpression "+this.GetType())).GetHashCode()^getBaseExpression().GetHashCode());
        }
        public override string ToString() {
            return (((getExpressionName()+"(")+getBaseExpression())+")");
        }
        public override string toShortString() {
            return (((getExpressionName()+"(")+getBaseExpression().toShortString())+")");
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            string name = getExpressionName();
            if ((name == null)) {
                @out.startElement("unaryOperator", this);
                string op = displayOperator(@out.getConfiguration());
                if ((op != null)) {
                    @out.emitAttribute("op", op);
                }
            } else {
                @out.startElement(name, this);
            }
            emitExtraAttributes(@out);
            getBaseExpression().export(@out);
            @out.endElement();
        }
        protected internal virtual void emitExtraAttributes(Saxon.Hej.trace.ExpressionPresenter @out) {
        }
        protected internal virtual string displayOperator(Saxon.Hej.Configuration config) {
            return null;
        }
    
    }
}