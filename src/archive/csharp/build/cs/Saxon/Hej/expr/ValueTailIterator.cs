using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class ValueTailIterator : Saxon.Hej.om.SequenceIterator, Saxon.Hej.tree.iter.GroundedIterator, Saxon.Hej.tree.iter.LookaheadIterator {
        private readonly Saxon.Hej.om.GroundedValue baseValue;
        private readonly int start;
        private int pos = 0;
        public ValueTailIterator(Saxon.Hej.om.GroundedValue @base, int start)  {
            baseValue = @base;
            this.start = start;
            pos = 0;
        }
        public Saxon.Hej.om.Item next() {
            return baseValue.itemAt((start+pos++));
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return (baseValue.itemAt((start+pos)) != null);
        }
        public bool isActuallyGrounded() {
            return true;
        }
        public Saxon.Hej.om.GroundedValue materialize() {
            if ((start == 0)) {
                return baseValue;
            } else {
                return baseValue.subsequence(start, System.Int32.MaxValue);
            }
        }
        public Saxon.Hej.om.GroundedValue getResidue() {
            if (((start == 0) && (pos == 0))) {
                return baseValue;
            } else {
                return baseValue.subsequence((start+pos), System.Int32.MaxValue);
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