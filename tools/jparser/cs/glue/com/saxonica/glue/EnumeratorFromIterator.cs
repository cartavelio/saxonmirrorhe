using System;
using System.Collections.Generic;

/**
 * A C# wrapper class that converts a Java Iterator to a C# Enumerator
 */
 
namespace com.saxonica.glue {

    public class EnumeratorFromIterator<T> : IEnumerator<T>
    {
        private java.util.Iterator<T> iter;
        public T Current = default(T);
        public EnumeratorFromIterator(java.util.Iterator<T> iter) {
            this.iter = iter;
        } 
        public bool MoveNext() {
            if (iter.hasNext()) {
                Current = iter.next();
                return true;
            } else {
                Current = default(T);
                return false;
            }
        }

    }
}
