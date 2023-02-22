using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.jiter {
    public class MonoIterator<T>  : System.Collections.Generic.IEnumerator<T> {
        private readonly T thing;
        private bool gone;
        public MonoIterator(T thing)  {
            gone = false;
            this.thing = thing;
        }
        public bool hasNext() {
            return !(gone);
        }
        public T next() {
            if (gone) {
                throw new Saxon.Ejava.util.NoSuchElementException();
            } else {
                gone = true;
                return thing;
            }
        }
        
    /* Add methods to implement IEnumerable */

        object System.Collections.IEnumerator.Current => Current;
        public T Current { get; private set; }
        public bool MoveNext() {
            if (hasNext()) {Current = next(); return true;} else return false;
        }
        public void Reset() {throw new NotImplementedException();}
        public void Dispose() {}
      
    }
}