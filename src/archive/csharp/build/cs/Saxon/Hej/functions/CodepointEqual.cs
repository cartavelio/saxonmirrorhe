using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class CodepointEqual : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue op1 = (Saxon.Hej.value.StringValue)(arguments[0].head());
            Saxon.Hej.value.StringValue op2 = (Saxon.Hej.value.StringValue)(arguments[1].head());
            if (((op1 == null) || (op2 == null))) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            return Saxon.Hej.value.BooleanValue.get(op1.getUnicodeStringValue().Equals(op2.getUnicodeStringValue()));
        }
    
    }
}