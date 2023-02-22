using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Adjust_1 : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.CalendarValue @in = (Saxon.Hej.value.CalendarValue)(arguments[0].head());
            if ((@in == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            } else {
                return @in.adjustTimezone(context.getImplicitTimezone());
            }
        }
    
    }
}