using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class OpGreedyFixed : Saxon.Hej.regex.OpRepeat {
        private readonly int len;
        internal OpGreedyFixed(Saxon.Hej.regex.Operation op, int min, int max, int len) : base (op, min, max, true) {
            this.len = len;
        }
        public override int getMatchLength() {
            return ((min == max) ? (min*len) : -1);
        }
        public override int matchesEmptyString() {
            if ((min == 0)) {
                return MATCHES_ZLS_ANYWHERE;
            }
            return op.matchesEmptyString();
        }
        public override Saxon.Hej.regex.Operation optimize(Saxon.Hej.regex.REProgram program, Saxon.Hej.regex.REFlags flags) {
            if ((max == 0)) {
                return new Saxon.Hej.regex.OpNothing();
            }
            if ((op.getMatchLength() == 0)) {
                return op;
            }
            op = op.optimize(program, flags);
            return this;
        }
        public override Saxon.Hej.z.IntIterator iterateMatches(Saxon.Hej.regex.REMatcher matcher, int position) {
            int guard = matcher.search.length32();
            if ((max<System.Int32.MaxValue)) {
                guard = System.Math.Min(guard, (position+(len*max)));
            }
            if (((position>=guard) && (min>0))) {
                return Saxon.Hej.expr.sort.EmptyIntIterator.getInstance();
            }
            int p = position;
            int matches = 0;
            while ((p<=guard)) {
                Saxon.Hej.z.IntIterator it = op.iterateMatches(matcher, p);
                bool matched = false;
                if (it.hasNext()) {
                    matched = true;
                    it.next();
                }
                if (matched) {
                    matches++;
                    p += len;
                    if ((matches == max)) {
                        break ;
                    }
                } else {
                    break ;
                }
            }
            if ((matches<min)) {
                return Saxon.Hej.expr.sort.EmptyIntIterator.getInstance();
            }
            return new Saxon.Hej.z.IntStepIterator(p, -(len), (position+(len*min)));
        }
    
    }
}