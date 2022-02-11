using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public interface Callable {
        Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments);
    
    }
}