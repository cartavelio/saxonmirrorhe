using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class OpChoice : Saxon.Hej.regex.Operation {
        internal System.Collections.Generic.IList<Saxon.Hej.regex.Operation> branches;
        internal OpChoice(System.Collections.Generic.IList<Saxon.Hej.regex.Operation> branches)  {
            this.branches = branches;
        }
        public override int getMatchLength() {
            int @fixed = branches[0].getMatchLength();
            for (int i = 1; (i<branches.Count); i++) {
                if ((branches[i].getMatchLength() != @fixed)) {
                    return -1;
                }
            }
            return @fixed;
        }
        public override int getMinimumMatchLength() {
            int min = branches[0].getMinimumMatchLength();
            for (int i = 1; (i<branches.Count); i++) {
                int m = branches[i].getMinimumMatchLength();
                if ((m<min)) {
                    min = m;
                }
            }
            return min;
        }
        public override int matchesEmptyString() {
            int m = 0;
            foreach (Saxon.Hej.regex.Operation branch in branches) {
                int b = branch.matchesEmptyString();
                if ((b != MATCHES_ZLS_NEVER)) {
                    m |= b;
                }
            }
            return m;
        }
        public override bool containsCapturingExpressions() {
            foreach (Saxon.Hej.regex.Operation o in branches) {
                if (((o is Saxon.Hej.regex.OpCapture) || o.containsCapturingExpressions())) {
                    return true;
                }
            }
            return false;
        }
        public override Saxon.Hej.regex.charclass.CharacterClass getInitialCharacterClass(bool caseBlind) {
            Saxon.Hej.regex.charclass.CharacterClass result = Saxon.Hej.regex.charclass.EmptyCharacterClass.getInstance();
            foreach (Saxon.Hej.regex.Operation o in branches) {
                result = Saxon.Hej.regex.RECompiler.makeUnion(result, o.getInitialCharacterClass(caseBlind));
            }
            return result;
        }
        public override Saxon.Hej.regex.Operation optimize(Saxon.Hej.regex.REProgram program, Saxon.Hej.regex.REFlags flags) {
            for (int i = 0; (i<branches.Count); i++) {
                Saxon.Hej.regex.Operation o1 = branches[i];
                Saxon.Hej.regex.Operation o2 = o1.optimize(program, flags);
                if ((o1 != o2)) {
                    branches[i] = o2;
                }
            }
            return this;
        }
        public override Saxon.Hej.z.IntIterator iterateMatches(Saxon.Hej.regex.REMatcher matcher, int position) {
            return new Inner_IntIterator_d1497e472(this, matcher, position);
        }
        public override string display() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
            fsb.append("(?:");
            bool first = true;
            foreach (Saxon.Hej.regex.Operation branch in branches) {
                if (first) {
                    first = false;
                } else {
                    fsb.append('|');
                }
                fsb.append(branch.display());
            }
            fsb.append(")");
            return fsb.ToString();
        }
    
        private class Inner_IntIterator_d1497e472 : Saxon.Hej.z.IntIterator {
            private readonly OpChoice outerInstance;
            public Inner_IntIterator_d1497e472(OpChoice outerInstance, Saxon.Hej.regex.REMatcher matcher, int position) {
                this.outerInstance = outerInstance;
                this.matcher = matcher;
                this.position = position;
                branchIter = outerInstance.branches.GetEnumerator();
                currentIter = null;
                currentOp = null;
            }
            private Saxon.Hej.regex.REMatcher matcher;
            private int position;
            readonly System.Collections.Generic.IEnumerator<Saxon.Hej.regex.Operation> branchIter;
            internal Saxon.Hej.z.IntIterator currentIter;
            internal Saxon.Hej.regex.Operation currentOp;
            public bool hasNext() {
                while (true) {
                    if ((/*field*/currentIter == null)) {
                        if (/*field*/branchIter.MoveNext() /*UNSAFE CONVERSION*/) {
                            /*extraParam*/matcher.clearCapturedGroupsBeyond(/*extraParam*/position);
                            /*field*/currentOp = /*field*/branchIter.Current;
                            /*field*/currentIter = /*field*/currentOp.iterateMatches(/*extraParam*/matcher, /*extraParam*/position);
                        } else {
                            return false;
                        }
                    }
                    if (/*field*/currentIter.hasNext()) {
                        return true;
                    } else {
                        /*field*/currentIter = null;
                    }
                }
            }
            public int next() {
                return /*field*/currentIter.next();
            }
        }

    }
}