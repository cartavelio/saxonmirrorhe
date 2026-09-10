using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class OpCharClass : Saxon.Hej.regex.Operation {
        private readonly Saxon.Hej.z.IntPredicateProxy predicate;
        internal OpCharClass(Saxon.Hej.z.IntPredicateProxy predicate)  {
            this.predicate = predicate;
        }
        public Saxon.Hej.z.IntPredicateProxy getPredicate() {
            return predicate;
        }
        public override int getMatchLength() {
            return 1;
        }
        public override int matchesEmptyString() {
            return MATCHES_ZLS_NEVER;
        }
        public override Saxon.Hej.regex.charclass.CharacterClass getInitialCharacterClass(bool caseBlind) {
            if ((predicate is Saxon.Hej.regex.charclass.CharacterClass)) {
                return (Saxon.Hej.regex.charclass.CharacterClass)(predicate);
            } else {
                return base.getInitialCharacterClass(caseBlind);
            }
        }
        public override Saxon.Hej.z.IntIterator iterateMatches(Saxon.Hej.regex.REMatcher matcher, int position) {
            Saxon.Hej.str.UnicodeString @in = matcher.search;
            if (((position<@in.length()) && predicate.test(@in.codePointAt(position)))) {
                return new Saxon.Hej.z.IntSingletonIterator((position+1));
            } else {
                return Saxon.Hej.expr.sort.EmptyIntIterator.getInstance();
            }
        }
        public override string display() {
            if ((predicate is Saxon.Hej.z.IntSetPredicate)) {
                Saxon.Hej.z.IntSet s = ((Saxon.Hej.z.IntSetPredicate)(predicate)).getIntSet();
                if ((s is Saxon.Hej.z.IntSingletonSet)) {
                    return (""+(char)(((Saxon.Hej.z.IntSingletonSet)(s)).getMember()));
                } else {
                    if ((s is Saxon.Hej.z.IntRangeSet)) {
                        Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
                        Saxon.Hej.z.IntRangeSet irs = (Saxon.Hej.z.IntRangeSet)(s);
                        fsb.append("[");
                        for (int i = 0; (i<irs.getNumberOfRanges()); i++) {
                            fsb.append((char)(irs.getStartPoints()[1]));
                            fsb.append("-");
                            fsb.append((char)(irs.getEndPoints()[1]));
                        }
                        fsb.append("[");
                        return fsb.ToString();
                    } else {
                        return "[....]";
                    }
                }
            } else {
                return "[....]";
            }
        }
    
    }
}