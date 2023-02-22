using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class TwoItemIterator : Saxon.Hej.om.SequenceIterator, Saxon.Hej.tree.iter.LookaheadIterator, Saxon.Hej.tree.iter.GroundedIterator, Saxon.Hej.expr.LastPositionFinder {
        private readonly Saxon.Hej.om.Item one;
        private readonly Saxon.Hej.om.Item two;
        private int pos = 0;
        public TwoItemIterator(Saxon.Hej.om.Item one, Saxon.Hej.om.Item two)  {
            this.one = one;
            this.two = two;
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return (pos<2);
        }
        public Saxon.Hej.om.Item next() {
            switch (pos++) {
              case 0:
                return one;
              case 1:
                return two;
              default:
                return null;
            }
        }
        public bool supportsGetLength() {
            return true;
        }
        public int getLength() {
            return 2;
        }
        public bool isActuallyGrounded() {
            return true;
        }
        public Saxon.Hej.om.GroundedValue getResidue() {
            switch (pos) {
              case 0:
                return new Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.om.Item>(new Saxon.Hej.om.Item[]{one, two});
              case 1:
                return two;
              default:
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        }
    
        /* Expanding default method materialize from interface GroundedIterator */
        public virtual Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.value.SequenceExtent.from(this).reduce();
        }

        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

        /* Expanding default method discharge from interface SequenceIterator */
        public virtual void discharge() {
        }

    }
}