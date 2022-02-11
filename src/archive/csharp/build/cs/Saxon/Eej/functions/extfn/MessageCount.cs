using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class MessageCount : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.value.IntegerValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.Controller controller = context.getController();
            if (!(((controller is Saxon.Hej.trans.XsltController)))) {
                return Saxon.Hej.value.Int64Value.ZERO;
            }
            System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,int> counters = ((Saxon.Hej.trans.XsltController)(controller)).getMessageCounters();
            Saxon.Hej.value.QNameValue arg = (Saxon.Hej.value.QNameValue)(arguments[0].head());
            if ((arg == null)) {
                long total = 0;
                foreach (int n in counters.Values) {
                    total += n;
                }
                return new Saxon.Hej.value.Int64Value(total);
            } else {
                int result = Saxon.Impl.Helpers.MapUtils.GetOrDefault(counters, arg.getStructuredQName(), 0);
                return new Saxon.Hej.value.Int64Value(result);
            }
        }
    
    }
}