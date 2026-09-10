using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.jiter {
    public class ConcatenatingIterator<E>  : System.Collections.Generic.IEnumerator<E> {
        internal System.Collections.Generic.IEnumerator<E> first;
        internal Saxon.Ejava.util.function.Supplier<System.Collections.Generic.IEnumerator<E>> second;
        internal System.Collections.Generic.IEnumerator<E> active;
        public ConcatenatingIterator(System.Collections.Generic.IEnumerator<E> first, Saxon.Ejava.util.function.Supplier<System.Collections.Generic.IEnumerator<E>> second)  {
            this.first = first;
            this.second = second;
            this.active = first;
        }
        public bool hasNext() {
            if (active.MoveNext() /*UNSAFE CONVERSION*/) {
                return true;
            } else {
                if ((active == first)) {
                    first = null;
                    active = second();
                    return active.MoveNext() /*UNSAFE CONVERSION*/;
                } else {
                    return false;
                }
            }
        }
        public E next() {
            return active.Current;
        }
        
    /* Add methods to implement IEnumerable */

        object System.Collections.IEnumerator.Current => Current;
        public E Current { get; private set; }
        public bool MoveNext() {
            if (hasNext()) {Current = next(); return true;} else return false;
        }
        public void Reset() {throw new NotImplementedException();}
        public void Dispose() {}
      
    }
}