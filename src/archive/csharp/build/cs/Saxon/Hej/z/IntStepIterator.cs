using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntStepIterator : Saxon.Hej.z.IntIterator {
        private int current;
        private readonly int step;
        private readonly int limit;
        public IntStepIterator(int start, int step, int limit)  {
            this.current = start;
            this.step = step;
            this.limit = limit;
        }
        public bool hasNext() {
            return ((step>0) ? (current<=limit) : (current>=limit));
        }
        public int next() {
            int n = current;
            current += step;
            return n;
        }
    
    }
}