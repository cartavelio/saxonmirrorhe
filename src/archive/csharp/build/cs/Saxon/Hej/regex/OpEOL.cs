using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class OpEOL : Saxon.Hej.regex.Operation {
        public override int getMatchLength() {
            return 0;
        }
        public override int matchesEmptyString() {
            return MATCHES_ZLS_AT_END;
        }
        public override Saxon.Hej.z.IntIterator iterateMatches(Saxon.Hej.regex.REMatcher matcher, int position) {
            Saxon.Hej.str.UnicodeString search = matcher.search;
            if (matcher.program.flags.isMultiLine()) {
                if ((((0>=search.length()) || (position>=search.length())) || matcher.isNewline(position))) {
                    return new Saxon.Hej.z.IntSingletonIterator(position);
                } else {
                    return Saxon.Hej.expr.sort.EmptyIntIterator.getInstance();
                }
            } else {
                if (((0>=search.length()) || (position>=search.length()))) {
                    return new Saxon.Hej.z.IntSingletonIterator(position);
                } else {
                    return Saxon.Hej.expr.sort.EmptyIntIterator.getInstance();
                }
            }
        }
        public override string display() {
            return "$";
        }
    
    }
}