using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trace {
    public interface TraceableComponent : Saxon.Hej.trace.Traceable {
        Saxon.Hej.expr.Expression getBody();
        void setBody(Saxon.Hej.expr.Expression expression);
        string getTracingTag();
    
    }
}