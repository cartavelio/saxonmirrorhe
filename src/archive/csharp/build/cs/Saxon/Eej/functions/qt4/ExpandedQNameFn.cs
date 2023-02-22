using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class ExpandedQNameFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.QNameValue s = (Saxon.Hej.value.QNameValue)(arguments[0].head());
            if ((s == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            return new Saxon.Hej.value.StringValue(s.getStructuredQName().getEQName());
        }
    
    }
}