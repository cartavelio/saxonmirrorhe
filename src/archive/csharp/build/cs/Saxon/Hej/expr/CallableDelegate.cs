using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class CallableDelegate : Saxon.Hej.expr.Callable {
        public delegate Saxon.Hej.om.Sequence Lambda (Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments);
        private readonly Saxon.Hej.expr.CallableDelegate.Lambda expression;
        public CallableDelegate(Saxon.Hej.expr.CallableDelegate.Lambda expression)  {
            this.expression = expression;
        }
        public Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return /*Delegate.call()*/expression(context, arguments);
        }
    
    }
}