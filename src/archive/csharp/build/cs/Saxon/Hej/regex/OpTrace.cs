using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class OpTrace : Saxon.Hej.regex.Operation {
        private Saxon.Hej.regex.Operation @base;
        private static int counter = 0;
        internal OpTrace(Saxon.Hej.regex.Operation @base)  {
            this.@base = @base;
        }
        public override Saxon.Hej.z.IntIterator iterateMatches(Saxon.Hej.regex.REMatcher matcher, int position) {
            Saxon.Hej.z.IntIterator baseIter = @base.iterateMatches(matcher, position);
            int iterNr = counter++;
            string clName = baseIter.GetType().Name;
            int lastDot = clName.LastIndexOf(".");
            string iterName = clName.Substring((lastDot+1));
            System.Console.Error.WriteLine(((((((((("Iterating over "+@base.GetType().Name)+" ")+@base.display())+" at position ")+position)+" returning ")+iterName)+" ")+iterNr));
            return new Inner_IntIterator_9(this, baseIter, iterNr);
        }
        public override int getMatchLength() {
            return @base.getMatchLength();
        }
        public override int matchesEmptyString() {
            return @base.matchesEmptyString();
        }
        public override Saxon.Hej.regex.Operation optimize(Saxon.Hej.regex.REProgram program, Saxon.Hej.regex.REFlags flags) {
            @base = @base.optimize(program, flags);
            return this;
        }
        public override string display() {
            return @base.display();
        }
    
        private class Inner_IntIterator_9 : Saxon.Hej.z.IntIterator {
            private readonly OpTrace outerInstance;
            public Inner_IntIterator_9(OpTrace outerInstance, Saxon.Hej.z.IntIterator baseIter, int iterNr) {
                this.outerInstance = outerInstance;
                this.baseIter = baseIter;
                this.iterNr = iterNr;
            }
            private Saxon.Hej.z.IntIterator baseIter;
            private int iterNr;
            public bool hasNext() {
                bool b = /*extraParam*/baseIter.hasNext();
                System.Console.Error.WriteLine(((("IntIterator "+/*extraParam*/iterNr)+" hasNext() = ")+/*localvar*/b));
                return /*localvar*/b;
            }
            public int next() {
                int n = /*extraParam*/baseIter.next();
                System.Console.Error.WriteLine(((("IntIterator "+/*extraParam*/iterNr)+" next() = ")+/*localvar*/n));
                return /*localvar*/n;
            }
        }

    }
}