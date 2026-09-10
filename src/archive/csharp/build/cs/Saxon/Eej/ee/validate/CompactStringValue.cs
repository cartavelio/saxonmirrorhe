using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class CompactStringValue : Saxon.Hej.om.AtomicSequence {
        private readonly char[] value;
        public CompactStringValue(Saxon.Hej.str.UnicodeString content)  {
            this.value = content.ToString().ToCharArray();
        }
        public CompactStringValue(string content)  {
            this.value = content.ToCharArray();
        }
        public Saxon.Hej.om.Item head() {
            return new Saxon.Hej.value.StringValue(new string(value));
        }
        public Saxon.Hej.om.SequenceIterator iterate() {
            return Saxon.Hej.tree.iter.SingleAtomicIterator.makeIterator(((/*1430*/Saxon.Hej.value.StringValue)head()));
        }
        public Saxon.Hej.om.Item itemAt(int n) {
            return ((n == 0) ? ((/*1430*/Saxon.Hej.value.StringValue)head()) : null);
        }
        public int getLength() {
            return 1;
        }
        public Saxon.Hej.str.UnicodeString getCanonicalLexicalRepresentation() {
            return getUnicodeStringValue();
        }
        public override bool Equals(System.Object obj) {
            return ((obj is Saxon.Eej.ee.validate.CompactStringValue) && Saxon.Ejava.util.Arrays.Equals(value, ((Saxon.Eej.ee.validate.CompactStringValue)(obj)).value));
        }
        public override int GetHashCode() {
            int h = 0;
            foreach (int cp in value) {
                h = ((31*h)+cp);
            }
            return h;
        }
        public Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return Saxon.Hej.str.StringView.of(new string(value)).tidy();
        }
        public string getStringValue() {
            return new string(value);
        }
        public Saxon.Hej.om.GroundedValue subsequence(int start, int length) {
            return ((/*1430*/Saxon.Hej.value.StringValue)head()).subsequence(start, length);
        }
        public bool effectiveBooleanValue() {
            return (value.Length != 0);
        }
        public Saxon.Hej.om.GroundedValue reduce() {
            return ((/*1430*/Saxon.Hej.value.StringValue)head());
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.value.AtomicValue> GetEnumerator() {
            return new Saxon.Hej.tree.jiter.MonoIterator<Saxon.Hej.value.AtomicValue>(((/*1430*/Saxon.Hej.value.StringValue)head()));
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public override string ToString() {
            return new string(value);
        }
        public char[] getChars() {
            return value;
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
            return new Saxon.Hej.om.GroundedValueAsIterable(this);
        }

        /* Expanding default method containsNode from interface GroundedValue */
        public virtual bool containsNode(Saxon.Hej.om.NodeInfo sought) {
            return Saxon.Hej.expr.SingletonIntersectExpression.containsNode(iterate(), sought);
        }

        /* Expanding default method concatenate from interface GroundedValue */
        public virtual Saxon.Hej.om.GroundedValue concatenate(params Saxon.Hej.om.GroundedValue[]  others) {
            Saxon.Hej.ma.zeno.ZenoChain<Saxon.Hej.om.Item> chain = new Saxon.Hej.ma.zeno.ZenoChain<Saxon.Hej.om.Item>();
            chain = chain.addAll(asIterable());
            foreach (Saxon.Hej.om.GroundedValue val in others) {
                chain = chain.addAll(val.asIterable());
            }
            return new Saxon.Hej.ma.zeno.ZenoSequence(chain);
        }

        /* Expanding default method makeRepeatable from interface Sequence */
        public virtual Saxon.Hej.om.Sequence makeRepeatable() {
            return this;
        }

    }
}