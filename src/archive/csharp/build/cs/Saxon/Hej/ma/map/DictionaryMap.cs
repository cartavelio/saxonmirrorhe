using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.map {
    public class DictionaryMap : Saxon.Hej.ma.map.MapItem {
        private readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.om.GroundedValue> hashMap;
        public DictionaryMap()  {
            hashMap = new System.Collections.Generic.Dictionary<string, Saxon.Hej.om.GroundedValue>();
        }
        public DictionaryMap(int size)  {
            hashMap = new System.Collections.Generic.Dictionary<string, Saxon.Hej.om.GroundedValue>(size);
        }
        public void initialPut(string key, Saxon.Hej.om.GroundedValue value) {
            Saxon.Impl.Helpers.MapUtils.Put(hashMap, key, value);
        }
        public void initialAppend(string key, Saxon.Hej.om.GroundedValue value) {
            Saxon.Hej.om.GroundedValue existingValue = Saxon.Impl.Helpers.MapUtils.Get(hashMap, key);
            if ((existingValue == null)) {
                initialPut(key, value);
            } else {
                Saxon.Impl.Helpers.MapUtils.Put(hashMap, key, existingValue.concatenate(value));
            }
        }
        public override Saxon.Hej.om.GroundedValue get(Saxon.Hej.value.AtomicValue key) {
            if ((key is Saxon.Hej.value.StringValue)) {
                return Saxon.Impl.Helpers.MapUtils.Get(hashMap, key.getStringValue());
            } else {
                return null;
            }
        }
        public override int size() {
            return hashMap.Count;
        }
        public override bool isEmpty() {
            return hashMap.Count==0;
        }
        public override Saxon.Hej.tree.iter.AtomicIterator keys() {
            return new Saxon.Hej.ma.map.DictionaryMap.KeyIterator(hashMap);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.ma.map.KeyValuePair> keyValuePairs() {
            System.Collections.Generic.IList<Saxon.Hej.ma.map.KeyValuePair> pairs = new System.Collections.Generic.List<Saxon.Hej.ma.map.KeyValuePair>();
            foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.om.GroundedValue> entry in hashMap) {
                pairs.Add(new Saxon.Hej.ma.map.KeyValuePair(new Saxon.Hej.value.StringValue(entry.Key), entry.Value));
            }
            return pairs;
        }
        public override Saxon.Hej.ma.map.MapItem addEntry(Saxon.Hej.value.AtomicValue key, Saxon.Hej.om.GroundedValue value) {
            return toHashTrieMap().addEntry(key, value);
        }
        public override Saxon.Hej.ma.map.MapItem remove(Saxon.Hej.value.AtomicValue key) {
            return toHashTrieMap().remove(key);
        }
        public override bool conforms(Saxon.Hej.type.AtomicType keyType, Saxon.Hej.value.SequenceType valueType, Saxon.Hej.type.TypeHierarchy th) {
            if (isEmpty()) {
                return true;
            }
            if (!((((keyType == Saxon.Hej.type.BuiltInAtomicType.STRING) || (keyType == Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC))))) {
                return false;
            }
            if (valueType.Equals(Saxon.Hej.value.SequenceType.ANY_SEQUENCE)) {
                return true;
            }
            foreach (Saxon.Hej.om.GroundedValue val in hashMap.Values) {
                if (!(valueType.matches(val, th))) {
                    return false;
                }
            }
            return true;
        }
        public override Saxon.Hej.type.ItemType getItemType(Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Hej.type.ItemType valueType = null;
            int valueCard = 0;
            Saxon.Hej.tree.iter.AtomicIterator keyIter = keys();
            Saxon.Hej.value.AtomicValue key;
            foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.om.GroundedValue> entry in hashMap) {
                Saxon.Hej.om.GroundedValue val = entry.Value;
                if ((valueType == null)) {
                    valueType = Saxon.Hej.om.SequenceTool.getItemType(val, th);
                    valueCard = Saxon.Hej.om.SequenceTool.getCardinality(val);
                } else {
                    valueType = Saxon.Hej.type.Type.getCommonSuperType(valueType, Saxon.Hej.om.SequenceTool.getItemType(val, th), th);
                    valueCard = Saxon.Hej.value.Cardinality.union(valueCard, Saxon.Hej.om.SequenceTool.getCardinality(val));
                }
            }
            if ((valueType == null)) {
                return Saxon.Hej.ma.map.MapType.EMPTY_MAP_TYPE;
            } else {
                return new Saxon.Hej.ma.map.MapType(Saxon.Hej.type.BuiltInAtomicType.STRING, Saxon.Hej.value.SequenceType.makeSequenceType(valueType, valueCard));
            }
        }
        public override Saxon.Hej.type.UType getKeyUType() {
            return (hashMap.Count==0 ? Saxon.Hej.type.UType.VOID : Saxon.Hej.type.UType.STRING);
        }
        private Saxon.Impl.DataModel.ImmMap toHashTrieMap() {
            Saxon.Impl.DataModel.ImmMap target = new Saxon.Impl.DataModel.ImmMap();
            foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.om.GroundedValue> entry in hashMap) {
                target.initialPut(new Saxon.Hej.value.StringValue(entry.Key), entry.Value);
            }
            return target;
        }
        private class KeyIterator : Saxon.Hej.tree.iter.AtomicIterator {
            internal System.Collections.Generic.IEnumerator<string> keyIter;
            public KeyIterator(System.Collections.Generic.IDictionary<string,Saxon.Hej.om.GroundedValue> hashMap)  {
                this.keyIter = Saxon.Impl.Helpers.MapUtils.KeySet(hashMap).GetEnumerator();
            }
            public Saxon.Hej.om.Item next() {
                if (this.keyIter.MoveNext()) {
                    return new Saxon.Hej.value.StringValue(this.keyIter.Current);
                } else {
                    return null;
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