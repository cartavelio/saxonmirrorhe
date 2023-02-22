using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class GeneralComparison20 : Saxon.Hej.expr.GeneralComparison {
        public GeneralComparison20(Saxon.Hej.expr.Expression p0, int op, Saxon.Hej.expr.Expression p1) : base (p0, op, p1) {
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.GeneralComparison20 gc = new Saxon.Hej.expr.GeneralComparison20(getLhsExpression().copy(rebindings), @operator, getRhsExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, gc);
            gc.setRetainedStaticContext(getRetainedStaticContext());
            gc.comparer = comparer;
            gc.singletonOperator = singletonOperator;
            gc.runtimeCheckNeeded = runtimeCheckNeeded;
            gc.comparisonCardinality = comparisonCardinality;
            return gc;
        }
        protected internal override Saxon.Hej.expr.GeneralComparison getInverseComparison() {
            Saxon.Hej.expr.GeneralComparison20 gc = new Saxon.Hej.expr.GeneralComparison20(getRhsExpression(), Saxon.Hej.expr.parser.Token.inverse(@operator), getLhsExpression());
            gc.setRetainedStaticContext(getRetainedStaticContext());
            return gc;
        }
    
    }
}