using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class EmptyAtomicSequence : Saxon.Hej.om.AtomicSequence {
        private EmptyAtomicSequence()  {
        }
        private readonly static Saxon.Hej.om.EmptyAtomicSequence INSTANCE = new Saxon.Hej.om.EmptyAtomicSequence();
        public static Saxon.Hej.om.EmptyAtomicSequence getInstance() {
            return INSTANCE;
        }
        public Saxon.Hej.om.Item head() {
            return null;
        }
        public Saxon.Hej.om.SequenceIterator iterate() {
            return Saxon.Hej.tree.iter.EmptyIterator.ofAtomic();
        }
        public Saxon.Hej.om.Item itemAt(int n) {
            return null;
        }
        public int getLength() {
            return 0;
        }
        public Saxon.Hej.str.UnicodeString getCanonicalLexicalRepresentation() {
            return Saxon.Hej.str.EmptyUnicodeString.getInstance();
        }
        public Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return Saxon.Hej.str.EmptyUnicodeString.getInstance();
        }
        public string getStringValue() {
            return "";
        }
        public Saxon.Hej.om.GroundedValue subsequence(int start, int length) {
            return this;
        }
        public bool effectiveBooleanValue() {
            return false;
        }
        public Saxon.Hej.om.GroundedValue reduce() {
            return this;
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.value.AtomicValue> GetEnumerator() {
            return new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>().GetEnumerator();;
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
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