using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public interface ContextSwitchingExpression : Saxon.Hej.expr.ContextOriginator {
        Saxon.Hej.expr.Expression getSelectExpression();
        Saxon.Hej.expr.Expression getActionExpression();
    
    }
}