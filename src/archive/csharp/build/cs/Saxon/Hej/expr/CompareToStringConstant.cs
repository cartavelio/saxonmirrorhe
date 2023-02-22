using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public class CompareToStringConstant : Saxon.Hej.expr.CompareToConstant {
        private readonly Saxon.Hej.str.UnicodeString comparand;
        public CompareToStringConstant(Saxon.Hej.expr.Expression operand, int @operator, Saxon.Hej.str.UnicodeString comparand) : base (operand) {
            this.@operator = @operator;
            this.comparand = comparand;
        }
        public Saxon.Hej.str.UnicodeString getComparand() {
            return comparand;
        }
        public override Saxon.Hej.expr.Expression getRhsExpression() {
            return new Saxon.Hej.expr.StringLiteral(comparand);
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.CompareToStringConstant c2 = new Saxon.Hej.expr.CompareToStringConstant(getLhsExpression().copy(rebindings), @operator, comparand);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, c2);
            return c2;
        }
        public override bool Equals(System.Object other) {
            return ((((other is Saxon.Hej.expr.CompareToStringConstant) && ((Saxon.Hej.expr.CompareToStringConstant)(other)).getLhsExpression().isEqual(getLhsExpression())) && ((Saxon.Hej.expr.CompareToStringConstant)(other)).comparand.Equals(comparand)) && (((Saxon.Hej.expr.CompareToStringConstant)(other)).@operator == @operator));
        }
        protected internal override int computeHashCode() {
            int h = 0x484b12a0;
            return ((h+getLhsExpression().GetHashCode())^comparand.GetHashCode());
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.str.UnicodeString s = getLhsExpression().evaluateItem(context).getUnicodeStringValue();
            int c = Saxon.Hej.expr.sort.CodepointCollator.getInstance().compareStrings(s, comparand);
            return interpretComparisonResult(c);
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override string getExpressionName() {
            return "compareToString";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("compareToString", this);
            destination.emitAttribute("op", Saxon.Hej.expr.parser.Token.tokens[@operator]);
            destination.emitAttribute("val", getComparand().ToString());
            getLhsExpression().export(destination);
            destination.endElement();
        }
        public override string ToString() {
            return ((((Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getLhsExpression())+" ")+Saxon.Hej.expr.parser.Token.tokens[@operator])+" ")+comparand.ToString());
        }
        public override string toShortString() {
            return (((((getLhsExpression().toShortString()+" ")+Saxon.Hej.expr.parser.Token.tokens[@operator])+" \"")+comparand)+"\"");
        }
        public override Saxon.Hej.expr.sort.AtomicComparer getAtomicComparer() {
            return Saxon.Hej.expr.sort.CodepointCollatingComparer.getInstance();
        }
    
    }
}