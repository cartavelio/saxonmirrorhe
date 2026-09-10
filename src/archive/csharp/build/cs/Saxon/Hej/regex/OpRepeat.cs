using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class OpRepeat : Saxon.Hej.regex.Operation {
        protected internal Saxon.Hej.regex.Operation op;
        protected internal int min;
        protected internal int max;
        internal bool greedy;
        internal OpRepeat(Saxon.Hej.regex.Operation op, int min, int max, bool greedy)  {
            this.op = op;
            this.min = min;
            this.max = max;
            this.greedy = greedy;
        }
        internal Saxon.Hej.regex.Operation getRepeatedOperation() {
            return op;
        }
        public override int matchesEmptyString() {
            if ((min == 0)) {
                return MATCHES_ZLS_ANYWHERE;
            }
            return op.matchesEmptyString();
        }
        public override bool containsCapturingExpressions() {
            return ((op is Saxon.Hej.regex.OpCapture) || op.containsCapturingExpressions());
        }
        public override Saxon.Hej.regex.charclass.CharacterClass getInitialCharacterClass(bool caseBlind) {
            return op.getInitialCharacterClass(caseBlind);
        }
        public override int getMatchLength() {
            return (((min == max) && (op.getMatchLength()>=0)) ? (min*op.getMatchLength()) : -1);
        }
        public override int getMinimumMatchLength() {
            return (min*op.getMinimumMatchLength());
        }
        public override Saxon.Hej.regex.Operation optimize(Saxon.Hej.regex.REProgram program, Saxon.Hej.regex.REFlags flags) {
            op = op.optimize(program, flags);
            if (((min == 0) && (op.matchesEmptyString() == MATCHES_ZLS_ANYWHERE))) {
                min = 1;
            }
            return this;
        }
        public override Saxon.Hej.z.IntIterator iterateMatches(Saxon.Hej.regex.REMatcher matcher, int position) {
            System.Collections.Generic.Stack<Saxon.Hej.z.IntIterator> iterators = new System.Collections.Generic.Stack<Saxon.Hej.z.IntIterator>();
            System.Collections.Generic.Stack<int> positions = new System.Collections.Generic.Stack<int>();
            int bound = System.Math.Min(max, ((matcher.search.length32()-position)+1));
            int p = position;
            if (greedy) {
                if (((min == 0) && !(matcher.history.isDuplicateZeroLengthMatch(this, position)))) {
                    iterators.Push(new Saxon.Hej.z.IntSingletonIterator(position));
                    positions.Push(p);
                }
                for (int i = 0; (i<bound); i++) {
                    Saxon.Hej.z.IntIterator it = op.iterateMatches(matcher, p);
                    if (it.hasNext()) {
                        p = it.next();
                        iterators.Push(it);
                        positions.Push(p);
                    } else {
                        if ((iterators.Count==0)) {
                            return Saxon.Hej.expr.sort.EmptyIntIterator.getInstance();
                        } else {
                            break ;
                        }
                    }
                }
                Saxon.Hej.z.IntIterator @base = new Inner_IntIterator_11(this, matcher, iterators, positions, bound, position);
                return new Saxon.Hej.regex.Operation.ForceProgressIterator(@base);
            } else {
                Saxon.Hej.z.IntIterator iter = new Inner_IntIterator_5(this, matcher, iterators, positions, bound, position);
                return new Saxon.Hej.regex.Operation.ForceProgressIterator(iter);
            }
        }
        public override string display() {
            string quantifier;
            if (((min == 0) && (max == System.Int32.MaxValue))) {
                quantifier = "*";
            } else {
                if (((min == 1) && (max == System.Int32.MaxValue))) {
                    quantifier = "+";
                } else {
                    if (((min == 0) && (max == 1))) {
                        quantifier = "?";
                    } else {
                        quantifier = (((("{"+min)+",")+max)+"}");
                    }
                }
            }
            if (!(greedy)) {
                quantifier += "?";
            }
            return (op.display()+quantifier);
        }
    
        private class Inner_IntIterator_5 : Saxon.Hej.z.IntIterator {
            private readonly OpRepeat outerInstance;
            public Inner_IntIterator_5(OpRepeat outerInstance, Saxon.Hej.regex.REMatcher matcher, System.Collections.Generic.Stack<Saxon.Hej.z.IntIterator> iterators, System.Collections.Generic.Stack<System.Int32> positions, int bound, int position) {
                this.outerInstance = outerInstance;
                this.matcher = matcher;
                this.iterators = iterators;
                this.positions = positions;
                this.bound = bound;
                this.position = position;
                pos = /*extraParam*/position;
                counter = 0;
            }
            private Saxon.Hej.regex.REMatcher matcher;
            private System.Collections.Generic.Stack<Saxon.Hej.z.IntIterator> iterators;
            private System.Collections.Generic.Stack<System.Int32> positions;
            private int bound;
            private int position;
            private int pos;
            private int counter;
            public void advance() {
                Saxon.Hej.z.IntIterator it = outerInstance.op.iterateMatches(/*extraParam*/matcher, /*field*/pos);
                if (/*localvar*/it.hasNext()) {
                    /*field*/pos = /*localvar*/it.next();
                    if ((++/*field*/counter>outerInstance.max)) {
                        /*field*/pos = -1;
                    }
                } else {
                    if (((outerInstance.min == 0) && (/*field*/counter == 0))) {
                        /*field*/counter++;
                    } else {
                        /*field*/pos = -1;
                    }
                }
            }
            public bool hasNext() {
                do {
                    /*method*/advance();
                } while (((/*field*/counter<outerInstance.min) && (/*field*/pos>=0)));
                return (/*field*/pos>=0);
            }
            public int next() {
                return /*field*/pos;
            }
        }

        private class Inner_IntIterator_11 : Saxon.Hej.z.IntIterator {
            private readonly OpRepeat outerInstance;
            public Inner_IntIterator_11(OpRepeat outerInstance, Saxon.Hej.regex.REMatcher matcher, System.Collections.Generic.Stack<Saxon.Hej.z.IntIterator> iterators, System.Collections.Generic.Stack<System.Int32> positions, int bound, int position) {
                this.outerInstance = outerInstance;
                this.matcher = matcher;
                this.iterators = iterators;
                this.positions = positions;
                this.bound = bound;
                this.position = position;
                primed = true;
            }
            private Saxon.Hej.regex.REMatcher matcher;
            private System.Collections.Generic.Stack<Saxon.Hej.z.IntIterator> iterators;
            private System.Collections.Generic.Stack<System.Int32> positions;
            private int bound;
            private int position;
            internal bool primed;
            public void advance() {
                Saxon.Hej.z.IntIterator top = /*extraParam*/iterators.Peek();
                if (/*localvar*/top.hasNext()) {
                    int p = /*localvar*/top.next();
                    /*extraParam*/positions.Pop();
                    /*extraParam*/positions.Push(/*localvar*/p);
                    while ((/*extraParam*/iterators.Count</*extraParam*/bound)) {
                        Saxon.Hej.z.IntIterator it = outerInstance.op.iterateMatches(/*extraParam*/matcher, /*localvar*/p);
                        if (/*localvar*/it.hasNext()) {
                            /*localvar*/p = /*localvar*/it.next();
                            /*extraParam*/iterators.Push(/*localvar*/it);
                            /*extraParam*/positions.Push(/*localvar*/p);
                        } else {
                            break ;
                        }
                    }
                } else {
                    /*extraParam*/iterators.Pop();
                    /*extraParam*/positions.Pop();
                }
            }
            public bool hasNext() {
                if ((/*field*/primed && (/*extraParam*/iterators.Count>=outerInstance.min))) {
                    return !((/*extraParam*/iterators.Count==0));
                } else {
                    if ((/*extraParam*/iterators.Count==0)) {
                        return false;
                    } else {
                        do {
                            /*method*/advance();
                        } while (((/*extraParam*/iterators.Count<outerInstance.min) && !((/*extraParam*/iterators.Count==0))));
                        return !((/*extraParam*/iterators.Count==0));
                    }
                }
            }
            public int next() {
                /*field*/primed = false;
                return /*extraParam*/positions.Peek();
            }
        }

    }
}