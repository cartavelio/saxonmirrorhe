using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class IteratorWrapper : Saxon.Hej.om.SequenceIterator {
        internal System.Collections.Generic.IEnumerator<Saxon.Hej.om.Item> iterator;
        public IteratorWrapper(System.Collections.Generic.IEnumerator<Saxon.Hej.om.Item> iterator)  {
            this.iterator = iterator;
        }
        public Saxon.Hej.om.Item next() {
            return (iterator.MoveNext() ? iterator.Current : null);
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

    }
}