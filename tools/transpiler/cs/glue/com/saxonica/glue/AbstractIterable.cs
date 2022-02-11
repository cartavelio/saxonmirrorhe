using System.Collections;
using System.Collections.Generic;
using java.util;

namespace com.saxonica.glue {
    
    /**
     * Supplies implementation of GetEnumerator() to a class that only supplies iterator()
     */
    public abstract class AbstractIterable<T> : java.lang.Iterable<T> {
        IEnumerator IEnumerable.GetEnumerator() {
            return new EnumeratorFromIterator<T>(iterator());
        }

        public IEnumerator<T> GetEnumerator() {
            return new EnumeratorFromIterator<T>(iterator());
        }

        public abstract Iterator<T> iterator();
    }
}