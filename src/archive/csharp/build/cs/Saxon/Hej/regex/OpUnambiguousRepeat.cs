using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class OpUnambiguousRepeat : Saxon.Hej.regex.OpRepeat {
        internal OpUnambiguousRepeat(Saxon.Hej.regex.Operation op, int min, int max) : base (op, min, max, true) {
        }
        public override int matchesEmptyString() {
            if ((min == 0)) {
                return MATCHES_ZLS_ANYWHERE;
            }
            return op.matchesEmptyString();
        }
        public override int getMatchLength() {
            if (((op.getMatchLength() != -1) && (min == max))) {
                return (op.getMatchLength()*min);
            } else {
                return -1;
            }
        }
        public override Saxon.Hej.regex.Operation optimize(Saxon.Hej.regex.REProgram program, Saxon.Hej.regex.REFlags flags) {
            op = op.optimize(program, flags);
            return this;
        }
        public override Saxon.Hej.z.IntIterator iterateMatches(Saxon.Hej.regex.REMatcher matcher, int position) {
            int guard = matcher.search.length32();
            int p = position;
            int matches = 0;
            while (((matches<max) && (p<=guard))) {
                Saxon.Hej.z.IntIterator it = op.iterateMatches(matcher, p);
                if (it.hasNext()) {
                    matches++;
                    p = it.next();
                } else {
                    break ;
                }
            }
            if ((matches<min)) {
                return Saxon.Hej.expr.sort.EmptyIntIterator.getInstance();
            } else {
                return new Saxon.Hej.z.IntSingletonIterator(p);
            }
        }
    
    }
}