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
            return ((Saxon.Hej.expr.sort.XPathComparable)(a)).compareTo((Saxon.Hej.expr.sort.XPathComparable)(b));
        }
        public bool comparesEqual(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            return a.Equals(b);
        }
        public string save() {
            return "CAVC";
        }
    
    }
}