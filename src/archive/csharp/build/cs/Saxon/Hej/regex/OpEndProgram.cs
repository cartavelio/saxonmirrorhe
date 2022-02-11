using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class OpEndProgram : Saxon.Hej.regex.Operation {
        public override int getMatchLength() {
            return 0;
        }
        public override int matchesEmptyString() {
            return MATCHES_ZLS_ANYWHERE;
        }
        public override Saxon.Hej.z.IntIterator iterateMatches(Saxon.Hej.regex.REMatcher matcher, int position) {
            if (matcher.anchoredMatch) {
                if ((position>=matcher.search.length())) {
                    return new Saxon.Hej.z.IntSingletonIterator(position);
                } else {
                    return Saxon.Hej.expr.sort.EmptyIntIterator.getInstance();
                }
            } else {
                matcher.setParenEnd(0, position);
                return new Saxon.Hej.z.IntSingletonIterator(position);
            }
        }
        public override string display() {
            return "\\Z";
        }
    
    }
}