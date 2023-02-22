using System.Collections.Generic;
using net.sf.saxon.expr;
using net.sf.saxon.om;
using net.sf.saxon.tree.iter;

namespace SaxonCS3.Saxon.Impl.Helpers {
    public class ReverseListIterator<T> : AbstractSequenceIterator, 
        UnfailingIterator, LastPositionFinder, ReversibleIterator
        where T : Item {
        private readonly IList<T> list;
        private int position;

        public ReverseListIterator(IList<T> list) {
            this.list = list;
            this.position = list.Count - 1;
        }

        public Item next() {
            if (position >= 0) {
                return list[position--];
            } else {
                return null;
            }
        }

        public int getLength() {
            return list.Count;
        }

        public SequenceIteratorProperty getProperties() {
            return SequenceIteratorProperty.LAST_POSITION_FINDER
        }

        public void close() {
            throw new System.NotImplementedException();
        }

        public SequenceIterator getReverseIterator() {
            return new ListIterator.Of<T>(list);
        }
        
        
    }
}