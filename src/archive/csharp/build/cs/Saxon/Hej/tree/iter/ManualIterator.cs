using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class ManualIterator : Saxon.Hej.om.FocusIterator, Saxon.Hej.om.SequenceIterator, Saxon.Hej.tree.iter.ReversibleIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.GroundedIterator, Saxon.Hej.tree.iter.LookaheadIterator {
        private Saxon.Hej.om.Item item;
        private int _position;
        private Saxon.Ejava.util.function.Supplier<int> lengthFinder;
        public ManualIterator()  {
            item = null;
            _position = 0;
        }
        public ManualIterator(Saxon.Hej.om.Item value, int position)  {
            this.item = value;
            this._position = position;
        }
        public ManualIterator(Saxon.Hej.om.Item value)  {
            this.item = value;
            this._position = 1;
            this.lengthFinder = () => 1;
        }
        public void setContextItem(Saxon.Hej.om.Item value) {
            this.item = value;
        }
        public void setLengthFinder(Saxon.Ejava.util.function.Supplier<int> finder) {
            this.lengthFinder = finder;
        }
        public void incrementPosition() {
            _position++;
        }
        public void setPosition(int position) {
            this._position = position;
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return (position() != getLength());
        }
        public virtual Saxon.Hej.om.Item next() {
            return null;
        }
        public Saxon.Hej.om.Item current() {
            return item;
        }
        public int position() {
            return _position;
        }
        public bool supportsGetLength() {
            return true;
        }
        public virtual int getLength() {
            if ((lengthFinder == null)) {
                throw new Saxon.Hej.trans.UncheckedXPathException("Saxon streaming restriction: last() cannot be used when consuming a sequence of streamed nodes, even if the items being processed are grounded");
            } else {
                int length = lengthFinder();
                return length;
            }
        }
        public bool isActuallyGrounded() {
            return true;
        }
        public Saxon.Hej.om.SequenceIterator getReverseIterator() {
            return new Saxon.Hej.tree.iter.ManualIterator(item);
        }
        public Saxon.Hej.om.GroundedValue materialize() {
            return item;
        }
        public Saxon.Hej.om.GroundedValue getResidue() {
            return item;
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

        /* Expanding default method discharge from interface SequenceIterator */
        public virtual void discharge() {
        }

    }
}