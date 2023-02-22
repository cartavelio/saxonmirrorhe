using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class SingletonIterator : Saxon.Hej.om.SequenceIterator, Saxon.Hej.tree.iter.ReversibleIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.GroundedIterator, Saxon.Hej.tree.iter.LookaheadIterator {
        private readonly Saxon.Hej.om.Item item;
        internal bool gone = false;
        public SingletonIterator(Saxon.Hej.om.Item value)  {
            this.item = value;
        }
        public static Saxon.Hej.om.SequenceIterator makeIterator(Saxon.Hej.om.Item item) {
            if ((item == null)) {
                return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
            } else {
                return new Saxon.Hej.tree.iter.SingletonIterator(item);
            }
        }
        public static Saxon.Hej.tree.iter.SingletonIterator rawIterator(Saxon.Hej.om.Item item) {
            System.Diagnostics.Debug.Assert((item != null));
            return new Saxon.Hej.tree.iter.SingletonIterator(item);
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return !(gone);
        }
        public virtual Saxon.Hej.om.Item next() {
            if (gone) {
                return null;
            } else {
                gone = true;
                return item;
            }
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
            if ((item != null)) {
                return item;
            } else {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        }
        public Saxon.Hej.om.GroundedValue getResidue() {
            if (gone) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            } else {
                return item;
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