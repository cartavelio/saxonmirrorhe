using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class AtomicCounter {
    // Injecting C# code
        private long counter = 0;
    // End of injected C# code
        public AtomicCounter(int initialValue)  {
            init(initialValue);
        }
        private void init(int initialValue) {
            counter = initialValue;
        }
        public long getAndIncrement() {
            return System.Threading.Interlocked.Increment(ref counter);
        }
        public long incrementAndGet() {
            return (getAndIncrement()+1L);
        }
        public long get() {
            return counter;
        }
    
    }
}