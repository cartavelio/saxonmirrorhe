using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.map {
    public class RangeKey : Saxon.Hej.ma.map.MapItem {
        private readonly Saxon.Hej.str.UnicodeString min;
        private readonly Saxon.Hej.str.UnicodeString max;
        private readonly System.Collections.Generic.SortedDictionary<Saxon.Hej.expr.sort.AtomicMatchKey,System.Object> index;
        public RangeKey(Saxon.Hej.str.UnicodeString min, Saxon.Hej.str.UnicodeString max, System.Collections.Generic.SortedDictionary<Saxon.Hej.expr.sort.AtomicMatchKey,System.Object> index)  {
            this.min = min;
            this.max = max;
            this.index = index;
        }
        public override Saxon.Hej.om.GroundedValue get(Saxon.Hej.value.AtomicValue key) {
            Saxon.Hej.str.UnicodeString k = key.getUnicodeStringValue();
            if (((((min == null) || (min.compareTo(k)<=0))) && (((max == null) || (max.compareTo(k)>=0))))) {
                System.Object value = Saxon.Impl.Helpers.MapUtils.Get(index, k);
                if ((value == null)) {
                    return null;
                } else {
                    if ((value is Saxon.Hej.om.NodeInfo)) {
                        return ((Saxon.Hej.om.NodeInfo)(value));
                    } else {
                        System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> nodes = (System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo>)(value);
                        return (nodes.Count==0 ? null : new Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.om.NodeInfo>(nodes));
                    }
                }
            }
            return null;
        }
        public override int size() {
            try {
                return Saxon.Hej.functions.Count.count(keys());
            } catch (Saxon.Hej.trans.XPathException err) {
                return 0;
            }
        }
        public override bool isEmpty() {
            return (((/*1430*/Saxon.Hej.value.AtomicValue)keys().next()) == null);
        }
        public override Saxon.Hej.tree.iter.AtomicIterator keys() {
            return new Saxon.Hej.ma.map.RangeKey.RangeKeyIterator(min, max, index);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.ma.map.KeyValuePair> keyValuePairs() {
            System.Collections.Generic.IList<Saxon.Hej.ma.map.KeyValuePair> kvpList = new System.Collections.Generic.List<Saxon.Hej.ma.map.KeyValuePair>();
            Saxon.Hej.tree.iter.AtomicIterator keyIter = keys();
            Saxon.Hej.value.AtomicValue key;
            while (((key = ((/*1430*/Saxon.Hej.value.AtomicValue)keyIter.next())) != null)) {
                kvpList.Add(new Saxon.Hej.ma.map.KeyValuePair(key, get(key)));
            }
            return kvpList;
        }
        public override Saxon.Hej.ma.map.MapItem remove(Saxon.Hej.value.AtomicValue key) {
            return Saxon.Impl.DataModel.ImmMap.copy(this).remove(key);
        }
        public override Saxon.Hej.type.UType getKeyUType() {
            return Saxon.Hej.type.UType.STRING;
        }
        public override Saxon.Hej.ma.map.MapItem addEntry(Saxon.Hej.value.AtomicValue key, Saxon.Hej.om.GroundedValue value) {
            return Saxon.Impl.DataModel.ImmMap.copy(this).addEntry(key, value);
        }
        public override bool conforms(Saxon.Hej.type.AtomicType keyType, Saxon.Hej.value.SequenceType valueType, Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Hej.tree.iter.AtomicIterator keyIter = keys();
            Saxon.Hej.value.AtomicValue key;
            while (((key = ((/*1430*/Saxon.Hej.value.AtomicValue)keyIter.next())) != null)) {
                Saxon.Hej.om.GroundedValue value = get(key);
                if (!(valueType.matches(value, th))) {
                    return false;
                }
            }
            return true;
        }
        public override Saxon.Hej.ma.map.MapType getItemType(Saxon.Hej.type.TypeHierarchy th) {
            return new Saxon.Hej.ma.map.MapType(Saxon.Hej.type.BuiltInAtomicType.STRING, Saxon.Hej.value.SequenceType.NODE_SEQUENCE);
        }
        public override Saxon.Hej.ma.map.MapType getFunctionItemType() {
            return new Saxon.Hej.ma.map.MapType(Saxon.Hej.type.BuiltInAtomicType.STRING, Saxon.Hej.value.SequenceType.NODE_SEQUENCE);
        }
        public override string getDescription() {
            return "range key";
        }
        public override bool deepEquals(Saxon.Hej.om.Function other, Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.sort.AtomicComparer comparer, int flags) {
            if ((other is Saxon.Hej.ma.map.RangeKey)) {
                Saxon.Hej.ma.map.RangeKey rk = (Saxon.Hej.ma.map.RangeKey)(other);
                return ((min.Equals(rk.min) && max.Equals(rk.max)) && index.Equals(rk.index));
            } else {
                return false;
            }
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("range-key-map");
            @out.emitAttribute("size", (size()+""));
            @out.endElement();
        }
        public override bool isTrustedResultType() {
            return false;
        }
        public override string ToString() {
            return Saxon.Hej.ma.map.MapItem.mapToString(this);
        }
        private class RangeKeyIterator : Saxon.Hej.tree.iter.AtomicIterator {
            private int pos = 0;
            private Saxon.Hej.value.StringValue curr = null;
            private readonly Saxon.Hej.value.StringValue top;
            private readonly Saxon.Hej.str.UnicodeString min;
            private readonly Saxon.Hej.str.UnicodeString max;
            private readonly System.Collections.Generic.SortedDictionary<Saxon.Hej.expr.sort.AtomicMatchKey,System.Object> index;
            public RangeKeyIterator(Saxon.Hej.str.UnicodeString min, Saxon.Hej.str.UnicodeString max, System.Collections.Generic.SortedDictionary<Saxon.Hej.expr.sort.AtomicMatchKey,System.Object> index)  {
                top = new Saxon.Hej.value.StringValue((Saxon.Hej.str.UnicodeString)((((max == null) ? Saxon.Impl.Helpers.TreeMapUtils.FirstKey(index) : Saxon.Impl.Helpers.TreeMapUtils.FloorKey(index, max)))));
                this.min = min;
                this.max = max;
                this.index = index;
            }
            public Saxon.Hej.om.Item next() {
                if ((pos<=0)) {
                    if ((pos<0)) {
                        return null;
                    }
                    if ((min == null)) {
                        curr = new Saxon.Hej.value.StringValue((Saxon.Hej.str.UnicodeString)(Saxon.Impl.Helpers.TreeMapUtils.FirstKey(index)));
                    } else {
                        Saxon.Hej.str.UnicodeString c = (Saxon.Hej.str.UnicodeString)(Saxon.Impl.Helpers.TreeMapUtils.CeilingKey(index, min));
                        if (((c == null) || (((max != null) && (c.compareTo(max)>0))))) {
                            curr = null;
                        } else {
                            curr = new Saxon.Hej.value.StringValue(c);
                        }
                    }
                } else {
                    if (curr.Equals(top)) {
                        curr = null;
                    } else {
                        curr = new Saxon.Hej.value.StringValue((Saxon.Hej.str.UnicodeString)(Saxon.Impl.Helpers.TreeMapUtils.HigherKey(index, curr.getUnicodeStringValue())));
                    }
                }
                if ((curr == null)) {
                    pos = -1;
                    return null;
                } else {
                    pos++;
                    return curr;
                }
            }
        
            /* Expanding default method close from interface SequenceIterator */
            public virtual void close() {
            }

            /* Expanding default method discharge from interface SequenceIterator */
            public virtual void discharge() {
            }

        }
    
    }
}