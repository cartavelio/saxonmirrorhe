using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntSingletonIterator : Saxon.Hej.z.IntIterator {
        private readonly int value;
        internal bool gone = false;
        public IntSingletonIterator(int value)  {
            this.value = value;
        }
        public bool hasNext() {
            return !(gone);
        }
        public int next() {
            gone = true;
            return value;
        }
    
    }
}