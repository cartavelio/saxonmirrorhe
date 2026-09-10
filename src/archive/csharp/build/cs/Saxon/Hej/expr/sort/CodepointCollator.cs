using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class CodepointCollator : Saxon.Hej.lib.StringCollator, Saxon.Hej.lib.SubstringMatcher {
        private static readonly Saxon.Hej.expr.sort.CodepointCollator theInstance = new Saxon.Hej.expr.sort.CodepointCollator();
        public static Saxon.Hej.expr.sort.CodepointCollator getInstance() {
            return theInstance;
        }
        public string getCollationURI() {
            return Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI;
        }
        public int compareStrings(Saxon.Hej.str.UnicodeString a, Saxon.Hej.str.UnicodeString b) {
            return a.compareTo(b);
        }
        public bool comparesEqual(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            return s1.Equals(s2);
        }
        public bool contains(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            return (s1.indexOf(s2, 0)>=0);
        }
        public bool endsWith(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            if ((s2.length()>s1.length())) {
                return false;
            }
            return s1.hasSubstring(s2, (s1.length()-s2.length()));
        }
        public bool startsWith(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            return s1.hasSubstring(s2, 0);
        }
        public Saxon.Hej.str.UnicodeString substringAfter(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            long i = s1.indexOf(s2, 0);
            if ((i<0)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
            return s1.substring((i+s2.length()));
        }
        public Saxon.Hej.str.UnicodeString substringBefore(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            long j = s1.indexOf(s2, 0);
            if ((j<0)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
            return s1.prefix(j);
        }
        public Saxon.Hej.expr.sort.AtomicMatchKey getCollationKey(Saxon.Hej.str.UnicodeString s) {
            return s;
        }
        public bool isEqualToEmpty(Saxon.Hej.str.UnicodeString s1) {
            return s1.isEmpty();
        }
    
    }
}