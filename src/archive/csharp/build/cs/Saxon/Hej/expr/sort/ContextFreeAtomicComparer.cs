using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class ContextFreeAtomicComparer : Saxon.Hej.expr.sort.AtomicComparer {
        private static readonly Saxon.Hej.expr.sort.ContextFreeAtomicComparer THE_INSTANCE = new Saxon.Hej.expr.sort.ContextFreeAtomicComparer();
        public static Saxon.Hej.expr.sort.ContextFreeAtomicComparer getInstance() {
            return THE_INSTANCE;
        }
        protected internal ContextFreeAtomicComparer()  {
        }
        public Saxon.Hej.lib.StringCollator getCollator() {
            return null;
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
            return ((Saxon.Hej.value.ContextFreeAtomicValue)(a)).getXPathComparable().compareTo(((Saxon.Hej.value.ContextFreeAtomicValue)(b)).getXPathComparable());
        }
        public bool comparesEqual(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            return a.Equals(b);
        }
        public string save() {
            return "CAVC";
        }
    
    }
}