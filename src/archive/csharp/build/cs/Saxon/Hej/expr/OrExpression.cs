using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class OrExpression : Saxon.Hej.expr.BooleanExpression {
        public OrExpression(Saxon.Hej.expr.Expression p1, Saxon.Hej.expr.Expression p2) : base (p1, Saxon.Hej.expr.parser.Token.OR, p2) {
        }
        protected internal override Saxon.Hej.expr.Expression preEvaluate() {
            if ((Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(getLhsExpression(), true) || Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(getRhsExpression(), true))) {
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE, this);
            } else {
                if (Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(getLhsExpression(), false)) {
                    return forceToBoolean(getRhsExpression());
                } else {
                    if (Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(getRhsExpression(), false)) {
                        return forceToBoolean(getLhsExpression());
                    } else {
                        return this;
                    }
                }
            }
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.Expression e = base.optimize(visitor, contextItemType);
            if ((e != this)) {
                return e;
            }
            if (!(((getParentExpression() is Saxon.Hej.expr.OrExpression)))) {
                Saxon.Hej.expr.Expression e2 = visitor.obtainOptimizer().tryGeneralComparison(visitor, contextItemType, this);
                if (((e2 != null) && (e2 != this))) {
                    return e2;
                }
            }
            return this;
        }
        public override double getCost() {
            return (getLhsExpression().getCost()+(getRhsExpression().getCost()/2));
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.OrExpression exp = new Saxon.Hej.expr.OrExpression(getLhsExpression().copy(rebindings), getRhsExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override Saxon.Hej.expr.Expression negate() {
            Saxon.Hej.expr.Expression not0 = Saxon.Hej.functions.SystemFunction.makeCall("not", getRetainedStaticContext(), getLhsExpression());
            Saxon.Hej.expr.Expression not1 = Saxon.Hej.functions.SystemFunction.makeCall("not", getRetainedStaticContext(), getRhsExpression());
            Saxon.Hej.expr.AndExpression result = new Saxon.Hej.expr.AndExpression(not0, not1);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, result);
            return result;
        }
        protected internal override string tag() {
            return "or";
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext c) {
            return (getLhsExpression().effectiveBooleanValue(c) || getRhsExpression().effectiveBooleanValue(c));
        }
    
    }
}