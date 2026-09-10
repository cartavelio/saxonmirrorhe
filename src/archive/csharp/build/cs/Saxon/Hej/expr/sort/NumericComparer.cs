using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class NumericComparer : Saxon.Hej.expr.sort.AtomicComparer {
        private static readonly Saxon.Hej.expr.sort.NumericComparer THE_INSTANCE = new Saxon.Hej.expr.sort.NumericComparer();
        protected internal Saxon.Hej.type.StringToDouble converter = Saxon.Hej.type.StringToDouble.getInstance();
        public static Saxon.Hej.expr.sort.NumericComparer getInstance() {
            return THE_INSTANCE;
        }
        protected internal NumericComparer()  {
        }
        public Saxon.Hej.lib.StringCollator getCollator() {
            return null;
        }
        public Saxon.Hej.expr.sort.AtomicComparer provideContext(Saxon.Hej.expr.XPathContext context) {
            return this;
        }
        public int compareAtomicValues(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            double d1, d2;
            if ((a is Saxon.Hej.value.NumericValue)) {
                d1 = ((Saxon.Hej.value.NumericValue)(a)).getDoubleValue();
            } else {
                if ((a == null)) {
                    d1 = System.Double.NaN;
                } else {
                    try {
                        d1 = converter.stringToNumber(a.getUnicodeStringValue());
                    } catch (Saxon.Ejava.lang.NumberFormatException err) {
                        d1 = System.Double.NaN;
                    }
                }
            }
            if ((b is Saxon.Hej.value.NumericValue)) {
                d2 = ((Saxon.Hej.value.NumericValue)(b)).getDoubleValue();
            } else {
                if ((b == null)) {
                    d2 = System.Double.NaN;
                } else {
                    try {
                        d2 = converter.stringToNumber(b.getUnicodeStringValue());
                    } catch (Saxon.Ejava.lang.NumberFormatException err) {
                        d2 = System.Double.NaN;
                    }
                }
            }
            if (System.Double.IsNaN(d1)) {
                if (System.Double.IsNaN(d2)) {
                    return 0;
                } else {
                    return -1;
                }
            }
            if (System.Double.IsNaN(d2)) {
                return +(1);
            }
            if ((d1<d2)) {
                return -1;
            }
            if ((d1>d2)) {
                return +(1);
            }
            return 0;
        }
        public bool comparesEqual(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            return (compareAtomicValues(a, b) == 0);
        }
        public virtual string save() {
            return "NC";
        }
    
    }
}