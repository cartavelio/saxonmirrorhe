using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class AscendingRangeIterator : Saxon.Hej.tree.iter.RangeIterator, Saxon.Hej.tree.iter.AtomicIterator, Saxon.Hej.tree.iter.ReversibleIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.LookaheadIterator, Saxon.Hej.tree.iter.GroundedIterator {
        internal long start;
        internal long step;
        internal long currentValue;
        internal long limit;
        public static Saxon.Hej.tree.iter.AtomicIterator makeRangeIterator(Saxon.Hej.value.IntegerValue start, Saxon.Hej.value.IntegerValue step, Saxon.Hej.value.IntegerValue end) {
            if ((((start == null) || (step == null)) || (end == null))) {
                return Saxon.Hej.tree.iter.EmptyIterator.ofAtomic();
            } else {
                int direction = step.compareTo(Saxon.Hej.value.Int64Value.ZERO);
                if (((direction == 0) || (start.compareTo(end)>0))) {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofAtomic();
                }
                if ((((start is Saxon.Hej.value.BigIntegerValue) || (step is Saxon.Hej.value.BigIntegerValue)) || (end is Saxon.Hej.value.BigIntegerValue))) {
                    if ((direction<0)) {
                        return new Saxon.Hej.expr.BigRangeIterator(end.asBigInteger(), step.asBigInteger(), start.asBigInteger());
                    } else {
                        return new Saxon.Hej.expr.BigRangeIterator(start.asBigInteger(), step.asBigInteger(), end.asBigInteger());
                    }
                } else {
                    long startVal = start.longValue();
                    long stepVal = step.longValue();
                    long endVal = end.longValue();
                    if (((((endVal-startVal))/stepVal)>System.Int32.MaxValue)) {
                        throw new Saxon.Hej.trans.XPathException("Saxon limit on sequence length exceeded (2^31)", "XPDY0130");
                    }
                    if ((stepVal>0)) {
                        return new Saxon.Hej.expr.AscendingRangeIterator(startVal, stepVal, endVal);
                    } else {
                        return new Saxon.Hej.expr.DescendingRangeIterator(endVal, -(stepVal), startVal);
                    }
                }
            }
        }
        public AscendingRangeIterator(long start, long step, long end)  {
            this.start = start;
            this.step = step;
            currentValue = (start-step);
            limit = end;
            if ((step != 1L)) {
                limit = (start+(((((end-start))/step))*step));
            }
        }
        public override Saxon.Hej.value.IntegerValue getFirst() {
            return new Saxon.Hej.value.Int64Value(start);
        }
        public override Saxon.Hej.value.IntegerValue getLast() {
            return new Saxon.Hej.value.Int64Value(limit);
        }
        public override Saxon.Hej.value.IntegerValue getMin() {
            return new Saxon.Hej.value.Int64Value(start);
        }
        public override Saxon.Hej.value.IntegerValue getMax() {
            return new Saxon.Hej.value.Int64Value(limit);
        }
        public override Saxon.Hej.value.IntegerValue getStep() {
            return new Saxon.Hej.value.Int64Value(step);
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return ((currentValue+step)<=limit);
        }
        public override Saxon.Hej.value.IntegerValue next() {
            currentValue += step;
            if ((currentValue>limit)) {
                return null;
            }
            return Saxon.Hej.value.Int64Value.makeIntegerValue(currentValue);
        }
        public override bool isActuallyGrounded() {
            return true;
        }
        public bool supportsGetLength() {
            return true;
        }
        public int getLength() {
            return (int)(((((limit-start))+1)));
        }
        public Saxon.Hej.om.SequenceIterator getReverseIterator() {
            return new Saxon.Hej.expr.DescendingRangeIterator(limit, step, start);
        }
        public override Saxon.Hej.om.GroundedValue materialize() {
            return new Saxon.Hej.value.IntegerRange(start, step, limit);
        }
        public override Saxon.Hej.om.GroundedValue getResidue() {
            return new Saxon.Hej.value.IntegerRange(currentValue, step, limit);
        }
    
    }
}