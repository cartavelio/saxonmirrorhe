using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public class TailExpression : Saxon.Hej.expr.UnaryExpression {
        internal int start;
        public TailExpression(Saxon.Hej.expr.Expression @base, int start) : base (@base) {
            this.start = start;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().optimize(visitor, contextInfo);
            if ((getBaseExpression() is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.om.GroundedValue value = Saxon.Hej.om.SequenceTool.toGroundedValue(iterate(visitor.getStaticContext().makeEarlyEvaluationContext()));
                return Saxon.Hej.expr.Literal.makeLiteral(value, this);
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.TailExpression exp = new Saxon.Hej.expr.TailExpression(getBaseExpression().copy(rebindings), start);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getBaseExpression().getItemType();
        }
        protected internal override int computeCardinality() {
            return (getBaseExpression().getCardinality()|Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO);
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION;
        }
        public int getStart() {
            return start;
        }
        public override bool Equals(System.Object other) {
            return (((other is Saxon.Hej.expr.TailExpression) && getBaseExpression().isEqual(((Saxon.Hej.expr.TailExpression)(other)).getBaseExpression())) && (start == ((Saxon.Hej.expr.TailExpression)(other)).start));
        }
        protected internal override int computeHashCode() {
            return (base.computeHashCode()^start);
        }
        public override string getStreamerName() {
            return "TailExpression";
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.SequenceIterator baseIter = getBaseExpression().iterate(context);
            return Saxon.Hej.expr.TailIterator.make(baseIter, start);
        }
        public override string getExpressionName() {
            return "tail";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("tail", this);
            destination.emitAttribute("start", (start+""));
            getBaseExpression().export(destination);
            destination.endElement();
        }
        public override string ToString() {
            if ((start == 2)) {
                return (("tail("+getBaseExpression())+")");
            } else {
                return (((Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getBaseExpression())+"[position() ge ")+start)+"]");
            }
        }
        public override string toShortString() {
            if ((start == 2)) {
                return (("tail("+getBaseExpression().toShortString())+")");
            } else {
                return (((getBaseExpression().toShortString()+"[position() ge ")+start)+"]");
            }
        }
    
    }
}