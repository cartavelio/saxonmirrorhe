using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public abstract class TuplePull {
        public abstract bool nextTuple(Saxon.Hej.expr.XPathContext context);
        public virtual void close() {
        }
    
    }
}