using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class TimestampFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.value.DateTimeValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.value.DateTimeValue.now();
        }
    
    }
}