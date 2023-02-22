using System;
using System.Collections.Generic;
using System.Management.Instrumentation;

namespace com.saxonica.glue {
    public class IteratorFromEnumerator<T> : java.util.Iterator<T> {
        private IEnumerator<T> enumerator;
        private bool moreToCome ;
        public IteratorFromEnumerator(IEnumerator<T> enumerator) {
            this.enumerator = enumerator;
            this.moreToCome = enumerator.MoveNext();
        }
        public bool hasNext() {
            return moreToCome;
        }

        public T next() {
            T nextItem = enumerator.Current;
            this.moreToCome = enumerator.MoveNext();
            return nextItem;
        }
    }
}