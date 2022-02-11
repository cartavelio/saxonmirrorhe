using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class BigRangeIterator : Saxon.Hej.tree.iter.RangeIterator, Saxon.Hej.tree.iter.AtomicIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.LookaheadIterator {
        internal System.Numerics.BigInteger start;
        internal System.Numerics.BigInteger step;
        internal System.Numerics.BigInteger currentValue;
        internal System.Numerics.BigInteger limit;
        internal bool descending;
        public BigRangeIterator(System.Numerics.BigInteger start, System.Numerics.BigInteger step, System.Numerics.BigInteger end)  {
            if ((System.Numerics.BigInteger.Divide(System.Numerics.BigInteger.Subtract(end, start), step).CompareTo(Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(System.Int32.MaxValue))>0)) {
                throw new Saxon.Hej.trans.XPathException("Saxon limit on sequence length exceeded (2^31)", "XPDY0130");
            }
            this.start = start;
            this.step = step;
            currentValue = System.Numerics.BigInteger.Subtract(start, step);
            limit = end;
            descending = (Saxon.Impl.Helpers.BigIntegerUtils.Signum(step)<0);
        }
        public override Saxon.Hej.om.GroundedValue getResidue() {
            return Saxon.Hej.value.SequenceExtent.makeResidue(this);
        }
        public override Saxon.Hej.value.IntegerValue getFirst() {
            return Saxon.Hej.value.IntegerValue.makeIntegerValue(start);
        }
        public override Saxon.Hej.value.IntegerValue getLast() {
            return Saxon.Hej.value.IntegerValue.makeIntegerValue(limit);
        }
        public override Saxon.Hej.value.IntegerValue getMin() {
            return (descending ? getLast() : getFirst());
        }
        public override Saxon.Hej.value.IntegerValue getMax() {
            return (descending ? getFirst() : getLast());
        }
        public override Saxon.Hej.value.IntegerValue getStep() {
            return Saxon.Hej.value.IntegerValue.makeIntegerValue(step);
        }
        private bool test(System.Numerics.BigInteger value) {
            return (descending ? (value.CompareTo(limit)>=0) : (value.CompareTo(limit)<=0));
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return test(System.Numerics.BigInteger.Add(currentValue, step));
        }
        public override Saxon.Hej.value.IntegerValue next() {
            currentValue = System.Numerics.BigInteger.Add(currentValue, step);
            if (!(test(currentValue))) {
                return null;
            }
            return Saxon.Hej.value.IntegerValue.makeIntegerValue(currentValue);
        }
        public bool supportsGetLength() {
            return true;
        }
        public int getLength() {
            System.Numerics.BigInteger len = Saxon.Impl.Helpers.BigIntegerUtils.DivideAndRemainder(System.Numerics.BigInteger.Subtract(limit, start), step)[0];
            if ((len.CompareTo(Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(System.Int32.MaxValue))>0)) {
                throw new Saxon.Hej.trans.UncheckedXPathException(new Saxon.Hej.trans.XPathException("Sequence exceeds Saxon limit (32-bit integer)"));
            }
            return (((int)len)+1);
        }
        public override bool isActuallyGrounded() {
            return true;
        }
    
    }
}