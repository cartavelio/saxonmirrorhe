using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public abstract class SingleItemFilter : Saxon.Hej.expr.UnaryExpression {
        public SingleItemFilter(Saxon.Hej.expr.Expression @base) : base (@base) {
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().optimize(visitor, contextInfo);
            Saxon.Hej.expr.Expression @base = getBaseExpression();
            if (!(Saxon.Hej.value.Cardinality.allowsMany(@base.getCardinality()))) {
                return @base;
            }
            return base.optimize(visitor, contextInfo);
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
        }
    
    }
}