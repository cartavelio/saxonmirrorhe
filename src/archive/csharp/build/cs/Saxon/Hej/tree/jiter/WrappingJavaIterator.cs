using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.jiter {
    public class WrappingJavaIterator<T>  : System.Collections.Generic.IEnumerator<T> where T : Saxon.Hej.om.Item {
        private readonly Saxon.Hej.tree.iter.LookaheadIterator input;
        public WrappingJavaIterator(Saxon.Hej.om.SequenceIterator @in)  {
            try {
                this.input = Saxon.Hej.tree.iter.LookaheadIteratorImpl.makeLookaheadIterator(@in);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Ejava.lang.AssertionError(e);
            }
        }
        public bool hasNext() {
            return input.hasNext();
        }
        public T next() {
            return (T)(input.next());
        }
        public void remove() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
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