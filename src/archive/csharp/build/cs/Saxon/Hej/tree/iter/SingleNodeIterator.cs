using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class SingleNodeIterator : Saxon.Hej.tree.iter.AxisIterator, Saxon.Hej.tree.iter.ReversibleIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.GroundedIterator, Saxon.Hej.tree.iter.LookaheadIterator {
        private readonly Saxon.Hej.om.NodeInfo item;
        private int position = 0;
        private SingleNodeIterator(Saxon.Hej.om.NodeInfo value)  {
            this.item = value;
        }
        public static Saxon.Hej.tree.iter.AxisIterator makeIterator(Saxon.Hej.om.NodeInfo item) {
            if ((item == null)) {
                return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
            } else {
                return new Saxon.Hej.tree.iter.SingleNodeIterator(item);
            }
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return (position == 0);
        }
        public Saxon.Hej.om.Item next() {
            if ((position == 0)) {
                position = 1;
                return item;
            } else {
                if ((position == 1)) {
                    position = -1;
                    return null;
                } else {
                    return null;
                }
            }
        }
        public bool supportsGetLength() {
            return true;
        }
        public int getLength() {
            return 1;
        }
        public Saxon.Hej.om.SequenceIterator getReverseIterator() {
            return new Saxon.Hej.tree.iter.SingleNodeIterator(item);
        }
        public bool isActuallyGrounded() {
            return true;
        }
        public Saxon.Hej.om.NodeInfo getValue() {
            return item;
        }
        public Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.om.SequenceTool.itemOrEmpty(item);
        }
        public Saxon.Hej.om.GroundedValue getResidue() {
            return Saxon.Hej.om.SequenceTool.itemOrEmpty(item);
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

        /* Expanding default method discharge from interface SequenceIterator */
        public virtual void discharge() {
        }

    }
}