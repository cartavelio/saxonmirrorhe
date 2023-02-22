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
            return interpretComparisonResult(@operator, c);
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
        public override Saxon.Hej.lib.StringCollator getStringCollator() {
            return Saxon.Hej.expr.sort.CodepointCollator.getInstance();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.CompareToIntegerConstant.CompareToIntegerConstantElaborator();
        }
        public class CompareToIntegerConstantElaborator : Saxon.Hej.expr.elab.BooleanElaborator {
            public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
                Saxon.Hej.expr.CompareToIntegerConstant expression = (Saxon.Hej.expr.CompareToIntegerConstant)(getExpression());
                Saxon.Hej.expr.Expression arg = expression.getBaseExpression();
                Saxon.Hej.expr.elab.ItemEvaluator argEval = arg.makeElaborator().elaborateForItem();
                int @operator = expression.getComparisonOperator();
                long comparand = expression.getComparand();
                Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
                Saxon.Hej.type.ItemType operandType = arg.getItemType();
                bool isDouble = th.isSubType(operandType, Saxon.Hej.type.BuiltInAtomicType.DOUBLE);
                bool isFloat = th.isSubType(operandType, Saxon.Hej.type.BuiltInAtomicType.FLOAT);
                bool isInteger = th.isSubType(operandType, Saxon.Hej.type.BuiltInAtomicType.INTEGER);
                bool isDecimal = ((!(isInteger)) && th.isSubType(operandType, Saxon.Hej.type.BuiltInAtomicType.DECIMAL));
                if (isInteger) {
                    return context => {
                        Saxon.Hej.value.IntegerValue val = (Saxon.Hej.value.IntegerValue)(/*Delegate.eval()*/argEval(context));
                        return interpretComparisonResult(@operator, val.compareTo(comparand));
                    } ;
                }
                if ((isDouble || isFloat)) {
                    double constant = (double)(comparand);
                    return context => {
                        double val = ((Saxon.Hej.value.NumericValue)(/*Delegate.eval()*/argEval(context))).getDoubleValue();
                        if (System.Double.IsNaN(val)) {
                            return (@operator == Saxon.Hej.expr.parser.Token.FNE);
                        }
                        return interpretComparisonResult(@operator, val.CompareTo(constant));
                    } ;
                }
                if (isDecimal) {
                    Singulink.Numerics.BigDecimal deci = Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(comparand);
                    return context => {
                        Singulink.Numerics.BigDecimal val = ((Saxon.Hej.value.NumericValue)(/*Delegate.eval()*/argEval(context))).getDecimalValue();
                        return interpretComparisonResult(@operator, val.CompareTo(deci));
                    } ;
                }
                return context => {
                    Saxon.Hej.value.NumericValue num = ((Saxon.Hej.value.NumericValue)(/*Delegate.eval()*/argEval(context)));
                    if (num.isNaN()) {
                        return (@operator == Saxon.Hej.expr.parser.Token.FNE);
                    }
                    int c = num.compareTo(comparand);
                    return interpretComparisonResult(@operator, c);
                } ;
            }
        
        }
    
    }
}