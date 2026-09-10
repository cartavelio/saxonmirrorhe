using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class CalendarValueComparer : Saxon.Hej.expr.sort.AtomicComparer {
        private readonly Saxon.Hej.expr.XPathContext context;
        public CalendarValueComparer(Saxon.Hej.expr.XPathContext context)  {
            this.context = context;
        }
        public Saxon.Hej.lib.StringCollator getCollator() {
            return null;
        }
        public Saxon.Hej.expr.sort.AtomicComparer provideContext(Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.sort.CalendarValueComparer(context);
        }
        public int compareAtomicValues(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            if ((a == null)) {
                return ((b == null) ? 0 : -1);
            } else {
                if ((b == null)) {
                    return +(1);
                }
            }
            return ((Saxon.Hej.value.CalendarValue)(a)).compareTo((Saxon.Hej.value.CalendarValue)(b), context.getImplicitTimezone());
        }
        public bool comparesEqual(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            return (compareAtomicValues(a, b) == 0);
        }
        public string save() {
            return "CalVC";
        }
    
    }
}