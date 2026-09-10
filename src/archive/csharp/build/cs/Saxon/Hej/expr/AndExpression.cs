using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class AndExpression : Saxon.Hej.expr.BooleanExpression {
        public AndExpression(Saxon.Hej.expr.Expression p1, Saxon.Hej.expr.Expression p2) : base (p1, Saxon.Hej.expr.parser.Token.AND, p2) {
        }
        protected internal override Saxon.Hej.expr.Expression preEvaluate() {
            if ((Saxon.Hej.expr.Literal.isConstantBoolean(getLhsExpression(), false) || Saxon.Hej.expr.Literal.isConstantBoolean(getRhsExpression(), false))) {
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, this);
            } else {
                if (Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(getLhsExpression(), true)) {
                    return forceToBoolean(getRhsExpression());
                } else {
                    if (Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(getRhsExpression(), true)) {
                        return forceToBoolean(getLhsExpression());
                    } else {
                        return this;
                    }
                }
            }
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.Expression t = base.optimize(visitor, contextInfo);
            if ((t != this)) {
                return t;
            }
            Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
            if ((((getRhsExpression() is Saxon.Hej.expr.UserFunctionCall) && th.isSubType(getRhsExpression().getItemType(), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN)) && !(Saxon.Hej.expr.parser.ExpressionTool.isLoopingSubexpression(this, null)))) {
                Saxon.Hej.expr.Expression cond = Saxon.Hej.expr.instruct.Choose.makeConditional(getLhsExpression(), getRhsExpression(), Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, this));
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, cond);
                return cond;
            }
            return this;
        }
        public override double getCost() {
            return (getLhsExpression().getCost()+(getRhsExpression().getCost()/2));
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.AndExpression a2 = new Saxon.Hej.expr.AndExpression(getLhsExpression().copy(rebindings), getRhsExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, a2);
            return a2;
        }
        public override Saxon.Hej.expr.Expression negate() {
            Saxon.Hej.expr.Expression not0 = Saxon.Hej.functions.SystemFunction.makeCall("not", getRetainedStaticContext(), getLhsExpression());
            Saxon.Hej.expr.Expression not1 = Saxon.Hej.functions.SystemFunction.makeCall("not", getRetainedStaticContext(), getRhsExpression());
            return new Saxon.Hej.expr.OrExpression(not0, not1);
        }
        protected internal override string tag() {
            return "and";
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext c) {
            return (getLhsExpression().effectiveBooleanValue(c) && getRhsExpression().effectiveBooleanValue(c));
        }
        public static Saxon.Hej.expr.Expression distribute(System.Collections.Generic.ICollection<Saxon.Hej.expr.Expression> exprs) {
            Saxon.Hej.expr.Expression result = null;
            if ((exprs != null)) {
                bool first = true;
                foreach (Saxon.Hej.expr.Expression e in exprs) {
                    if (first) {
                        first = false;
                        result = e;
                    } else {
                        result = new Saxon.Hej.expr.AndExpression(result, e);
                    }
                }
            }
            return result;
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.AndExpression.AndElaborator();
        }
        public class AndElaborator : Saxon.Hej.expr.elab.BooleanElaborator {
            public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
                Saxon.Hej.expr.AndExpression expr = (Saxon.Hej.expr.AndExpression)(getExpression());
                Saxon.Hej.expr.elab.BooleanEvaluator eval0 = expr.getLhsExpression().makeElaborator().elaborateForBoolean();
                Saxon.Hej.expr.elab.BooleanEvaluator eval1 = expr.getRhsExpression().makeElaborator().elaborateForBoolean();
                return context => {
                    Saxon.Hej.trans.XPathException saved = null;
                    try {
                        bool b0 = /*Delegate.eval()*/eval0(context);
                        if (!(b0)) {
                            return false;
                        }
                    } catch (Saxon.Hej.trans.UncheckedXPathException err) {
                        saved = err.getXPathException();
                    } catch (Saxon.Hej.trans.XPathException err) {
                        saved = err;
                    }
                    bool b1 = /*Delegate.eval()*/eval1(context);
                    if (!(b1)) {
                        return false;
                    }
                    if ((saved != null)) {
                        throw saved;
                    }
                    return true;
                } ;
            }
        
        }
    
    }
}