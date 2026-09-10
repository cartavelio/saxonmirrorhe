using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public class CompareToIntegerConstant : Saxon.Hej.expr.CompareToConstant {
        private readonly long comparand;
        public CompareToIntegerConstant(Saxon.Hej.expr.Expression operand, int @operator, long comparand) : base (operand) {
            this.@operator = @operator;
            this.comparand = comparand;
        }
        public long getComparand() {
            return comparand;
        }
        public override Saxon.Hej.expr.Expression getRhsExpression() {
            return new Saxon.Hej.expr.Literal(new Saxon.Hej.value.Int64Value(comparand));
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.CompareToIntegerConstant c2 = new Saxon.Hej.expr.CompareToIntegerConstant(getLhsExpression().copy(rebindings), @operator, comparand);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, c2);
            return c2;
        }
        public override bool Equals(System.Object other) {
            return ((((other is Saxon.Hej.expr.CompareToIntegerConstant) && ((Saxon.Hej.expr.CompareToIntegerConstant)(other)).getLhsExpression().isEqual(getLhsExpression())) && (((Saxon.Hej.expr.CompareToIntegerConstant)(other)).comparand == comparand)) && (((Saxon.Hej.expr.CompareToIntegerConstant)(other)).@operator == @operator));
        }
        protected internal override int computeHashCode() {
            int h = 0x136b12a0;
            return ((h+getLhsExpression().GetHashCode())^(int)(comparand));
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.value.NumericValue n = (Saxon.Hej.value.NumericValue)(getLhsExpression().evaluateItem(context));
            if (n.isNaN()) {
                return (@operator == Saxon.Hej.expr.parser.Token.FNE);
            }
            int c = n.compareTo(comparand);
            return interpretComparisonResult(c);
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override string getExpressionName() {
            return "compareToInt";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("compareToInt", this);
            destination.emitAttribute("op", Saxon.Hej.expr.parser.Token.tokens[@operator]);
            destination.emitAttribute("val", (comparand+""));
            getLhsExpression().export(destination);
            destination.endElement();
        }
        public override string ToString() {
            return ((((Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getLhsExpression())+" ")+Saxon.Hej.expr.parser.Token.tokens[@operator])+" ")+comparand);
        }
        public override string toShortString() {
            return ((((getLhsExpression().toShortString()+" ")+Saxon.Hej.expr.parser.Token.tokens[@operator])+" ")+comparand);
        }
        public override Saxon.Hej.expr.sort.AtomicComparer getAtomicComparer() {
            return Saxon.Hej.expr.sort.DoubleSortComparer.getInstance();
        }
    
    }
}