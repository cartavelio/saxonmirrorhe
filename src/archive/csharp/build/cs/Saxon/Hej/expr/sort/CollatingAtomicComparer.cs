using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class CollatingAtomicComparer : Saxon.Hej.expr.sort.AtomicComparer {
        private readonly Saxon.Hej.lib.StringCollator collator;
        public CollatingAtomicComparer(Saxon.Hej.lib.StringCollator collator)  {
            if ((collator == null)) {
                this.collator = Saxon.Hej.expr.sort.CodepointCollator.getInstance();
            } else {
                this.collator = collator;
            }
        }
        public Saxon.Hej.lib.StringCollator getCollator() {
            return collator;
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
            return collator.compareStrings(a.getUnicodeStringValue(), b.getUnicodeStringValue());
        }
        public bool comparesEqual(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            return (compareAtomicValues(a, b) == 0);
        }
        public string save() {
            return ("CAC|"+getCollator().getCollationURI());
        }
        public override int GetHashCode() {
            return collator.GetHashCode();
        }
        public override bool Equals(System.Object obj) {
            return ((obj is Saxon.Hej.expr.sort.CollatingAtomicComparer) && collator.Equals(((Saxon.Hej.expr.sort.CollatingAtomicComparer)(obj)).collator));
        }
    
    }
}