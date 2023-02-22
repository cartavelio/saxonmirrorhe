using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class EmptyGreatestComparer : Saxon.Hej.expr.sort.AtomicComparer {
        private readonly Saxon.Hej.expr.sort.AtomicComparer baseComparer;
        public EmptyGreatestComparer(Saxon.Hej.expr.sort.AtomicComparer baseComparer)  {
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
                return new Saxon.Hej.expr.sort.EmptyGreatestComparer(newBase);
            } else {
                return this;
            }
        }
        public int compareAtomicValues(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            if ((a == null)) {
                if ((b == null)) {
                    return 0;
                } else {
                    return +(1);
                }
            } else {
                if ((b == null)) {
                    return -1;
                }
            }
            if (a.isNaN()) {
                return (b.isNaN() ? 0 : +(1));
            } else {
                if (b.isNaN()) {
                    return -1;
                }
            }
            return baseComparer.compareAtomicValues(a, b);
        }
        public bool comparesEqual(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            return ((((a == null) && (b == null))) || baseComparer.comparesEqual(a, b));
        }
        public string save() {
            return ("EG|"+baseComparer.save());
        }
    
    }
}