using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class EqualityComparer : Saxon.Hej.expr.sort.AtomicComparer {
        public static Saxon.Hej.expr.sort.EqualityComparer THE_INSTANCE = new Saxon.Hej.expr.sort.EqualityComparer();
        public static Saxon.Hej.expr.sort.EqualityComparer getInstance() {
            return THE_INSTANCE;
        }
        private EqualityComparer()  {
        }
        public Saxon.Hej.lib.StringCollator getCollator() {
            return null;
        }
        public Saxon.Hej.expr.sort.AtomicComparer provideContext(Saxon.Hej.expr.XPathContext context) {
            return this;
        }
        public int compareAtomicValues(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            throw new System.InvalidCastException("Values are not comparable");
        }
        public bool comparesEqual(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            return a.Equals(b);
        }
        public string save() {
            return "EQC";
        }
    
    }
}