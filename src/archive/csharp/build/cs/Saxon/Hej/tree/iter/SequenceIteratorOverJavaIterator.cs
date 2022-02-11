using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class SequenceIteratorOverJavaIterator<J>  : Saxon.Hej.om.SequenceIterator {
        private readonly System.Collections.Generic.IEnumerator<J> javaIterator;
        private readonly System.Func<J,Saxon.Hej.om.Item> mapper;
        public SequenceIteratorOverJavaIterator(System.Collections.Generic.IEnumerator<J> javaIterator, System.Func<J,Saxon.Hej.om.Item> mapper)  {
            this.javaIterator = javaIterator;
            this.mapper = mapper;
        }
        public Saxon.Hej.om.Item next() {
            if (javaIterator.MoveNext()) {
                return mapper(javaIterator.Current);
            } else {
                return null;
            }
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

        /* Expanding default method discharge from interface SequenceIterator */
        public virtual void discharge() {
        }

    }
}