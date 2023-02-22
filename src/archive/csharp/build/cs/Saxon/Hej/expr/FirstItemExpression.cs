using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public sealed class FirstItemExpression : Saxon.Hej.expr.SingleItemFilter {
        private FirstItemExpression(Saxon.Hej.expr.Expression @base) : base (@base) {
        }
        public static Saxon.Hej.expr.Expression makeFirstItemExpression(Saxon.Hej.expr.Expression @base) {
            if ((@base is Saxon.Hej.expr.FirstItemExpression)) {
                return @base;
            } else {
                return new Saxon.Hej.expr.FirstItemExpression(@base);
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.Expression e2 = new Saxon.Hej.expr.FirstItemExpression(getBaseExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, e2);
            return e2;
        }
        public override Saxon.Hej.pattern.Pattern toPattern(Saxon.Hej.Configuration config) {
            Saxon.Hej.pattern.Pattern basePattern = getBaseExpression().toPattern(config);
            Saxon.Hej.type.ItemType type = basePattern.getItemType();
            if ((type is Saxon.Hej.pattern.NodeTest)) {
                Saxon.Hej.expr.Expression baseExpr = getBaseExpression();
                if ((((baseExpr is Saxon.Hej.expr.AxisExpression) && (((Saxon.Hej.expr.AxisExpression)(baseExpr)).getAxis() == Saxon.Hej.om.AxisInfo.CHILD)) && (basePattern is Saxon.Hej.pattern.NodeTestPattern))) {
                    return new Saxon.Hej.pattern.SimplePositionalPattern((Saxon.Hej.pattern.NodeTest)(type), 1);
                } else {
                    return new Saxon.Hej.pattern.GeneralNodePattern(this, (Saxon.Hej.pattern.NodeTest)(type));
                }
            } else {
                return basePattern;
            }
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.SequenceIterator iter = getBaseExpression().iterate(context);
            Saxon.Hej.om.Item result = iter.next();
            iter.close();
            return result;
        }
        public override string getExpressionName() {
            return "first";
        }
        public override string toShortString() {
            return (getBaseExpression().toShortString()+"[1]");
        }
        public override string getStreamerName() {
            return "FirstItemExpression";
        }
    
    }
}