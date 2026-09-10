using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public sealed class EmptySequence : Saxon.Hej.om.GroundedValue {
        private static readonly Saxon.Hej.value.EmptySequence THE_INSTANCE = new Saxon.Hej.value.EmptySequence();
        private EmptySequence()  {
        }
        public static Saxon.Hej.value.EmptySequence getInstance() {
            return THE_INSTANCE;
        }
        public Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return Saxon.Hej.str.EmptyUnicodeString.getInstance();
        }
        public string getStringValue() {
            return "";
        }
        public Saxon.Hej.om.Item head() {
            return null;
        }
        public Saxon.Hej.om.SequenceIterator iterate() {
            return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
        }
        public int getLength() {
            return 0;
        }
        public override bool Equals(System.Object other) {
            if (!((((other is Saxon.Hej.om.GroundedValue) && (((Saxon.Hej.om.GroundedValue)(other)).getLength() == 0))))) {
                throw new System.InvalidCastException((("Cannot compare "+other.GetType())+" to empty sequence"));
            }
            return true;
        }
        public override int GetHashCode() {
            return 42;
        }
        public bool effectiveBooleanValue() {
            return false;
        }
        public Saxon.Hej.om.Item itemAt(int n) {
            return null;
        }
        public Saxon.Hej.om.GroundedValue subsequence(int min, int length) {
            return this;
        }
        public override string ToString() {
            return "()";
        }
        public Saxon.Hej.om.GroundedValue reduce() {
            return this;
        }
    
        /* Expanding default method materialize from interface Sequence */
        public Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.om.SequenceTool.toGroundedValue(iterate());
        }

        /* Expanding default method toShortString from interface GroundedValue */
        public string toShortString() {
            return Saxon.Hej.trans.Err.depictSequence(this).ToString();
        }

        /* Expanding default method asIterable from interface GroundedValue */
        public System.Collections.Generic.IEnumerable<Saxon.Hej.om.Item> asIterable() {
            return new Saxon.Hej.om.GroundedValueAsIterable(this);
        }

        /* Expanding default method containsNode from interface GroundedValue */
        public bool containsNode(Saxon.Hej.om.NodeInfo sought) {
            return Saxon.Hej.expr.SingletonIntersectExpression.containsNode(iterate(), sought);
        }

        /* Expanding default method concatenate from interface GroundedValue */
        public Saxon.Hej.om.GroundedValue concatenate(params Saxon.Hej.om.GroundedValue[]  others) {
            Saxon.Hej.ma.zeno.ZenoChain<Saxon.Hej.om.Item> chain = new Saxon.Hej.ma.zeno.ZenoChain<Saxon.Hej.om.Item>();
            chain = chain.addAll(asIterable());
            foreach (Saxon.Hej.om.GroundedValue val in others) {
                chain = chain.addAll(val.asIterable());
            }
            return new Saxon.Hej.ma.zeno.ZenoSequence(chain);
        }

        /* Expanding default method makeRepeatable from interface Sequence */
        public Saxon.Hej.om.Sequence makeRepeatable() {
            return this;
        }

    }
}