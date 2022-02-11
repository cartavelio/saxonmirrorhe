using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class OpReluctantFixed : Saxon.Hej.regex.OpRepeat {
        private readonly int len;
        internal OpReluctantFixed(Saxon.Hej.regex.Operation op, int min, int max, int len) : base (op, min, max, false) {
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
            op = op.optimize(program, flags);
            return this;
        }
        public override Saxon.Hej.z.IntIterator iterateMatches(Saxon.Hej.regex.REMatcher matcher, int position) {
            return new Inner_IntIterator_d1449e157(this, matcher, position);
        }
    
        private class Inner_IntIterator_d1449e157 : Saxon.Hej.z.IntIterator {
            private readonly OpReluctantFixed outerInstance;
            public Inner_IntIterator_d1449e157(OpReluctantFixed outerInstance, Saxon.Hej.regex.REMatcher matcher, int position) {
                this.outerInstance = outerInstance;
                this.matcher = matcher;
                this.position = position;
                pos = /*extraParam*/position;
                count = 0;
                started = false;
            }
            private Saxon.Hej.regex.REMatcher matcher;
            private int position;
            private int pos;
            private int count;
            private bool started;
            public bool hasNext() {
                if (!(/*field*/started)) {
                    /*field*/started = true;
                    while ((/*field*/count</*guess*/outerInstance.min)) {
                        Saxon.Hej.z.IntIterator child = /*guess*/outerInstance.op.iterateMatches(/*extraParam*/matcher, /*field*/pos);
                        if (/*localvar*/child.hasNext()) {
                            /*field*/pos = /*localvar*/child.next();
                            /*field*/count++;
                        } else {
                            return false;
                        }
                    }
                    return true;
                }
                if ((/*field*/count</*guess*/outerInstance.max)) {
                    /*extraParam*/matcher.clearCapturedGroupsBeyond(/*field*/pos);
                    Saxon.Hej.z.IntIterator child = /*guess*/outerInstance.op.iterateMatches(/*extraParam*/matcher, /*field*/pos);
                    if (/*localvar*/child.hasNext()) {
                        /*field*/pos = /*localvar*/child.next();
                        /*field*/count++;
                        return true;
                    }
                }
                return false;
            }
            public int next() {
                return /*field*/pos;
            }
        }

    }
}