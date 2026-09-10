using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public interface PatternWithPredicate {
        Saxon.Hej.expr.Expression getPredicate();
    
    }
}