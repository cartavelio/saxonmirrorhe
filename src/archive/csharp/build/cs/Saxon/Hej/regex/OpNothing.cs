using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class OpNothing : Saxon.Hej.regex.Operation {
        public override Saxon.Hej.z.IntIterator iterateMatches(Saxon.Hej.regex.REMatcher matcher, int position) {
            return new Saxon.Hej.z.IntSingletonIterator(position);
        }
        public override int matchesEmptyString() {
            return MATCHES_ZLS_ANYWHERE;
        }
        public override int getMatchLength() {
            return 0;
        }
        public override string display() {
            return "()";
        }
    
    }
}