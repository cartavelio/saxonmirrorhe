using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public abstract class Operation {
        protected internal const int MATCHES_ZLS_AT_START = 1;
        protected internal const int MATCHES_ZLS_AT_END = 2;
        protected internal const int MATCHES_ZLS_ANYWHERE = 7;
        protected internal const int MATCHES_ZLS_NEVER = 1024;
        public abstract Saxon.Hej.z.IntIterator iterateMatches(Saxon.Hej.regex.REMatcher matcher, int position);
        public virtual int getMatchLength() {
            return -1;
        }
        public virtual int getMinimumMatchLength() {
            int @fixed = getMatchLength();
            return System.Math.Max(@fixed, 0);
        }
        public abstract int matchesEmptyString();
        public virtual bool containsCapturingExpressions() {
            return false;
        }
        public virtual Saxon.Hej.regex.charclass.CharacterClass getInitialCharacterClass(bool caseBlind) {
            return Saxon.Hej.regex.charclass.EmptyCharacterClass.getComplement();
        }
        public virtual Saxon.Hej.regex.Operation optimize(Saxon.Hej.regex.REProgram program, Saxon.Hej.regex.REFlags flags) {
            return this;
        }
        public abstract string display();
        public virtual int getMaxLoopingDepth() {
            return 0;
        }
        protected internal class ForceProgressIterator : Saxon.Hej.z.IntIterator {
            private readonly Saxon.Hej.z.IntIterator @base;
            internal int countZeroLength = 0;
            internal int currentPos = -1;
            internal int loopingDepth = 1;
            internal int maxTries = 10;
            internal ForceProgressIterator(Saxon.Hej.z.IntIterator @base, int loopingDepth)  {
                this.@base = @base;
                this.loopingDepth = System.Math.Max(loopingDepth, 1);
            }
            public bool hasNext() {
                return ((countZeroLength<=maxTries) && @base.hasNext());
            }
            public int next() {
                int p = @base.next();
                if ((p == currentPos)) {
                    countZeroLength++;
                } else {
                    countZeroLength = 0;
                    currentPos = p;
                    double limit = System.Math.Min(System.Int32.MaxValue, (System.Math.Pow((currentPos+2), loopingDepth)/2));
                    maxTries = (int)(limit);
                }
                return p;
            }
        
        }
    
    }
}