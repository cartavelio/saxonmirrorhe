using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public abstract class ContextAccessorFunction : Saxon.Hej.functions.SystemFunction {
        public abstract Saxon.Hej.om.Function bindContext(Saxon.Hej.expr.XPathContext context);
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return bindContext(context).call(context, arguments);
        }
    
    }
}