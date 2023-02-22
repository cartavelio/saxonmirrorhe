using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class DescendingComparer : Saxon.Hej.expr.sort.AtomicComparer {
        private readonly Saxon.Hej.expr.sort.AtomicComparer baseComparer;
        public DescendingComparer(Saxon.Hej.expr.sort.AtomicComparer @base)  {
            baseComparer = @base;
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
                return new Saxon.Hej.expr.sort.DescendingComparer(newBase);
            } else {
                return this;
            }
        }
        public int compareAtomicValues(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            return (0-baseComparer.compareAtomicValues(a, b));
        }
        public bool comparesEqual(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            return baseComparer.comparesEqual(a, b);
        }
        public string save() {
            return ("DESC|"+baseComparer.save());
        }
    
    }
}