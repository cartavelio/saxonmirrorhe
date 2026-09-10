using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    internal class LFUCacheEntryWithCounter<W>  {
        public W value;
        internal int counter;
        public LFUCacheEntryWithCounter(W value)  {
            this.value = value;
            this.counter = 0;
        }
    
    }
}