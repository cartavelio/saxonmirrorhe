using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public sealed class PatternMatchExpression : Saxon.Hej.expr.Expression {
        private readonly Saxon.Hej.expr.Operand selectOp;
        private readonly Saxon.Hej.expr.Operand patternOp;
        public PatternMatchExpression(Saxon.Hej.expr.Expression select, Saxon.Hej.pattern.Pattern pattern)  {
            selectOp = new Saxon.Hej.expr.Operand(this, select, new Saxon.Hej.expr.OperandRole(0, Saxon.Hej.expr.OperandUsage.NAVIGATION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE));
            patternOp = new Saxon.Hej.expr.Operand(this, pattern, Saxon.Hej.expr.OperandRole.INSPECT);
        }
        public Saxon.Hej.expr.Expression getSelectExpression() {
            return selectOp.getChildExpression();
        }
        public Saxon.Hej.pattern.Pattern getPattern() {
            return (Saxon.Hej.pattern.Pattern)(patternOp.getChildExpression());
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(selectOp, patternOp);
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext c) {
            Saxon.Hej.pattern.Pattern pattern = getPattern();
            Saxon.Hej.om.SequenceIterator iter = getSelectExpression().iterate(c);
            Saxon.Hej.om.Item it;
            while (((it = iter.next()) != null)) {
                if (!(pattern.matches(it, c))) {
                    return false;
                }
            }
            return true;
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.value.BooleanValue.get(effectiveBooleanValue(context));
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.BOOLEAN;
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.PatternMatchExpression exp = new Saxon.Hej.expr.PatternMatchExpression(selectOp.getChildExpression(), (Saxon.Hej.pattern.Pattern)(patternOp.getChildExpression()));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public override bool Equals(System.Object other) {
            return (((other is Saxon.Hej.expr.PatternMatchExpression) && ((Saxon.Hej.expr.PatternMatchExpression)(other)).getSelectExpression().Equals(getSelectExpression())) && ((Saxon.Hej.expr.PatternMatchExpression)(other)).getPattern().Equals(getSelectExpression()));
        }
        protected internal override int computeHashCode() {
            return ((getSelectExpression().computeHashCode()<<5)^getPattern().computeHashCode());
        }
        public override string getExpressionName() {
            return "match";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("patMatch", this);
            getSelectExpression().export(destination);
            getPattern().export(destination);
            destination.endElement();
        }
        public override string ToString() {
            return (((("patMatch("+getSelectExpression().ToString())+",")+getPattern().ToString())+")");
        }
        public override string toShortString() {
            return (((("patMatch("+getSelectExpression().toShortString())+",")+getPattern().toShortString())+")");
        }
    
    }
}