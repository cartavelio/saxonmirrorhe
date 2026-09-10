using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public abstract class SequenceExtent : Saxon.Hej.om.GroundedValue {
        public static Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.om.Item> from(Saxon.Hej.om.SequenceIterator iter) {
            System.Collections.Generic.IList<Saxon.Hej.om.Item> list = new System.Collections.Generic.List<Saxon.Hej.om.Item>((!(Saxon.Hej.om.SequenceTool.supportsGetLength(iter)) ? 20 : ((Saxon.Hej.expr.LastPositionFinder)(iter)).getLength()));
            Saxon.Hej.om.Item item;
            while (((item = iter.next()) != null)) {
                list.Add(item);
            }
            return new Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.om.Item>(list);
        }
        public static Saxon.Hej.om.GroundedValue makeResidue(Saxon.Hej.om.SequenceIterator iter) {
            if (((iter is Saxon.Hej.tree.iter.GroundedIterator) && ((Saxon.Hej.tree.iter.GroundedIterator)(iter)).isActuallyGrounded())) {
                return ((Saxon.Hej.tree.iter.GroundedIterator)(iter)).getResidue();
            }
            Saxon.Hej.value.SequenceExtent extent = from(iter);
            return extent.reduce();
        }
        public static Saxon.Hej.om.GroundedValue makeSequenceExtent<T> (System.Collections.Generic.IList<T> input) where T : Saxon.Hej.om.Item {
            int len = input.Count;
            if ((len == 0)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            } else {
                if ((len == 1)) {
                    return input[0];
                } else {
                    return new Saxon.Hej.value.SequenceExtent.Of<T>(input);
                }
            }
        }
        public abstract Saxon.Hej.om.SequenceIterator reverseIterate();
        public class Of<T>  : Saxon.Hej.value.SequenceExtent, System.Collections.Generic.IEnumerable<T> where T : Saxon.Hej.om.Item {
            private System.Collections.Generic.IList<T> items;
            public Of(System.Collections.Generic.IList<T> list)  {
                this.items = list;
            }
            public Of(T[] items) : this (Saxon.Ejava.util.Arrays.asList(items)) {
            }
            public Of(Saxon.Hej.value.SequenceExtent.Of<T> ext, int start, int length)  {
                items = Saxon.Impl.Helpers.ListUtils.SubList(ext.items, start, (start+length));
            }
            public override  /* REPLACING public override */ Saxon.Hej.tree.iter.ListIterator.Of<T> iterate() {
                return new Saxon.Hej.tree.iter.ListIterator.Of<T>(items);
            }
            public override Saxon.Hej.om.SequenceIterator reverseIterate() {
                return Saxon.Hej.functions.Reverse.reverseIterator(items);
            }
            public override bool effectiveBooleanValue() {
                int len = getLength();
                if ((len == 0)) {
                    return false;
                } else {
                    Saxon.Hej.om.Item first = items[0];
                    if ((first is Saxon.Hej.om.NodeInfo)) {
                        return true;
                    } else {
                        if (((len == 1) && (first is Saxon.Hej.value.AtomicValue))) {
                            return first.effectiveBooleanValue();
                        } else {
                            return Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(iterate());
                        }
                    }
                }
            }
            public override  /* REPLACING public override */ Saxon.Hej.om.Item itemAt(int n) {
                if (((n>=0) && (n<items.Count))) {
                    return items[n];
                } else {
                    return null;
                }
            }
            public override  /* REPLACING public override */ Saxon.Hej.om.Item head() {
                if (items.Count==0) {
                    return null;
                } else {
                    return items[0];
                }
            }
            public override  /* REPLACING public override */ int getLength() {
                return items.Count;
            }
            public override  /* REPLACING public override */ Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
                switch (getLength()) {
                  case 0:
                    return Saxon.Hej.str.EmptyUnicodeString.getInstance();
                  case 1:
                    return this.head().getUnicodeStringValue();
                  default:
                    Saxon.Hej.str.UnicodeBuilder builder = new Saxon.Hej.str.UnicodeBuilder();
                    Saxon.Hej.str.UnicodeString separator = Saxon.Hej.str.EmptyUnicodeString.getInstance();
                    foreach (T item in items) {
                        builder.append(separator);
                        separator = Saxon.Hej.str.StringConstants.SINGLE_SPACE;
                        builder.append(item.getStringValue());
                    }
                    return builder.toUnicodeString();
                }
            }
            public override  /* REPLACING public override */ string getStringValue() {
                switch (getLength()) {
                  case 0:
                    return "";
                  case 1:
                    return this.head().getStringValue();
                  default:
                    Saxon.Ejava.lang.StringBuilder builder = new Saxon.Ejava.lang.StringBuilder();
                    string separator = "";
                    foreach (T item in items) {
                        builder.append(separator);
                        separator = " ";
                        builder.append(item.getStringValue());
                    }
                    return builder.ToString();
                }
            }
            public override  /* REPLACING public override */ Saxon.Hej.om.GroundedValue subsequence(int start, int length) {
                if ((start<0)) {
                    start = 0;
                }
                int size = getLength();
                if ((start>size)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                int limit = (((((long)(start)+(long)(length))>size)) ? size : (start+length));
                return new Saxon.Hej.value.SequenceExtent.Of<T>(Saxon.Impl.Helpers.ListUtils.SubList(items, start, limit)).reduce();
            }
            public override  /* REPLACING public override */ string ToString() {
                Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
                for (int i = 0; (i<getLength()); i++) {
                    fsb.append(((i == 0) ? "(" : ", "));
                    fsb.append(items[i].ToString());
                }
                fsb.append(')');
                return fsb.ToString();
            }
            public override  /* REPLACING public override */ Saxon.Hej.om.GroundedValue reduce() {
                int len = getLength();
                if ((len == 0)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                } else {
                    if ((len == 1)) {
                        return this.itemAt(0);
                    } else {
                        return this;
                    }
                }
            }
            public  /* REPLACING public */ System.Collections.Generic.IEnumerator<T> GetEnumerator() {
                return items.GetEnumerator();
            }
            // Extra method to make the class IEnumerable as well as IEnumerable<T>
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        
        }
    
        /* Expanding default method effectiveBooleanValue from interface GroundedValue */
        public virtual bool effectiveBooleanValue() {
            return Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(iterate());
        }

        /* Expanding default method reduce from interface GroundedValue */
        public virtual Saxon.Hej.om.GroundedValue reduce() {
            return this;
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

        /* Creating stub method iterate to satisfy interface GroundedValue */
        /* (Also inherited from Sequence) */
        public virtual Saxon.Hej.om.SequenceIterator iterate() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method itemAt to satisfy interface GroundedValue */
        public virtual Saxon.Hej.om.Item itemAt(int arg1) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method head to satisfy interface GroundedValue */
        /* (Also inherited from Sequence) */
        public virtual Saxon.Hej.om.Item head() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method subsequence to satisfy interface GroundedValue */
        public virtual Saxon.Hej.om.GroundedValue subsequence(int arg1, int arg2) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getLength to satisfy interface GroundedValue */
        public virtual int getLength() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getUnicodeStringValue to satisfy interface GroundedValue */
        public virtual Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getStringValue to satisfy interface GroundedValue */
        public virtual string getStringValue() {
            throw new System.NotImplementedException();
        }

    }
}