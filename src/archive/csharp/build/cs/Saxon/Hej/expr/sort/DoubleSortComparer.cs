using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class DoubleSortComparer : Saxon.Hej.expr.sort.AtomicComparer {
        private static readonly Saxon.Hej.expr.sort.DoubleSortComparer THE_INSTANCE = new Saxon.Hej.expr.sort.DoubleSortComparer();
        public static Saxon.Hej.expr.sort.DoubleSortComparer getInstance() {
            return THE_INSTANCE;
        }
        private DoubleSortComparer()  {
        }
        public Saxon.Hej.lib.StringCollator getCollator() {
            return null;
        }
        public Saxon.Hej.expr.sort.AtomicComparer provideContext(Saxon.Hej.expr.XPathContext context) {
            return this;
        }
        public int compareAtomicValues(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            if ((a == null)) {
                if ((b == null)) {
                    return 0;
                } else {
                    return -1;
                }
            } else {
                if ((b == null)) {
                    return +(1);
                }
            }
            Saxon.Hej.value.NumericValue an = (Saxon.Hej.value.NumericValue)(a);
            Saxon.Hej.value.NumericValue bn = (Saxon.Hej.value.NumericValue)(b);
            if (an.isNaN()) {
                return (bn.isNaN() ? 0 : -1);
            } else {
                if (bn.isNaN()) {
                    return +(1);
                }
            }
            return an.compareTo(bn);
        }
        public bool comparesEqual(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            return (compareAtomicValues(a, b) == 0);
        }
        public string save() {
            return "DblSC";
        }
    
    }
}