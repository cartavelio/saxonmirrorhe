using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class OpAtom : Saxon.Hej.regex.Operation {
        private readonly Saxon.Hej.str.UnicodeString atom;
        private readonly int len;
        internal OpAtom(Saxon.Hej.str.UnicodeString atom)  {
            this.atom = atom;
            this.len = atom.length32();
        }
        internal Saxon.Hej.str.UnicodeString getAtom() {
            return atom;
        }
        public override int getMatchLength() {
            return len;
        }
        public override int matchesEmptyString() {
            return ((len == 0) ? MATCHES_ZLS_ANYWHERE : MATCHES_ZLS_NEVER);
        }
        public override Saxon.Hej.regex.charclass.CharacterClass getInitialCharacterClass(bool caseBlind) {
            if ((len == 0)) {
                return Saxon.Hej.regex.charclass.EmptyCharacterClass.getInstance();
            } else {
                if (caseBlind) {
                    Saxon.Hej.z.IntSet set;
                    int ch = atom.codePointAt(0);
                    int[] variants = Saxon.Hej.regex.CaseVariants.getCaseVariants(ch);
                    if ((variants.Length>0)) {
                        set = new Saxon.Hej.z.IntHashSet(variants.Length);
                        set.add(ch);
                        foreach (int v in variants) {
                            set.add(v);
                        }
                        return new Saxon.Hej.regex.charclass.IntSetCharacterClass(set);
                    }
                }
            }
            return new Saxon.Hej.regex.charclass.SingletonCharacterClass(atom.codePointAt(0));
        }
        public override Saxon.Hej.z.IntIterator iterateMatches(Saxon.Hej.regex.REMatcher matcher, int position) {
            Saxon.Hej.str.UnicodeString @in = matcher.search;
            if (((position+len)>@in.length())) {
                return Saxon.Hej.expr.sort.EmptyIntIterator.getInstance();
            }
            if (matcher.program.flags.isCaseIndependent()) {
                for (int i = 0; (i<len); i++) {
                    if (!(matcher.equalCaseBlind(@in.codePointAt((position+i)), atom.codePointAt(i)))) {
                        return Saxon.Hej.expr.sort.EmptyIntIterator.getInstance();
                    }
                }
            } else {
                for (int i = 0; (i<len); i++) {
                    if ((@in.codePointAt((position+i)) != atom.codePointAt(i))) {
                        return Saxon.Hej.expr.sort.EmptyIntIterator.getInstance();
                    }
                }
            }
            return new Saxon.Hej.z.IntSingletonIterator((position+len));
        }
        public override string display() {
            return atom.ToString();
        }
    
    }
}