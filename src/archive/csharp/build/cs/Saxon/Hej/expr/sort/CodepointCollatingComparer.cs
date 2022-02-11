using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class CodepointCollatingComparer : Saxon.Hej.expr.sort.AtomicComparer {
        private static readonly Saxon.Hej.expr.sort.CodepointCollator collator = Saxon.Hej.expr.sort.CodepointCollator.getInstance();
        private static readonly Saxon.Hej.expr.sort.CodepointCollatingComparer THE_INSTANCE = new Saxon.Hej.expr.sort.CodepointCollatingComparer();
        public static Saxon.Hej.expr.sort.CodepointCollatingComparer getInstance() {
            return THE_INSTANCE;
        }
        private CodepointCollatingComparer()  {
        }
        public Saxon.Hej.lib.StringCollator getCollator() {
            return collator;
        }
        public Saxon.Hej.expr.sort.AtomicComparer provideContext(Saxon.Hej.expr.XPathContext context) {
            return this;
        }
        public int compareAtomicValues(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            if ((a == null)) {
                return (((b == null) ? 0 : -1));
            } else {
                if ((b == null)) {
                    return +(1);
                }
            }
            Saxon.Hej.value.StringValue @as = (Saxon.Hej.value.StringValue)(a);
            Saxon.Hej.value.StringValue bs = (Saxon.Hej.value.StringValue)(b);
            return collator.compareStrings(@as.getUnicodeStringValue(), bs.getUnicodeStringValue());
        }
        public bool comparesEqual(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            return ((Saxon.Hej.value.StringValue)(a)).Equals((Saxon.Hej.value.StringValue)(b));
        }
        public string save() {
            return "CCC";
        }
    
    }
}