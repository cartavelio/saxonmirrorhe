using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class ConsumingOperand : Saxon.Hej.expr.UnaryExpression {
        public ConsumingOperand(Saxon.Hej.expr.Expression subExpression) : base (subExpression) {
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return new Saxon.Hej.expr.OperandRole(0, Saxon.Hej.expr.OperandUsage.ABSORPTION);
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getBaseExpression().getItemType();
        }
        public override int getIntrinsicDependencies() {
            return getBaseExpression().getIntrinsicDependencies();
        }
        protected internal override int computeCardinality() {
            return getBaseExpression().getCardinality();
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.ConsumingOperand exp = new Saxon.Hej.expr.ConsumingOperand(getBaseExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override int getImplementationMethod() {
            return (EVALUATE_METHOD|ITERATE_METHOD);
        }
        public Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.XPathContext c) {
            if (c.getStackFrame().holdsDynamicValue()) {
                return c.getStackFrame().popDynamicValue();
            } else {
                return new Saxon.Hej.om.LazySequence(getBaseExpression().iterate(c));
            }
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return evaluate(context).iterate();
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return evaluate(context).head();
        }
        public override string getExpressionName() {
            return "consume";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("consume", this);
            getBaseExpression().export(destination);
            destination.endElement();
        }
        public override string ToString() {
            return (("consume("+getBaseExpression().ToString())+")");
        }
        public override string toShortString() {
            return (("consume("+getBaseExpression().toShortString())+")");
        }
    
    }
}