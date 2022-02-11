using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public interface AtomicComparer {
        Saxon.Hej.lib.StringCollator getCollator();
        Saxon.Hej.expr.sort.AtomicComparer provideContext(Saxon.Hej.expr.XPathContext context);
        int compareAtomicValues(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b);
        bool comparesEqual(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b);
        string save();
    
    }
}