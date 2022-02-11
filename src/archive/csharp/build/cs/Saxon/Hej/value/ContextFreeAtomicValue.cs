using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public interface ContextFreeAtomicValue {
        Saxon.Hej.expr.sort.XPathComparable getXPathComparable();
    
    }
}