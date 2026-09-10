using System.Collections;
using System.Collections.Generic;
using java.util;

namespace Saxon.Impl.Helpers {
    public class IterableFromEnumerable<T> : java.lang.Iterable<T> {
        private readonly IEnumerable<T> enumerable;
        
        public IterableFromEnumerable(IEnumerable<T> enumerable) {
            this.enumerable = enumerable;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return enumerable.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator() {
            throw enumerable.GetEnumerator();
        }

        public Iterator<T> iterator() {
            return new IteratorFromEnumerator<T>(GetEnumerator());
        }

 
    }
}