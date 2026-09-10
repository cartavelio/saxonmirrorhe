using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public interface SortKeyEvaluator {
        Saxon.Hej.value.AtomicValue evaluateSortKey(int n, Saxon.Hej.expr.XPathContext context);
    
    }
}