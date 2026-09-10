using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class EagerLetExpression : Saxon.Hej.expr.LetExpression {
        public EagerLetExpression()  {
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.Expression e = base.optimize(visitor, contextItemType);
            if ((e == this)) {
                setEvaluator(getSequence().makeElaborator().eagerly());
            }
            return e;
        }
    
    }
}