using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class OpSequence : Saxon.Hej.regex.Operation {
        protected internal readonly System.Collections.Generic.IList<Saxon.Hej.regex.Operation> operations;
        internal OpSequence(System.Collections.Generic.IList<Saxon.Hej.regex.Operation> operations)  {
            this.operations = operations;
        }
        public System.Collections.Generic.IList<Saxon.Hej.regex.Operation> getOperations() {
            return operations;
        }
        public override int getMatchLength() {
            int len = 0;
            foreach (Saxon.Hej.regex.Operation o in operations) {
                int i = o.getMatchLength();
                if ((i == -1)) {
                    return -1;
                }
                len += i;
            }
            return len;
        }
        public override int getMinimumMatchLength() {
            int len = 0;
            foreach (Saxon.Hej.regex.Operation o in operations) {
                len += o.getMinimumMatchLength();
            }
            return len;
        }
        public override int matchesEmptyString() {
            bool matchesEmptyAnywhere = true;
            foreach (Saxon.Hej.regex.Operation o in operations) {
                int m = o.matchesEmptyString();
                if ((m == MATCHES_ZLS_NEVER)) {
                    return MATCHES_ZLS_NEVER;
                }
                if ((m != MATCHES_ZLS_ANYWHERE)) {
                    matchesEmptyAnywhere = false;
                    break ;
                }
            }
            if (matchesEmptyAnywhere) {
                return MATCHES_ZLS_ANYWHERE;
            }
            bool matchesBOL = true;
            foreach (Saxon.Hej.regex.Operation o in operations) {
                if ((((o.matchesEmptyString()&MATCHES_ZLS_AT_START)) == 0)) {
                    matchesBOL = false;
                    break ;
                }
            }
            if (matchesBOL) {
                return MATCHES_ZLS_AT_START;
            }
            bool matchesEOL = true;
            foreach (Saxon.Hej.regex.Operation o in operations) {
                if ((((o.matchesEmptyString()&MATCHES_ZLS_AT_END)) == 0)) {
                    matchesEOL = false;
                    break ;
                }
            }
            if (matchesEOL) {
                return MATCHES_ZLS_AT_END;
            }
            return 0;
        }
        public override bool containsCapturingExpressions() {
            foreach (Saxon.Hej.regex.Operation o in operations) {
                if (((o is Saxon.Hej.regex.OpCapture) || o.containsCapturingExpressions())) {
                    return true;
                }
            }
            return false;
        }
        public override Saxon.Hej.regex.charclass.CharacterClass getInitialCharacterClass(bool caseBlind) {
            Saxon.Hej.regex.charclass.CharacterClass result = Saxon.Hej.regex.charclass.EmptyCharacterClass.getInstance();
            foreach (Saxon.Hej.regex.Operation o in operations) {
                result = Saxon.Hej.regex.RECompiler.makeUnion(result, o.getInitialCharacterClass(caseBlind));
                if ((o.matchesEmptyString() == MATCHES_ZLS_NEVER)) {
                    return result;
                }
            }
            return result;
        }
        public override string display() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
            foreach (Saxon.Hej.regex.Operation op in operations) {
                fsb.append(op.display());
            }
            return fsb.ToString();
        }
        public override Saxon.Hej.regex.Operation optimize(Saxon.Hej.regex.REProgram program, Saxon.Hej.regex.REFlags flags) {
            if ((operations.Count == 0)) {
                return new Saxon.Hej.regex.OpNothing();
            } else {
                if ((operations.Count == 1)) {
                    return operations[0];
                } else {
                    for (int i = 0; (i<(operations.Count-1)); i++) {
                        Saxon.Hej.regex.Operation o1 = operations[i];
                        Saxon.Hej.regex.Operation o2 = o1.optimize(program, flags);
                        if ((o1 != o2)) {
                            operations[i] = o2;
                        }
                        if ((o2 is Saxon.Hej.regex.OpRepeat)) {
                            Saxon.Hej.regex.Operation o1r = ((Saxon.Hej.regex.OpRepeat)(o1)).getRepeatedOperation();
                            if (((o1r is Saxon.Hej.regex.OpAtom) || (o1r is Saxon.Hej.regex.OpCharClass))) {
                                Saxon.Hej.regex.Operation o2r = operations[(i+1)];
                                if (((((Saxon.Hej.regex.OpRepeat)(o1)).min == ((Saxon.Hej.regex.OpRepeat)(o1)).max) || Saxon.Hej.regex.RECompiler.noAmbiguity(o1r, o2r, flags.isCaseIndependent(), !(((Saxon.Hej.regex.OpRepeat)(o1)).greedy)))) {
                                    operations[i] = new Saxon.Hej.regex.OpUnambiguousRepeat(o1r, ((Saxon.Hej.regex.OpRepeat)(o1)).min, ((Saxon.Hej.regex.OpRepeat)(o1)).max);
                                }
                            }
                        }
                    }
                    return this;
                }
            }
        }
        public override Saxon.Hej.z.IntIterator iterateMatches(Saxon.Hej.regex.REMatcher matcher, int position) {
            System.Collections.Generic.Stack<Saxon.Hej.z.IntIterator> iterators = new System.Collections.Generic.Stack<Saxon.Hej.z.IntIterator>();
            Saxon.Hej.regex.REMatcher.State savedState = (containsCapturingExpressions() ? matcher.captureState() : null);
            int backtrackingLimit = matcher.getProgram().getBacktrackingLimit();
            return new Inner_IntIterator_d1509e827(this, matcher, iterators, backtrackingLimit, savedState, position);
        }
    
        private class Inner_IntIterator_d1509e827 : Saxon.Hej.z.IntIterator {
            private readonly OpSequence outerInstance;
            public Inner_IntIterator_d1509e827(OpSequence outerInstance, Saxon.Hej.regex.REMatcher matcher, System.Collections.Generic.Stack<Saxon.Hej.z.IntIterator> iterators, int backtrackingLimit, Saxon.Hej.regex.REMatcher.State savedState, int position) {
                this.outerInstance = outerInstance;
                this.matcher = matcher;
                this.iterators = iterators;
                this.backtrackingLimit = backtrackingLimit;
                this.savedState = savedState;
                this.position = position;
                primed = false;
            }
            private Saxon.Hej.regex.REMatcher matcher;
            private System.Collections.Generic.Stack<Saxon.Hej.z.IntIterator> iterators;
            private int backtrackingLimit;
            private Saxon.Hej.regex.REMatcher.State savedState;
            private int position;
            private bool primed;
            private int nextPos;
            public int advance() {
                int counter = 0;
                while (!((/*extraParam*/iterators.Count==0))) {
                    Saxon.Hej.z.IntIterator top = /*extraParam*/iterators.Peek();
                    while (/*localvar*/top.hasNext()) {
                        int p = /*localvar*/top.next();
                        /*extraParam*/matcher.clearCapturedGroupsBeyond(/*localvar*/p);
                        int i = /*extraParam*/iterators.Count;
                        if ((/*localvar*/i>=outerInstance.operations.Count)) {
                            return /*localvar*/p;
                        }
                        /*localvar*/top = outerInstance.operations[/*localvar*/i].iterateMatches(/*extraParam*/matcher, /*localvar*/p);
                        /*extraParam*/iterators.Push(/*localvar*/top);
                    }
                    /*extraParam*/iterators.Pop();
                    if (((/*extraParam*/backtrackingLimit>=0) && (/*localvar*/counter++>/*extraParam*/backtrackingLimit))) {
                        throw new Saxon.Hej.trans.UncheckedXPathException(new Saxon.Hej.trans.XPathException(((("Regex backtracking limit exceeded processing "+/*extraParam*/matcher.operation.display())+". Simplify the regular expression, ")+"or set Feature.REGEX_BACKTRACKING_LIMIT to -1 to remove this limit.")));
                    }
                }
                if ((/*extraParam*/savedState != null)) {
                    /*extraParam*/matcher.resetState(/*extraParam*/savedState);
                }
                return -1;
            }
            public bool hasNext() {
                if (!(/*field*/primed)) {
                    /*extraParam*/iterators.Push(outerInstance.operations[0].iterateMatches(/*extraParam*/matcher, /*extraParam*/position));
                    /*field*/primed = true;
                }
                /*field*/nextPos = /*method*/advance();
                return (/*field*/nextPos>=0);
            }
            public int next() {
                return /*field*/nextPos;
            }
        }

    }
}