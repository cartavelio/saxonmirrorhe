using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class SequenceTool {
        public static readonly int INDETERMINATE_ORDERING = System.Int32.MinValue;
        public static Saxon.Hej.om.GroundedValue toGroundedValue(Saxon.Hej.om.SequenceIterator iterator) {
            if (((iterator is Saxon.Hej.tree.iter.GroundedIterator) && ((Saxon.Hej.tree.iter.GroundedIterator)(iterator)).isActuallyGrounded())) {
                return ((Saxon.Hej.tree.iter.GroundedIterator)(iterator)).materialize();
            } else {
                return Saxon.Hej.value.SequenceExtent.from(iterator).reduce();
            }
        }
        public static Saxon.Hej.om.Sequence toMemoSequence(Saxon.Hej.om.SequenceIterator iterator) {
            if ((iterator is Saxon.Hej.tree.iter.EmptyIterator)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            } else {
                if (((iterator is Saxon.Hej.tree.iter.GroundedIterator) && ((Saxon.Hej.tree.iter.GroundedIterator)(iterator)).isActuallyGrounded())) {
                    return toGroundedValue(iterator);
                } else {
                    return new Saxon.Hej.om.MemoSequence(iterator);
                }
            }
        }
        public static Saxon.Hej.om.Sequence toLazySequence(Saxon.Hej.om.SequenceIterator iterator) {
            if (((((iterator is Saxon.Hej.tree.iter.GroundedIterator) && ((Saxon.Hej.tree.iter.GroundedIterator)(iterator)).isActuallyGrounded()) && !(((iterator is Saxon.Hej.expr.AscendingRangeIterator)))) && !(((iterator is Saxon.Hej.expr.DescendingRangeIterator))))) {
                return toGroundedValue(iterator);
            } else {
                return new Saxon.Hej.om.LazySequence(iterator);
            }
        }
        public static bool supportsGetLength(Saxon.Hej.om.SequenceIterator iterator) {
            return ((iterator is Saxon.Hej.expr.LastPositionFinder) && ((Saxon.Hej.expr.LastPositionFinder)(iterator)).supportsGetLength());
        }
        public static int getLength(Saxon.Hej.om.SequenceIterator iterator) {
            try {
                return ((Saxon.Hej.expr.LastPositionFinder)(iterator)).getLength();
            } catch (System.InvalidCastException e) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("getLength() not available");
            }
        }
        public static void supply(Saxon.Hej.om.SequenceIterator iter, Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item> consumer) {
            Saxon.Hej.om.Item item;
            try {
                while (((item = iter.next()) != null)) {
                    /*Delegate.accept()*/consumer(item);
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
        }
        public static bool isUnrepeatable(Saxon.Hej.om.Sequence seq) {
            return ((seq is Saxon.Hej.om.LazySequence) || (((seq is Saxon.Hej.value.Closure) && !((((seq is Saxon.Hej.value.MemoClosure) || (seq is Saxon.Hej.value.SingletonClosure)))))));
        }
        public static int getLength(Saxon.Hej.om.Sequence sequence) {
            if ((sequence is Saxon.Hej.om.GroundedValue)) {
                return ((Saxon.Hej.om.GroundedValue)(sequence)).getLength();
            }
            return Saxon.Hej.functions.Count.count(sequence.iterate());
        }
        public static bool hasLength(Saxon.Hej.om.SequenceIterator iter, int length) {
            if (Saxon.Hej.om.SequenceTool.supportsGetLength(iter)) {
                return (((Saxon.Hej.expr.LastPositionFinder)(iter)).getLength() == length);
            } else {
                int n = 0;
                while ((iter.next() != null)) {
                    if ((n++ == length)) {
                        iter.close();
                        return false;
                    }
                }
                return (length == 0);
            }
        }
        public static bool sameLength(Saxon.Hej.om.SequenceIterator a, Saxon.Hej.om.SequenceIterator b) {
            if (Saxon.Hej.om.SequenceTool.supportsGetLength(a)) {
                return hasLength(b, ((Saxon.Hej.expr.LastPositionFinder)(a)).getLength());
            } else {
                if (Saxon.Hej.om.SequenceTool.supportsGetLength(b)) {
                    return hasLength(a, ((Saxon.Hej.expr.LastPositionFinder)(b)).getLength());
                } else {
                    while (true) {
                        Saxon.Hej.om.Item itA = a.next();
                        Saxon.Hej.om.Item itB = b.next();
                        if (((itA == null) || (itB == null))) {
                            if ((itA != null)) {
                                a.close();
                            }
                            if ((itB != null)) {
                                b.close();
                            }
                            return ((itA == null) && (itB == null));
                        }
                    }
                }
            }
        }
        public static Saxon.Hej.om.Item itemAt(Saxon.Hej.om.Sequence sequence, int index) {
            if (((sequence is Saxon.Hej.om.Item) && (index == 0))) {
                return (Saxon.Hej.om.Item)(sequence);
            }
            return sequence.materialize().itemAt(index);
        }
        public static Saxon.Hej.om.Item asItem(Saxon.Hej.om.Sequence sequence) {
            if ((sequence is Saxon.Hej.om.Item)) {
                return (Saxon.Hej.om.Item)(sequence);
            }
            Saxon.Hej.om.SequenceIterator iter = sequence.iterate();
            Saxon.Hej.om.Item first = iter.next();
            if ((first == null)) {
                return null;
            }
            if ((iter.next() != null)) {
                throw new Saxon.Hej.trans.XPathException("Sequence contains more than one item");
            }
            return first;
        }
        public static Saxon.Hej.str.UnicodeString getStringValue(Saxon.Hej.om.Sequence sequence) {
            Saxon.Hej.str.UnicodeBuilder ub = new Saxon.Hej.str.UnicodeBuilder();
            supply(sequence.iterate(), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => {
                if (!(ub.isEmpty())) {
                    ub.append(' ');
                }
                ub.accept(item.getUnicodeStringValue());
            } )));
            return ub.toUnicodeString();
        }
        public static string stringify(Saxon.Hej.om.Sequence sequence) {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(64);
            supply(sequence.iterate(), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => {
                if ((sb.length() != 0)) {
                    sb.append(' ');
                }
                sb.append(item.getStringValue());
            } )));
            return sb.ToString();
        }
        public static Saxon.Hej.type.ItemType getItemType(Saxon.Hej.om.Sequence sequence, Saxon.Hej.type.TypeHierarchy th) {
            if ((sequence is Saxon.Hej.om.Item)) {
                return Saxon.Hej.type.Type.getItemType((Saxon.Hej.om.Item)(sequence), th);
            } else {
                if ((sequence is Saxon.Hej.om.GroundedValue)) {
                    try {
                        Saxon.Hej.type.ItemType type = null;
                        Saxon.Hej.om.SequenceIterator iter = sequence.iterate();
                        Saxon.Hej.om.Item item;
                        while (((item = iter.next()) != null)) {
                            if ((type == null)) {
                                type = Saxon.Hej.type.Type.getItemType(item, th);
                            } else {
                                type = Saxon.Hej.type.Type.getCommonSuperType(type, Saxon.Hej.type.Type.getItemType(item, th), th);
                            }
                            if ((type == Saxon.Hej.type.AnyItemType.getInstance())) {
                                break ;
                            }
                        }
                        return ((type == null) ? Saxon.Hej.type.ErrorType.getInstance() : type);
                    } catch (Saxon.Hej.trans.UncheckedXPathException err) {
                        return Saxon.Hej.type.AnyItemType.getInstance();
                    }
                } else {
                    return Saxon.Hej.type.AnyItemType.getInstance();
                }
            }
        }
        public static Saxon.Hej.type.UType getUType(Saxon.Hej.om.Sequence sequence) {
            if ((sequence is Saxon.Hej.om.Item)) {
                return Saxon.Hej.type.UType.getUType((Saxon.Hej.om.Item)(sequence));
            } else {
                if ((sequence is Saxon.Hej.om.GroundedValue)) {
                    Saxon.Hej.type.UType type = Saxon.Hej.type.UType.VOID;
                    Saxon.Hej.om.SequenceIterator iter = ((Saxon.Hej.om.GroundedValue)(sequence)).iterate();
                    Saxon.Hej.om.Item item;
                    while (((item = iter.next()) != null)) {
                        type = type.union(Saxon.Hej.type.UType.getUType(item));
                        if ((type == Saxon.Hej.type.UType.ANY)) {
                            break ;
                        }
                    }
                    return type;
                } else {
                    return Saxon.Hej.type.UType.ANY;
                }
            }
        }
        public static int getCardinality(Saxon.Hej.om.Sequence sequence) {
            if ((sequence is Saxon.Hej.om.Item)) {
                return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
            }
            if ((sequence is Saxon.Hej.om.GroundedValue)) {
                int len = ((Saxon.Hej.om.GroundedValue)(sequence)).getLength();
                switch (len) {
                  case 0:
                    return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO;
                  case 1:
                    return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
                  default:
                    return Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE;
                }
            }
            try {
                Saxon.Hej.om.SequenceIterator iter = sequence.iterate();
                Saxon.Hej.om.Item item = iter.next();
                if ((item == null)) {
                    return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO;
                }
                item = iter.next();
                return ((item == null) ? Saxon.Hej.expr.StaticProperty.EXACTLY_ONE : Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE);
            } catch (Saxon.Hej.trans.UncheckedXPathException err) {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE;
            }
        }
        public static void process(Saxon.Hej.om.Sequence value, Saxon.Hej.@event.Outputter output, Saxon.Hej.s9api.Location locationId) {
            supply(value.iterate(), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((it => output.append(it, locationId, Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES))));
        }
        public static Saxon.Hej.om.Sequence[] makeSequenceArray(int length) {
            return new Saxon.Hej.om.Sequence[length];
        }
        public static Saxon.Hej.om.Sequence[] fromItems(params Saxon.Hej.om.Item[]  items) {
            Saxon.Hej.om.Sequence[] seq = new Saxon.Hej.om.Sequence[items.Length];
            Saxon.Ejava.lang.System.arraycopy(items, 0, seq, 0, items.Length);
            return seq;
        }
        public static Saxon.Hej.om.AttributeMap attributeMapFromList(System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> list) {
            int n = list.Count;
            if ((n == 0)) {
                return Saxon.Hej.om.EmptyAttributeMap.getInstance();
            } else {
                if ((n == 1)) {
                    return Saxon.Hej.om.SingletonAttributeMap.of(list[0]);
                } else {
                    if ((n<=Saxon.Hej.om.SmallAttributeMap.LIMIT)) {
                        return new Saxon.Hej.om.SmallAttributeMap(list);
                    } else {
                        return new Saxon.Hej.om.LargeAttributeMap(list);
                    }
                }
            }
        }
        public static Saxon.Hej.om.GroundedValue itemOrEmpty(Saxon.Hej.om.Item item) {
            if ((item == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            } else {
                return item;
            }
        }
    
    }
}