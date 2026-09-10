using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public interface CodeInjector {
        Saxon.Hej.expr.Expression inject(Saxon.Hej.expr.Expression exp);
        void process(Saxon.Hej.trace.TraceableComponent component);
        Saxon.Hej.expr.flwor.Clause injectClause(Saxon.Hej.expr.flwor.FLWORExpression expression, Saxon.Hej.expr.flwor.Clause clause);
    
    }
}