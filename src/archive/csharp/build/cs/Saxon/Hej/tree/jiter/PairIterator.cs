using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.jiter {
    public class PairIterator<T>  : System.Collections.Generic.IEnumerator<T> {
        private readonly T one;
        private readonly T two;
        private int pos = 0;
        public PairIterator(T one, T two)  {
            this.one = one;
            this.two = two;
        }
        public bool hasNext() {
            return (pos<2);
        }
        public T next() {
            switch (pos++) {
              case 0:
                return one;
              case 1:
                return two;
              default:
                throw new Saxon.Ejava.util.NoSuchElementException();
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