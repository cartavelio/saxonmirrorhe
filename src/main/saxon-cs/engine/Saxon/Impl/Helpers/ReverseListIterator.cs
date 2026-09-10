using System.Collections.Generic;
using net.sf.saxon.expr;
using net.sf.saxon.om;
using net.sf.saxon.tree.iter;

namespace Saxon.Impl.Helpers {
    public class ReverseListIterator<T> : SequenceIterator, 
            LastPositionFinder, LookaheadIterator, ReversibleIterator
            where T : Item {
        
        private readonly IList<T> list;
        private int position;

        public ReverseListIterator(IList<T> list) {
            this.list = list;
            this.position = list.Count - 1;
        }

        public bool supportsHasNext() {
            return true;
        }

        public bool hasNext() {
            return position >= 0;
        }

        public Item next() {
            if (position >= 0) {
                return list[position--];
            } else {
                return null;
            }
        }

        public bool supportsGetLength() {
            return true;
        }

        public int getLength() {
            return list.Count;
        }

        public void close() {
            // no action
        }

        public SequenceIterator getReverseIterator() {
            return new ListIterator.Of<T>(list);
        }
        
        
    }
}