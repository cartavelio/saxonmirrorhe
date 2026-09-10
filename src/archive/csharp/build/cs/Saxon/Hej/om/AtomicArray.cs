using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class AtomicArray : Saxon.Hej.om.AtomicSequence {
        private static readonly System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> emptyAtomicList = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
        public static Saxon.Hej.om.AtomicArray EMPTY_ATOMIC_ARRAY = new Saxon.Hej.om.AtomicArray(emptyAtomicList);
        private readonly System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> content;
        public AtomicArray(System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> content)  {
            this.content = content;
        }
        public AtomicArray(Saxon.Hej.om.SequenceIterator iter)  {
            System.Collections.Generic.List<Saxon.Hej.value.AtomicValue> list = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>(10);
            Saxon.Hej.om.SequenceTool.supply(iter, (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => list.Add((Saxon.Hej.value.AtomicValue)(item)))));
            content = list;
        }
        public Saxon.Hej.om.Item head() {
            return (content.Count==0 ? null : content[0]);
        }
        public Saxon.Hej.om.SequenceIterator iterate() {
            return new Saxon.Hej.tree.iter.ListIterator.OfAtomic<Saxon.Hej.value.AtomicValue>(content);
        }
        public Saxon.Hej.om.Item itemAt(int n) {
            if (((n>=0) && (n<content.Count))) {
                return content[n];
            } else {
                return null;
            }
        }
        public int getLength() {
            return content.Count;
        }
        public Saxon.Hej.om.GroundedValue subsequence(int start, int length) {
            if ((start<0)) {
                start = 0;
            }
            if (((start+length)>content.Count)) {
                length = (content.Count-start);
            }
            return new Saxon.Hej.om.AtomicArray(Saxon.Impl.Helpers.ListUtils.SubList(content, start, (start+length)));
        }
        public Saxon.Hej.str.UnicodeString getCanonicalLexicalRepresentation() {
            return getUnicodeStringValue();
        }
        public Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            Saxon.Hej.str.UnicodeBuilder ub = new Saxon.Hej.str.UnicodeBuilder();
            bool first = true;
            foreach (Saxon.Hej.value.AtomicValue av in content) {
                if (!(first)) {
                    ub.append(' ');
                } else {
                    first = false;
                }
                ub.accept(av.getUnicodeStringValue());
            }
            return ub.toUnicodeString();
        }
        public string getStringValue() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(64);
            bool first = true;
            foreach (Saxon.Hej.value.AtomicValue av in content) {
                if (!(first)) {
                    sb.append(' ');
                } else {
                    first = false;
                }
                sb.append(av.getStringValue());
            }
            return sb.ToString();
        }
        public bool effectiveBooleanValue() {
            return Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(((/*1430*/Saxon.Hej.tree.iter.AtomicIterator)iterate()));
        }
        public Saxon.Hej.om.GroundedValue reduce() {
            int len = getLength();
            if ((len == 0)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            } else {
                if ((len == 1)) {
                    return itemAt(0);
                } else {
                    return this;
                }
            }
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.value.AtomicValue> GetEnumerator() {
            return content.GetEnumerator();
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