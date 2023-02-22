using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public interface Negatable {
        bool isNegatable(Saxon.Hej.type.TypeHierarchy th);
        Saxon.Hej.expr.Expression negate();
    
    }
}