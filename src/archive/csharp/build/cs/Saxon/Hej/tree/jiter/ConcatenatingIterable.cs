using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.jiter {
    public class ConcatenatingIterable<E>  : System.Collections.Generic.IEnumerable<E> {
        internal System.Collections.Generic.IEnumerable<E> first;
        internal System.Collections.Generic.IEnumerable<E> second;
        public ConcatenatingIterable(System.Collections.Generic.IEnumerable<E> first, System.Collections.Generic.IEnumerable<E> second)  {
            this.first = first;
            this.second = second;
        }
        public System.Collections.Generic.IEnumerator<E> GetEnumerator() {
            return new Saxon.Hej.tree.jiter.ConcatenatingIterator<E>(first.GetEnumerator(), () => second.GetEnumerator());
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    
    }
}