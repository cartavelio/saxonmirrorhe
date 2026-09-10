using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class TextComparer : Saxon.Hej.expr.sort.AtomicComparer {
        private readonly Saxon.Hej.expr.sort.AtomicComparer baseComparer;
        public TextComparer(Saxon.Hej.expr.sort.AtomicComparer baseComparer)  {
            this.baseComparer = baseComparer;
        }
        public Saxon.Hej.expr.sort.AtomicComparer getBaseComparer() {
            return baseComparer;
        }
        public Saxon.Hej.lib.StringCollator getCollator() {
            return baseComparer.getCollator();
        }
        public Saxon.Hej.expr.sort.AtomicComparer provideContext(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.sort.AtomicComparer newBase = baseComparer.provideContext(context);
            if ((newBase != baseComparer)) {
                return new Saxon.Hej.expr.sort.TextComparer(newBase);
            } else {
                return this;
            }
        }
        public int compareAtomicValues(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            return baseComparer.compareAtomicValues(toStringValue(a), toStringValue(b));
        }
        private Saxon.Hej.value.StringValue toStringValue(Saxon.Hej.value.AtomicValue a) {
            return new Saxon.Hej.value.StringValue(((a == null) ? (Saxon.Hej.str.UnicodeString)(Saxon.Hej.str.EmptyUnicodeString.getInstance()) : a.getUnicodeStringValue()));
        }
        public bool comparesEqual(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            return (compareAtomicValues(a, b) == 0);
        }
        public string save() {
            return ("TEXT|"+baseComparer.save());
        }
    
    }
}