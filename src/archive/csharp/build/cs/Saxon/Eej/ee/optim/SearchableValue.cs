using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.optim {
    public abstract class SearchableValue : Saxon.Hej.om.GroundedValue {
        protected internal Saxon.Hej.om.Item[] value = null;
        protected internal int end;
        public static Saxon.Eej.ee.optim.SearchableValue makeSearchableValue(Saxon.Hej.om.SequenceIterator iter) {
            Saxon.Hej.om.Item[] array;
            int end;
            if (Saxon.Hej.om.SequenceTool.supportsGetLength(iter)) {
                end = Saxon.Hej.om.SequenceTool.getLength(iter);
                array = new Saxon.Hej.om.Item[end];
                int i = 0;
                Saxon.Hej.om.Item it;
                while (((it = iter.next()) != null)) {
                    array[i++] = it;
                }
            } else {
                int used = 0;
                array = new Saxon.Hej.om.Item[20];
                Saxon.Hej.om.Item it;
                while (((it = iter.next()) != null)) {
                    if ((used>=(array.Length-1))) {
                        array = Saxon.Ejava.util.Arrays.copyOf(array, (used*2));
                    }
                    array[used++] = it;
                }
                if (((array.Length-used)>100)) {
                    array = Saxon.Ejava.util.Arrays.copyOf(array, used);
                }
                end = used;
            }
            if ((end<8)) {
                return new Saxon.Eej.ee.optim.SmallSearchableValue(array, end);
            } else {
                return new Saxon.Eej.ee.optim.IndexedValue(array, end);
            }
        }
        public Saxon.Hej.om.SequenceIterator iterate() {
            return new Saxon.Hej.tree.iter.ArrayIterator.Of<Saxon.Hej.om.Item>(value, 0, end);
        }
        public Saxon.Hej.om.Item itemAt(int n) {
            if (((n<0) || (n>=end))) {
                return null;
            }
            return value[n];
        }
        public int getLength() {
            return end;
        }
        public abstract Saxon.Hej.om.SequenceIterator findItems(Saxon.Hej.expr.Expression use, Saxon.Hej.om.SequenceIterator values, bool convertUntypedToOther, bool firstOnly, Saxon.Hej.expr.sort.AtomicComparer comparer, Saxon.Hej.expr.XPathContext context);
        public Saxon.Hej.om.GroundedValue subsequence(int start, int length) {
            if ((length>end)) {
                length = end;
            }
            return new Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.om.Item>(Saxon.Impl.Helpers.ListUtils.SubList(Saxon.Ejava.util.Arrays.asList(value), start, (start+length)));
        }
        public bool effectiveBooleanValue() {
            return Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(iterate());
        }
        public Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return Saxon.Hej.om.SequenceTool.getStringValue(this);
        }
        public string getStringValue() {
            return Saxon.Hej.om.SequenceTool.stringify(this);
        }
        public Saxon.Hej.om.Item head() {
            return ((value.Length>0) ? value[0] : null);
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