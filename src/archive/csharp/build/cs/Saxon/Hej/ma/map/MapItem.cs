using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.map {
    public abstract class MapItem : Saxon.Hej.om.FunctionItem {
        abstract public Saxon.Hej.om.GroundedValue get(Saxon.Hej.value.AtomicValue key);
        abstract public int size();
        public virtual bool isEmpty() {
            return (size() == 0);
        }
        abstract public Saxon.Hej.tree.iter.AtomicIterator keys();
        abstract public System.Collections.Generic.IEnumerable<Saxon.Hej.ma.map.KeyValuePair> keyValuePairs();
        public Saxon.Hej.om.SequenceIterator entries() {
            return new Saxon.Hej.tree.iter.SequenceIteratorOverJavaIterator<Saxon.Hej.ma.map.KeyValuePair>(keyValuePairs().GetEnumerator(), entry => new Saxon.Hej.ma.map.SingleEntryMap(entry.key, entry.value));
        }
        abstract public Saxon.Hej.ma.map.MapItem addEntry(Saxon.Hej.value.AtomicValue key, Saxon.Hej.om.GroundedValue value);
        abstract public Saxon.Hej.ma.map.MapItem remove(Saxon.Hej.value.AtomicValue key);
        abstract public bool conforms(Saxon.Hej.type.AtomicType keyType, Saxon.Hej.value.SequenceType valueType, Saxon.Hej.type.TypeHierarchy th);
        abstract public Saxon.Hej.type.ItemType getItemType(Saxon.Hej.type.TypeHierarchy th);
        abstract public Saxon.Hej.type.UType getKeyUType();
        public string toShortString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
            sb.append("map{");
            int count = size();
            if ((count == 0)) {
                sb.append("}");
            } else {
                if ((count<=5)) {
                    int pos = 0;
                    foreach (Saxon.Hej.ma.map.KeyValuePair pair in keyValuePairs()) {
                        if ((pos++>0)) {
                            sb.append(",");
                        }
                        sb.append(Saxon.Hej.trans.Err.depict(pair.key)).append(":").append(Saxon.Hej.trans.Err.depictSequence(pair.value));
                    }
                    sb.append("}");
                } else {
                    sb.append("(:size ").append(count).append(":)}");
                }
            }
            return sb.ToString();
        }
        public Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.MAP;
        }
        public bool isArray() {
            return false;
        }
        public bool isMap() {
            return true;
        }
        public Saxon.Hej.query.AnnotationList getAnnotations() {
            return Saxon.Hej.query.AnnotationList.EMPTY;
        }
        public Saxon.Hej.om.AtomicSequence atomize() {
            throw new Saxon.Hej.trans.XPathException((("Cannot atomize a map ("+toShortString())+")"), "FOTY0013");
        }
        public static bool isKnownToConform(Saxon.Hej.om.Sequence value, Saxon.Hej.type.ItemType itemType) {
            if ((itemType == Saxon.Hej.type.AnyItemType.getInstance())) {
                return true;
            }
            try {
                Saxon.Hej.om.SequenceIterator iter = value.iterate();
                for (Saxon.Hej.om.Item item; ((item = iter.next()) != null); ) {
                    if ((item is Saxon.Hej.value.AtomicValue)) {
                        if ((itemType is Saxon.Hej.type.AtomicType)) {
                            if (!(Saxon.Hej.type.Type.isSubType(((Saxon.Hej.value.AtomicValue)(item)).getItemType(), (Saxon.Hej.type.AtomicType)(itemType)))) {
                                return false;
                            }
                        } else {
                            return false;
                        }
                    } else {
                        if ((item is Saxon.Hej.om.NodeInfo)) {
                            if ((itemType is Saxon.Hej.pattern.NodeTest)) {
                                if (!(((Saxon.Hej.pattern.NodeTest)(itemType)).test((Saxon.Hej.om.NodeInfo)(item)))) {
                                    return false;
                                }
                            } else {
                                return false;
                            }
                        } else {
                            return false;
                        }
                    }
                }
                return true;
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                return false;
            }
        }
        public static Saxon.Hej.type.ItemType getItemTypeOfSequence(Saxon.Hej.om.Sequence val) {
            try {
                Saxon.Hej.om.Item first = val.head();
                if ((first == null)) {
                    return Saxon.Hej.type.AnyItemType.getInstance();
                } else {
                    Saxon.Hej.type.ItemType type;
                    if ((first is Saxon.Hej.value.AtomicValue)) {
                        type = ((Saxon.Hej.value.AtomicValue)(first)).getItemType();
                    } else {
                        if ((first is Saxon.Hej.om.NodeInfo)) {
                            type = Saxon.Hej.pattern.NodeKindTest.makeNodeKindTest(((Saxon.Hej.om.NodeInfo)(first)).getNodeKind());
                        } else {
                            type = Saxon.Hej.type.AnyFunctionType.getInstance();
                        }
                    }
                    if (isKnownToConform(val, type)) {
                        return type;
                    } else {
                        return Saxon.Hej.type.AnyItemType.getInstance();
                    }
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                return Saxon.Hej.type.AnyItemType.getInstance();
            }
        }
        public Saxon.Hej.expr.OperandRole[] getOperandRoles() {
            return new Saxon.Hej.expr.OperandRole[]{Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC};
        }
        public virtual Saxon.Hej.type.FunctionItemType getFunctionItemType() {
            return Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE;
        }
        public Saxon.Hej.om.StructuredQName getFunctionName() {
            return null;
        }
        public virtual string getDescription() {
            return "map";
        }
        public int getArity() {
            return 1;
        }
        public Saxon.Hej.expr.XPathContext makeNewContext(Saxon.Hej.expr.XPathContext callingContext, Saxon.Hej.expr.ContextOriginator originator) {
            return callingContext;
        }
        public Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
            Saxon.Hej.value.AtomicValue key = (Saxon.Hej.value.AtomicValue)(args[0].head());
            Saxon.Hej.om.Sequence value = get(key);
            if ((value == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            } else {
                return value;
            }
        }
        public Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            throw new Saxon.Hej.trans.UncheckedXPathException(new Saxon.Hej.trans.XPathException("A map has no string value", "FOTY0014"));
        }
        public Saxon.Hej.om.SequenceIterator getTypedValue() {
            throw new Saxon.Hej.trans.XPathException("A map has no typed value");
        }
        public virtual bool deepEquals(Saxon.Hej.om.FunctionItem other, Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.sort.AtomicComparer comparer, int flags) {
            if (((other is Saxon.Hej.ma.map.MapItem) && (((Saxon.Hej.ma.map.MapItem)(other)).size() == size()))) {
                Saxon.Hej.tree.iter.AtomicIterator keyIter = keys();
                Saxon.Hej.value.AtomicValue key;
                while (((key = ((/*1430*/Saxon.Hej.value.AtomicValue)keyIter.next())) != null)) {
                    Saxon.Hej.om.Sequence thisValue = get(key);
                    System.Diagnostics.Debug.Assert((thisValue != null));
                    Saxon.Hej.om.Sequence otherValue = ((Saxon.Hej.ma.map.MapItem)(other)).get(key);
                    if ((otherValue == null)) {
                        return false;
                    }
                    if (!(Saxon.Hej.functions.DeepEqual.deepEqual(otherValue.iterate(), thisValue.iterate(), comparer, context, flags))) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        public Saxon.Hej.om.Item itemAt(int n) {
            return ((n == 0) ? this : null);
        }
        public bool effectiveBooleanValue() {
            throw new Saxon.Hej.trans.XPathException("A map item has no effective boolean value");
        }
        public static string mapToString(Saxon.Hej.ma.map.MapItem map) {
            Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(256);
            buffer.append("map{");
            foreach (Saxon.Hej.ma.map.KeyValuePair pair in map.keyValuePairs()) {
                if ((buffer.length()>4)) {
                    buffer.append(",");
                }
                buffer.append(pair.key.ToString());
                buffer.append(":");
                buffer.append(pair.value.ToString());
            }
            buffer.append("}");
            return buffer.ToString();
        }
        public virtual void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("map");
            @out.emitAttribute("size", (""+size()));
            foreach (Saxon.Hej.ma.map.KeyValuePair kvp in keyValuePairs()) {
                Saxon.Hej.expr.Literal.exportAtomicValue(kvp.key, @out);
                Saxon.Hej.expr.Literal.exportValue(kvp.value, @out);
            }
            @out.endElement();
        }
        public virtual bool isTrustedResultType() {
            return true;
        }
    
        /* Expanding default method isSequenceVariadic from interface FunctionItem */
        public virtual bool isSequenceVariadic() {
            return false;
        }

        /* Expanding default method reduce from interface Item */
        public virtual Saxon.Hej.om.GroundedValue reduce() {
            return this;
        }

        /* Expanding default method materialize from interface Sequence */
        public virtual Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.om.SequenceTool.toGroundedValue(iterate());
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

        /* Expanding default method head from interface Item */
        public virtual Saxon.Hej.om.Item head() {
            return this;
        }

        /* Expanding default method getStringValue from interface Item */
        public virtual string getStringValue() {
            return getUnicodeStringValue().ToString();
        }

        /* Expanding default method subsequence from interface Item */
        public virtual Saxon.Hej.om.GroundedValue subsequence(int start, int length) {
            if (((start<=0) && (((start+length))>0))) {
                return this;
            } else {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        }

        /* Expanding default method getLength from interface Item */
        public virtual int getLength() {
            return 1;
        }

        /* Expanding default method iterate from interface Item */
        public virtual Saxon.Hej.om.SequenceIterator iterate() {
            return (Saxon.Hej.tree.iter.SingletonIterator)(Saxon.Hej.tree.iter.SingletonIterator.makeIterator(this));
        }

        /* Expanding default method isStreamed from interface Item */
        public virtual bool isStreamed() {
            return false;
        }

    }
}