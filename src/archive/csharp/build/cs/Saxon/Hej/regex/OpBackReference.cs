using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class OpBackReference : Saxon.Hej.regex.Operation {
        internal int groupNr;
        internal OpBackReference(int groupNr)  {
            this.groupNr = groupNr;
        }
        public override int matchesEmptyString() {
            return 0;
        }
        public override Saxon.Hej.z.IntIterator iterateMatches(Saxon.Hej.regex.REMatcher matcher, int position) {
            int s = matcher.startBackref[groupNr];
            int e = matcher.endBackref[groupNr];
            if (((s == -1) || (e == -1))) {
                return Saxon.Hej.expr.sort.EmptyIntIterator.getInstance();
            }
            if ((s == e)) {
                return new Saxon.Hej.z.IntSingletonIterator(position);
            }
            int l = (e-s);
            Saxon.Hej.str.UnicodeString search = matcher.search;
            if ((((position+l)-1)>=search.length())) {
                return Saxon.Hej.expr.sort.EmptyIntIterator.getInstance();
            }
            if (matcher.program.flags.isCaseIndependent()) {
                for (int i = 0; (i<l); i++) {
                    if (!(matcher.equalCaseBlind(search.codePointAt((position+i)), search.codePointAt((s+i))))) {
                        return Saxon.Hej.expr.sort.EmptyIntIterator.getInstance();
                    }
                }
            } else {
                for (int i = 0; (i<l); i++) {
                    if ((search.codePointAt((position+i)) != search.codePointAt((s+i)))) {
                        return Saxon.Hej.expr.sort.EmptyIntIterator.getInstance();
                    }
                }
            }
            return new Saxon.Hej.z.IntSingletonIterator((position+l));
        }
        public override string display() {
            return ("\\"+groupNr);
        }
    
    }
}