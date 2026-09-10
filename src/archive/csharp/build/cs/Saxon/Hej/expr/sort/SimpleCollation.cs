using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class SimpleCollation : Saxon.Hej.lib.StringCollator {
        private System.Collections.Generic.Comparer<string> comparator;
        private readonly string uri;
        private static readonly Saxon.Hej.Platform platform = Saxon.Hej.Version.platform;
        public SimpleCollation(string uri, System.Collections.Generic.Comparer<string> comparator)  {
            this.uri = uri;
            this.comparator = comparator;
        }
        public string getCollationURI() {
            return uri;
        }
        public int compareStrings(Saxon.Hej.str.UnicodeString o1, Saxon.Hej.str.UnicodeString o2) {
            return comparator.Compare(o1.ToString(), o2.ToString());
        }
        public bool comparesEqual(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            return (comparator.Compare(s1.ToString(), s2.ToString()) == 0);
        }
        public System.Collections.Generic.Comparer<string> getComparator() {
            return comparator;
        }
        public void setComparator(System.Collections.Generic.Comparer<string> comparator) {
            this.comparator = comparator;
        }
        public Saxon.Hej.expr.sort.AtomicMatchKey getCollationKey(Saxon.Hej.str.UnicodeString s) {
            return platform.getCollationKey(this, s.ToString());
        }
        public Saxon.Hej.lib.SubstringMatcher getSubstringMatcher() {
            if ((comparator is Saxon.Hej.lib.SubstringMatcher)) {
                return (Saxon.Hej.lib.SubstringMatcher)(comparator);
            }
            return null;
        }
    
    }
}