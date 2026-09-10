using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public interface PushableFunction {
        void process(Saxon.Hej.@event.Outputter destination, Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments);
    
    }
}