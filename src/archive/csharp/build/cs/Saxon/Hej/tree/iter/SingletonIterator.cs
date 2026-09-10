using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class SingletonIterator : Saxon.Hej.om.SequenceIterator, Saxon.Hej.om.FocusIterator, Saxon.Hej.tree.iter.ReversibleIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.GroundedIterator, Saxon.Hej.tree.iter.LookaheadIterator {
        private readonly Saxon.Hej.om.Item item;
        private int currentPosition = -1;
        public SingletonIterator(Saxon.Hej.om.Item value)  {
            System.Diagnostics.Debug.Assert((value != null));
            this.item = value;
        }
        public static Saxon.Hej.om.SequenceIterator makeIterator(Saxon.Hej.om.Item item) {
            if ((item == null)) {
                return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
            } else {
                return new Saxon.Hej.tree.iter.SingletonIterator(item);
            }
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return (currentPosition<0);
        }
        public virtual Saxon.Hej.om.Item next() {
            return ((++currentPosition == 0) ? item : null);
        }
        public Saxon.Hej.om.Item current() {
            return ((currentPosition == 0) ? item : null);
        }
        public int position() {
            return (currentPosition+1);
        }
        public void close() {
        }
        public bool supportsGetLength() {
            return true;
        }
        public int getLength() {
            return 1;
        }
        public bool isActuallyGrounded() {
            return true;
        }
        public virtual Saxon.Hej.om.SequenceIterator getReverseIterator() {
            return new Saxon.Hej.tree.iter.SingletonIterator(item);
        }
        public Saxon.Hej.om.Item getValue() {
            return item;
        }
        public Saxon.Hej.om.GroundedValue materialize() {
            return item;
        }
        public Saxon.Hej.om.GroundedValue getResidue() {
            return ((currentPosition<0) ? item : Saxon.Hej.value.EmptySequence.getInstance());
        }
    
    }
}