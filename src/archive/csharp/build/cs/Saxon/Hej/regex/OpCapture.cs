using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class OpCapture : Saxon.Hej.regex.Operation {
        internal int groupNr;
        internal Saxon.Hej.regex.Operation childOp;
        internal OpCapture(Saxon.Hej.regex.Operation childOp, int group)  {
            this.childOp = childOp;
            this.groupNr = group;
        }
        public override int getMatchLength() {
            return childOp.getMatchLength();
        }
        public override int getMinimumMatchLength() {
            return childOp.getMinimumMatchLength();
        }
        public override int matchesEmptyString() {
            return childOp.matchesEmptyString();
        }
        public override Saxon.Hej.regex.Operation optimize(Saxon.Hej.regex.REProgram program, Saxon.Hej.regex.REFlags flags) {
            childOp = childOp.optimize(program, flags);
            return this;
        }
        public override Saxon.Hej.z.IntIterator iterateMatches(Saxon.Hej.regex.REMatcher matcher, int position) {
            if ((((matcher.program.optimizationFlags&Saxon.Hej.regex.REProgram.OPT_HASBACKREFS)) != 0)) {
                matcher.startBackref[groupNr] = position;
            }
            Saxon.Hej.z.IntIterator basis = childOp.iterateMatches(matcher, position);
            return new Inner_IntIterator_d1439e194(this, matcher, position, basis);
        }
        public override string display() {
            return (("("+childOp.display())+")");
        }
    
        private class Inner_IntIterator_d1439e194 : Saxon.Hej.z.IntIterator {
            private readonly OpCapture outerInstance;
            public Inner_IntIterator_d1439e194(OpCapture outerInstance, Saxon.Hej.regex.REMatcher matcher, int position, Saxon.Hej.z.IntIterator basis) {
                this.outerInstance = outerInstance;
                this.matcher = matcher;
                this.position = position;
                this.basis = basis;
            }
            private Saxon.Hej.regex.REMatcher matcher;
            private int position;
            private Saxon.Hej.z.IntIterator basis;
            public bool hasNext() {
                return /*extraParam*/basis.hasNext();
            }
            public int next() {
                int next = /*extraParam*/basis.next();
                if ((outerInstance.groupNr>=/*extraParam*/matcher._captureState.parenCount)) {
                    /*extraParam*/matcher._captureState.parenCount = (outerInstance.groupNr+1);
                }
                /*extraParam*/matcher.setParenStart(outerInstance.groupNr, /*extraParam*/position);
                /*extraParam*/matcher.setParenEnd(outerInstance.groupNr, /*localvar*/next);
                if ((((/*extraParam*/matcher.program.optimizationFlags&Saxon.Hej.regex.REProgram.OPT_HASBACKREFS)) != 0)) {
                    /*extraParam*/matcher.startBackref[outerInstance.groupNr] = /*extraParam*/position;
                    /*extraParam*/matcher.endBackref[outerInstance.groupNr] = /*localvar*/next;
                }
                return /*localvar*/next;
            }
        }

    }
}