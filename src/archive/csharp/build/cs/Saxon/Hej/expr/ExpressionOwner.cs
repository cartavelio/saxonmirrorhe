using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public interface ExpressionOwner {
        Saxon.Hej.expr.Expression getChildExpression();
        void setChildExpression(Saxon.Hej.expr.Expression expr);
    
    }
}