using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class CollationMatchKey : Saxon.Hej.expr.sort.AtomicMatchKey, Saxon.Ejava.lang.Comparable<Saxon.Hej.expr.sort.CollationMatchKey> {
        private readonly Saxon.Ejava.text.CollationKey key;
        public CollationMatchKey(Saxon.Ejava.text.CollationKey key)  {
            this.key = key;
        }
        public int compareTo(Saxon.Hej.expr.sort.CollationMatchKey o) {
            return key.compareTo(o.key);
        }
        public override int GetHashCode() {
            return key.GetHashCode();
        }
        public override bool Equals(System.Object o) {
            return ((o is Saxon.Hej.expr.sort.CollationMatchKey) && key.Equals(((Saxon.Hej.expr.sort.CollationMatchKey)(o)).key));
        }
        public Saxon.Hej.value.AtomicValue asAtomic() {
            return new Saxon.Hej.value.Base64BinaryValue(key.toByteArray());
        }
    
    }
}