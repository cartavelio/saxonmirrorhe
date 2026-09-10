using System;
using System.Threading;
using Saxon.Hej.value;

/*
 * Comparer implementing the rules for ordering of properties in canonical JSON: strings
 * are sorted based on UTF-16 code units
 */

namespace Saxon.Impl.Helpers {
    public class CanonicalJsonKeyComparer : System.Collections.Generic.Comparer<Saxon.Hej.value.AtomicValue> {
        
        public override int Compare(AtomicValue a1, AtomicValue a2) {
            string s1 = a1.getStringValue();
            string s2 = a2.getStringValue();
            return StringComparer.Ordinal.Compare(s1, s2);
        }
    }
}
