using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public class SubscriptExpression : Saxon.Hej.expr.SingleItemFilter {
        private readonly Saxon.Hej.expr.Operand subscriptOp;
        public SubscriptExpression(Saxon.Hej.expr.Expression @base, Saxon.Hej.expr.Expression subscript) : base (@base) {
            subscriptOp = new Saxon.Hej.expr.Operand(this, subscript, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
        }
        public Saxon.Hej.expr.Expression getSubscript() {
            return subscriptOp.getChildExpression();
        }
        public void setSubscript(Saxon.Hej.expr.Expression subscript) {
            subscriptOp.setChildExpression(subscript);
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().typeCheck(visitor, contextInfo);
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().optimize(visitor, contextInfo);
            if (Saxon.Hej.expr.Literal.isConstantOne(getSubscript())) {
                return Saxon.Hej.expr.FirstItemExpression.makeFirstItemExpression(getBaseExpression());
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.SubscriptExpression exp = new Saxon.Hej.expr.SubscriptExpression(getBaseExpression().copy(rebindings), getSubscript().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(getOperand(), subscriptOp);
        }
        public Saxon.Hej.expr.Expression getSubscriptExpression() {
            return getSubscript();
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public override bool Equals(System.Object other) {
            return (((other is Saxon.Hej.expr.SubscriptExpression) && getBaseExpression().isEqual(((Saxon.Hej.expr.SubscriptExpression)(other)).getBaseExpression())) && getSubscript().isEqual(((Saxon.Hej.expr.SubscriptExpression)(other)).getSubscript()));
        }
        protected internal override int computeHashCode() {
            return (getBaseExpression().GetHashCode()^getSubscript().GetHashCode());
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
        }
        public override string getStreamerName() {
            return "SubscriptExpression";
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.value.NumericValue index = (Saxon.Hej.value.NumericValue)(getSubscript().evaluateItem(context));
            if ((index == null)) {
                return null;
            }
            int intindex = index.asSubscript();
            if ((intindex != -1)) {
                Saxon.Hej.om.Item item;
                Saxon.Hej.om.SequenceIterator iter = getBaseExpression().iterate(context);
                if ((intindex == 1)) {
                    item = iter.next();
                } else {
                    if ((iter is Saxon.Hej.om.MemoSequence.ProgressiveIterator)) {
                        Saxon.Hej.om.MemoSequence mem = ((Saxon.Hej.om.MemoSequence.ProgressiveIterator)(iter)).getMemoSequence();
                        item = mem.itemAt((intindex-1));
                    } else {
                        if (((iter is Saxon.Hej.tree.iter.GroundedIterator) && ((Saxon.Hej.tree.iter.GroundedIterator)(iter)).isActuallyGrounded())) {
                            Saxon.Hej.om.GroundedValue value = Saxon.Hej.om.SequenceTool.toGroundedValue(iter);
                            item = value.itemAt((intindex-1));
                        } else {
                            Saxon.Hej.om.SequenceIterator tail = Saxon.Hej.expr.TailIterator.make(iter, intindex);
                            item = tail.next();
                            tail.close();
                        }
                    }
                }
                return item;
            } else {
                return null;
            }
        }
        public override string getExpressionName() {
            return "subscript";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("subscript", this);
            getBaseExpression().export(destination);
            getSubscript().export(destination);
            destination.endElement();
        }
        public override string ToString() {
            return (((Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getBaseExpression())+"[")+getSubscript())+"]");
        }
        public override string toShortString() {
            return (((Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getBaseExpression())+"[")+getSubscript().toShortString())+"]");
        }
    
    }
}