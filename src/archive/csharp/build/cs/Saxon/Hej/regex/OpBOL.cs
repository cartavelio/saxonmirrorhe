using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class OpBOL : Saxon.Hej.regex.Operation {
        public override int getMatchLength() {
            return 0;
        }
        public override int matchesEmptyString() {
            return MATCHES_ZLS_AT_START;
        }
        public override Saxon.Hej.z.IntIterator iterateMatches(Saxon.Hej.regex.REMatcher matcher, int position) {
            if ((position != 0)) {
                if (matcher.program.flags.isMultiLine()) {
                    if ((matcher.isNewline((position-1)) && !(((position>=matcher.search.length()))))) {
                        return new Saxon.Hej.z.IntSingletonIterator(position);
                    }
                }
                return Saxon.Hej.expr.sort.EmptyIntIterator.getInstance();
            }
            return new Saxon.Hej.z.IntSingletonIterator(position);
        }
        public override string display() {
            return "^";
        }
    
    }
}