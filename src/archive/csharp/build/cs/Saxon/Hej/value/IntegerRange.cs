using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class IntegerRange : Saxon.Hej.om.AtomicSequence {
        public long start;
        public long step;
        public long end;
        public IntegerRange(long start, long step, long end)  {
            if ((step == 0)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("step = 0 in IntegerRange");
            }
            if (((end != start) && (((end>start) != (step>0))))) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("end before start in IntegerRange");
            }
            if ((System.Math.Abs((((end-start))/step))>System.Int32.MaxValue)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Maximum length of sequence in Saxon is "+System.Int32.MaxValue));
            }
            this.start = start;
            this.step = step;
            this.end = (start+((step*((end-start)))/step));
        }
        public long getStart() {
            return start;
        }
        public long getStep() {
            return step;
        }
        public long getEnd() {
            return end;
        }
        public Saxon.Hej.om.SequenceIterator iterate() {
            if ((step>0)) {
                return new Saxon.Hej.expr.AscendingRangeIterator(start, step, end);
            } else {
                return new Saxon.Hej.expr.DescendingRangeIterator(start, -(step), end);
            }
        }
        public Saxon.Hej.om.Item itemAt(int n) {
            if (((n<0) || (n>=getLength()))) {
                return null;
            }
            return Saxon.Hej.value.Int64Value.makeIntegerValue((start+((n*step))));
        }
        public Saxon.Hej.om.GroundedValue subsequence(int start, int length) {
            if ((length<=0)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            long newStart = (this.start+System.Math.Max(start, 0));
            long newEnd = ((newStart+(((long)(length)*step)))-1L);
            if ((newEnd>end)) {
                newEnd = end;
            }
            if ((newEnd>=newStart)) {
                return new Saxon.Hej.value.IntegerRange(newStart, step, newEnd);
            } else {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        }
        public int getLength() {
            return ((int)(((((end-start))/step)))+1);
        }
        public Saxon.Hej.om.Item head() {
            return new Saxon.Hej.value.Int64Value(start);
        }
        public Saxon.Hej.str.UnicodeString getCanonicalLexicalRepresentation() {
            return getUnicodeStringValue();
        }
        public Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            try {
                return Saxon.Hej.om.SequenceTool.getStringValue(this);
            } catch (Saxon.Hej.trans.XPathException err) {
                throw new Saxon.Ejava.lang.AssertionError(err);
            }
        }
        public string getStringValue() {
            try {
                return Saxon.Hej.om.SequenceTool.stringify(this);
            } catch (Saxon.Hej.trans.XPathException err) {
                throw new Saxon.Ejava.lang.AssertionError(err);
            }
        }
        public bool effectiveBooleanValue() {
            return Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(((/*1430*/Saxon.Hej.tree.iter.AtomicIterator)iterate()));
        }
        public Saxon.Hej.om.GroundedValue reduce() {
            if ((start == end)) {
                return itemAt(0);
            } else {
                return this;
            }
        }
        public override string ToString() {
            return ((((("("+start)+(((step == 1) ? "" : ((" by "+step)))))+" to ")+end)+")");
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.value.AtomicValue> GetEnumerator() {
            return new Saxon.Hej.value.IntegerRange.IntegerRangeIterator(this);
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        private class IntegerRangeIterator : System.Collections.Generic.IEnumerator<Saxon.Hej.value.AtomicValue> {
            private Saxon.Hej.value.IntegerRange range;
            private long current;
            public IntegerRangeIterator(Saxon.Hej.value.IntegerRange range)  {
                this.range = range;
                current = range.start;
            }
            public bool hasNext() {
                return (current<=range.end);
            }
            public void remove() {
                throw new Saxon.Ejava.lang.UnsupportedOperationException();
            }
            public Saxon.Hej.value.AtomicValue next() {
                return new Saxon.Hej.value.Int64Value(current++);
            }
                
        /* Add methods to implement IEnumerable */

            object System.Collections.IEnumerator.Current => Current;
            public Saxon.Hej.value.AtomicValue Current { get; private set; }
            public bool MoveNext() {
                if (hasNext()) {Current = next(); return true;} else return false;
            }
            public void Reset() {throw new NotImplementedException();}
            public void Dispose() {}
      
        }
    
        /* Expanding default method materialize from interface Sequence */
        public virtual Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.om.SequenceTool.toGroundedValue(iterate());
        }

        /* Expanding default method toShortString from interface GroundedValue */
        public virtual string toShortString() {
            return Saxon.Hej.trans.Err.depictSequence(this).ToString();
        }

        /* Expanding default method asIterable from interface GroundedValue */
        public virtual System.Collections.Generic.IEnumerable<Saxon.Hej.om.Item> asIterable() {
            return new Saxon.Hej.om.GroundedValueAsIterable<Saxon.Hej.om.Item>(this);
        }

        /* Expanding default method containsNode from interface GroundedValue */
        public virtual bool containsNode(Saxon.Hej.om.NodeInfo sought) {
            return Saxon.Hej.expr.SingletonIntersectExpression.containsNode(iterate(), sought);
        }

        /* Expanding default method concatenate from interface GroundedValue */
        public virtual Saxon.Hej.om.GroundedValue concatenate(params Saxon.Hej.om.GroundedValue[]  others) {
            Saxon.Hej.ma.zeno.ZenoSequence chain = new Saxon.Hej.ma.zeno.ZenoSequence();
            chain.appendSequence(this);
            foreach (Saxon.Hej.om.GroundedValue seq in others) {
                chain.appendSequence(seq);
            }
            return chain;
        }

        /* Expanding default method makeRepeatable from interface Sequence */
        public virtual Saxon.Hej.om.Sequence makeRepeatable() {
            return this;
        }

    }
}