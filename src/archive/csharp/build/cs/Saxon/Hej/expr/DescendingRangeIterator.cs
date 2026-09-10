using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class DescendingRangeIterator : Saxon.Hej.tree.iter.RangeIterator, Saxon.Hej.tree.iter.AtomicIterator, Saxon.Hej.tree.iter.ReversibleIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.LookaheadIterator {
        internal long start;
        internal long step;
        internal long currentValue;
        internal long limit;
        public DescendingRangeIterator(long start, long step, long end)  {
            System.Diagnostics.Debug.Assert((step>0));
            System.Diagnostics.Debug.Assert(((start-end)<=System.Int32.MaxValue));
            this.start = start;
            this.step = step;
            currentValue = (start+step);
            limit = end;
            if ((step != 1L)) {
                limit = (start+(((((end-start))/step))*step));
            }
        }
        public override bool isActuallyGrounded() {
            return true;
        }
        public override Saxon.Hej.om.GroundedValue materialize() {
            return new Saxon.Hej.value.IntegerRange(start, -(step), limit);
        }
        public override Saxon.Hej.om.GroundedValue getResidue() {
            return new Saxon.Hej.value.IntegerRange(currentValue, -(step), limit);
        }
        public override Saxon.Hej.value.IntegerValue getFirst() {
            return new Saxon.Hej.value.Int64Value(start);
        }
        public override Saxon.Hej.value.IntegerValue getLast() {
            return new Saxon.Hej.value.Int64Value(limit);
        }
        public override Saxon.Hej.value.IntegerValue getMin() {
            return new Saxon.Hej.value.Int64Value(limit);
        }
        public override Saxon.Hej.value.IntegerValue getMax() {
            return new Saxon.Hej.value.Int64Value(start);
        }
        public override Saxon.Hej.value.IntegerValue getStep() {
            return new Saxon.Hej.value.Int64Value(-(step));
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return ((currentValue-step)>=limit);
        }
        public override Saxon.Hej.value.IntegerValue next() {
            currentValue -= step;
            if ((currentValue<limit)) {
                return null;
            }
            return Saxon.Hej.value.Int64Value.makeIntegerValue(currentValue);
        }
        public bool supportsGetLength() {
            return true;
        }
        public int getLength() {
            return (int)(((((start-limit))+1)));
        }
        public Saxon.Hej.om.SequenceIterator getReverseIterator() {
            return new Saxon.Hej.expr.AscendingRangeIterator(start, step, limit);
        }
    
    }
}