using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class EmptyIterator : Saxon.Hej.om.SequenceIterator, Saxon.Hej.tree.iter.ReversibleIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.GroundedIterator, Saxon.Hej.tree.iter.LookaheadIterator, Saxon.Hej.om.AtomizedValueIterator {
        private static readonly Saxon.Hej.tree.iter.EmptyIterator theInstance = new Saxon.Hej.tree.iter.EmptyIterator();
        public static Saxon.Hej.tree.iter.EmptyIterator getInstance() {
            return theInstance;
        }
        protected internal EmptyIterator()  {
        }
        public Saxon.Hej.om.AtomicSequence nextAtomizedValue() {
            return null;
        }
        public virtual Saxon.Hej.om.Item next() {
            return null;
        }
        public bool supportsGetLength() {
            return true;
        }
        public int getLength() {
            return 0;
        }
        public Saxon.Hej.om.SequenceIterator getReverseIterator() {
            return this;
        }
        public Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.value.EmptySequence.getInstance();
        }
        public Saxon.Hej.om.GroundedValue getResidue() {
            return Saxon.Hej.value.EmptySequence.getInstance();
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return false;
        }
        public bool isActuallyGrounded() {
            return true;
        }
        public static Saxon.Hej.tree.iter.AxisIterator ofNodes() {
            return Saxon.Hej.tree.iter.EmptyIterator.OfNodes.THE_INSTANCE;
        }
        public static Saxon.Hej.tree.iter.AtomicIterator ofAtomic() {
            return Saxon.Hej.tree.iter.EmptyIterator.OfAtomic.THE_INSTANCE;
        }
        private class OfNodes : Saxon.Hej.tree.iter.EmptyIterator, Saxon.Hej.tree.iter.AxisIterator {
            public readonly static Saxon.Hej.tree.iter.EmptyIterator.OfNodes THE_INSTANCE = new Saxon.Hej.tree.iter.EmptyIterator.OfNodes();
            public override Saxon.Hej.om.NodeInfo next() {
                return null;
            }
        
        }
        private class OfAtomic : Saxon.Hej.tree.iter.EmptyIterator, Saxon.Hej.tree.iter.AtomicIterator {
            public readonly static Saxon.Hej.tree.iter.EmptyIterator.OfAtomic THE_INSTANCE = new Saxon.Hej.tree.iter.EmptyIterator.OfAtomic();
            public override Saxon.Hej.value.AtomicValue next() {
                return null;
            }
        
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

    }
}