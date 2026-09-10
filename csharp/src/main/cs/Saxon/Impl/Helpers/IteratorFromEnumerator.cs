using System;
using System.Collections.Generic;

namespace Saxon.Impl.Helpers {
    public class IteratorFromEnumerator<T> : Saxon.Ejava.util.Iterator<T> {
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

// Copyright (c) 2021 - 2023 Saxonica Limited