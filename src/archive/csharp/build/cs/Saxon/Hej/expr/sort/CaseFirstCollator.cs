using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class CaseFirstCollator : Saxon.Hej.lib.StringCollator {
        private readonly Saxon.Hej.lib.StringCollator baseCollator;
        private readonly bool upperFirst;
        private readonly string uri;
        public CaseFirstCollator(Saxon.Hej.lib.StringCollator @base, bool upperFirst, string collationURI)  {
            this.baseCollator = @base;
            this.upperFirst = upperFirst;
            this.uri = collationURI;
        }
        public static Saxon.Hej.lib.StringCollator makeCaseOrderedCollator(string uri, Saxon.Hej.lib.StringCollator stringCollator, string caseOrder) {
            switch (caseOrder) {
              case "lower-first":
                stringCollator = new Saxon.Hej.expr.sort.CaseFirstCollator(stringCollator, false, uri);
                break ;
              case "upper-first":
                stringCollator = new Saxon.Hej.expr.sort.CaseFirstCollator(stringCollator, true, uri);
                break ;
              default:
                throw new Saxon.Hej.trans.XPathException("case-order must be lower-first, upper-first, or #default");
            }
            return stringCollator;
        }
        public string getCollationURI() {
            return uri;
        }
        public int compareStrings(Saxon.Hej.str.UnicodeString a, Saxon.Hej.str.UnicodeString b) {
            a = a.tidy();
            b = b.tidy();
            Saxon.Hej.regex.charclass.Categories.Category letters = Saxon.Hej.regex.charclass.Categories.getCategory("L");
            Saxon.Hej.regex.charclass.Categories.Category upperCase = Saxon.Hej.regex.charclass.Categories.getCategory("Lu");
            Saxon.Hej.regex.charclass.Categories.Category lowerCase = Saxon.Hej.regex.charclass.Categories.getCategory("Ll");
            int diff = baseCollator.compareStrings(a, b);
            if ((diff != 0)) {
                return diff;
            }
            long i = 0;
            long j = 0;
            while (true) {
                while ((((i<a.length()) && (j<b.length())) && (a.codePointAt(i) == b.codePointAt(j)))) {
                    i++;
                    j++;
                }
                while (((i<a.length()) && !(letters.test(a.codePointAt(i))))) {
                    i++;
                }
                while (((j<b.length()) && !(letters.test(b.codePointAt(j))))) {
                    j++;
                }
                if ((i>=a.length())) {
                    return 0;
                }
                if ((j>=b.length())) {
                    return 0;
                }
                bool aFirst = (upperFirst ? upperCase.test(a.codePointAt(i++)) : lowerCase.test(a.codePointAt(i++)));
                bool bFirst = (upperFirst ? upperCase.test(b.codePointAt(j++)) : lowerCase.test(b.codePointAt(j++)));
                if ((aFirst && !(bFirst))) {
                    return -1;
                }
                if ((bFirst && !(aFirst))) {
                    return +(1);
                }
            }
        }
        public bool comparesEqual(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            return (compareStrings(s1, s2) == 0);
        }
        public Saxon.Hej.expr.sort.AtomicMatchKey getCollationKey(Saxon.Hej.str.UnicodeString s) {
            return baseCollator.getCollationKey(s);
        }
    
    }
}