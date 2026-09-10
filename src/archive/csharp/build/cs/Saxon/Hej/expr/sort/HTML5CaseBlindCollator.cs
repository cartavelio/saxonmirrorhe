using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class HTML5CaseBlindCollator : Saxon.Hej.lib.StringCollator, Saxon.Hej.lib.SubstringMatcher {
        private static readonly Saxon.Hej.expr.sort.HTML5CaseBlindCollator theInstance = new Saxon.Hej.expr.sort.HTML5CaseBlindCollator();
        public static Saxon.Hej.expr.sort.HTML5CaseBlindCollator getInstance() {
            return theInstance;
        }
        public string getCollationURI() {
            return Saxon.Hej.lib.NamespaceConstant.HTML5_CASE_BLIND_COLLATION_URI;
        }
        public int compareStrings(Saxon.Hej.str.UnicodeString a, Saxon.Hej.str.UnicodeString b) {
            return compareCS(a, b);
        }
        private int compareCS(Saxon.Hej.str.UnicodeString a, Saxon.Hej.str.UnicodeString b) {
            long alen = a.length();
            long blen = b.length();
            long i = 0;
            long j = 0;
            while (true) {
                if ((i == alen)) {
                    if ((j == blen)) {
                        return 0;
                    } else {
                        return -1;
                    }
                }
                if ((j == blen)) {
                    return +(1);
                }
                int nexta = a.codePointAt(i++);
                int nextb = b.codePointAt(j++);
                if (((nexta>='a') && (nexta<='z'))) {
                    nexta += ('A'-'a');
                }
                if (((nextb>='a') && (nextb<='z'))) {
                    nextb += ('A'-'a');
                }
                int c = (nexta-nextb);
                if ((c != 0)) {
                    return c;
                }
            }
        }
        public bool comparesEqual(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            return (compareCS(s1, s2) == 0);
        }
        public bool contains(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            return (normalize(s1).indexOf(normalize(s2), 0)>=0);
        }
        public bool endsWith(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            return normalize(s1).hasSubstring(normalize(s2), (s1.length()-s2.length()));
        }
        public bool startsWith(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            return normalize(s1).hasSubstring(normalize(s2), 0);
        }
        public Saxon.Hej.str.UnicodeString substringAfter(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            long i = normalize(s1).indexOf(normalize(s2), 0);
            if ((i<0)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
            return s1.substring((i+s2.length()), s1.length());
        }
        public Saxon.Hej.str.UnicodeString substringBefore(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            long j = normalize(s1).indexOf(normalize(s2), 0);
            if ((j<0)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
            return s1.prefix(j);
        }
        public Saxon.Hej.expr.sort.AtomicMatchKey getCollationKey(Saxon.Hej.str.UnicodeString s) {
            return normalize(s);
        }
        private Saxon.Hej.str.UnicodeString normalize(Saxon.Hej.str.UnicodeString cs) {
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder(cs.length32());
            Saxon.Hej.z.IntIterator iter = cs.codePoints();
            while (iter.hasNext()) {
                int c = iter.next();
                if ((('a'<=c) && (c<='z'))) {
                    sb.append((char)((((c+'A')-'a'))));
                } else {
                    sb.append(c);
                }
            }
            return sb.toUnicodeString();
        }
    
        /* Expanding default method isEqualToEmpty from interface StringCollator */
        public virtual bool isEqualToEmpty(Saxon.Hej.str.UnicodeString s1) {
            return comparesEqual(s1, Saxon.Hej.str.EmptyUnicodeString.getInstance());
        }

    }
}