using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    #pragma warning disable 659
    public class FunctionLiteral : Saxon.Hej.expr.Literal {
        public FunctionLiteral(Saxon.Hej.om.Function value) : base (value) {
        }
        public override Saxon.Hej.om.Function getGroundedValue() {
            return (Saxon.Hej.om.Function)(base.getGroundedValue());
        }
        public override Saxon.Hej.expr.Expression simplify() {
            if ((getGroundedValue() is Saxon.Hej.functions.AbstractFunction)) {
                ((Saxon.Hej.functions.AbstractFunction)(getGroundedValue())).simplify();
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            if ((getGroundedValue() is Saxon.Hej.functions.AbstractFunction)) {
                ((Saxon.Hej.functions.AbstractFunction)(getGroundedValue())).typeCheck(visitor, contextInfo);
            }
            return this;
        }
        public override Saxon.Hej.type.FunctionItemType getItemType() {
            return getGroundedValue().getFunctionItemType();
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        protected internal override int computeSpecialProperties() {
            return Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED;
        }
        public override bool isVacuousExpression() {
            return false;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.functions.hof.FunctionLiteral fl2 = new Saxon.Hej.functions.hof.FunctionLiteral(getGroundedValue());
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, fl2);
            return fl2;
        }
        public override void setRetainedStaticContext(Saxon.Hej.expr.parser.RetainedStaticContext rsc) {
            base.setRetainedStaticContext(rsc);
        }
        public override bool Equals(System.Object obj) {
            return ((obj is Saxon.Hej.functions.hof.FunctionLiteral) && (((Saxon.Hej.functions.hof.FunctionLiteral)(obj)).getGroundedValue() == getGroundedValue()));
        }
        protected internal override int computeHashCode() {
            return getGroundedValue().GetHashCode();
        }
        public override string getExpressionName() {
            return "namedFunctionRef";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            Saxon.Hej.om.Function f = getGroundedValue();
            if ((f is Saxon.Hej.expr.instruct.UserFunction)) {
                new Saxon.Hej.functions.hof.UserFunctionReference((Saxon.Hej.expr.instruct.UserFunction)(f)).export(@out);
            } else {
                f.export(@out);
            }
        }
    
    }
}