using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Adjust_2 : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.CalendarValue @in = (Saxon.Hej.value.CalendarValue)(arguments[0].head());
            if ((@in == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            } else {
                Saxon.Hej.value.DayTimeDurationValue tz = (Saxon.Hej.value.DayTimeDurationValue)(arguments[1].head());
                if ((tz == null)) {
                    return @in.removeTimezone();
                } else {
                    return @in.adjustTimezone(tz);
                }
            }
        }
    
    }
}