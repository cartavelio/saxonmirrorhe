using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class ConcatenatingIntIterator : Saxon.Hej.z.IntIterator {
        internal Saxon.Hej.z.IntIterator first;
        internal Saxon.Ejava.util.function.Supplier<Saxon.Hej.z.IntIterator> second;
        internal Saxon.Hej.z.IntIterator active;
        public ConcatenatingIntIterator(Saxon.Hej.z.IntIterator first, Saxon.Ejava.util.function.Supplier<Saxon.Hej.z.IntIterator> second)  {
            this.first = first;
            this.second = second;
            this.active = first;
        }
        public bool hasNext() {
            if (active.hasNext()) {
                return true;
            } else {
                if ((active == first)) {
                    first = null;
                    active = second();
                    return active.hasNext();
                } else {
                    return false;
                }
            }
        }
        public int next() {
            return active.next();
        }
    
    }
}