using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.map {
    public class SingleEntryMap : Saxon.Hej.ma.map.MapItem {
        public Saxon.Hej.value.AtomicValue key;
        public Saxon.Hej.om.GroundedValue value;
        public SingleEntryMap(Saxon.Hej.value.AtomicValue key, Saxon.Hej.om.GroundedValue value)  {
            this.key = key;
            this.value = value;
        }
        public Saxon.Hej.value.AtomicValue getKey() {
            return key;
        }
        public Saxon.Hej.om.GroundedValue getValue() {
            return value;
        }
        public override Saxon.Hej.om.GroundedValue get(Saxon.Hej.value.AtomicValue key) {
            return (this.key.asMapKey().Equals(key.asMapKey()) ? value : null);
        }
        public override int size() {
            return 1;
        }
        public override bool isEmpty() {
            return false;
        }
        public override Saxon.Hej.tree.iter.AtomicIterator keys() {
            return new Saxon.Hej.tree.iter.SingleAtomicIterator(key);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.ma.map.KeyValuePair> keyValuePairs() {
            System.Collections.Generic.IList<Saxon.Hej.ma.map.KeyValuePair> list = new System.Collections.Generic.List<Saxon.Hej.ma.map.KeyValuePair>(1);
            list.Add(new Saxon.Hej.ma.map.KeyValuePair(key, value));
            return list;
        }
        public override Saxon.Hej.ma.map.MapItem addEntry(Saxon.Hej.value.AtomicValue key, Saxon.Hej.om.GroundedValue value) {
            return toHashTrieMap().addEntry(key, value);
        }
        public override Saxon.Hej.ma.map.MapItem remove(Saxon.Hej.value.AtomicValue key) {
            if ((get(key) == null)) {
                return this;
            } else {
                return new Saxon.Impl.DataModel.ImmMap();
            }
        }
        public override bool conforms(Saxon.Hej.type.AtomicType keyType, Saxon.Hej.value.SequenceType valueType, Saxon.Hej.type.TypeHierarchy th) {
            return (keyType.matches(key, th) && valueType.matches(value, th));
        }
        public override Saxon.Hej.type.ItemType getItemType(Saxon.Hej.type.TypeHierarchy th) {
            return new Saxon.Hej.ma.map.MapType(key.getItemType(), Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.om.SequenceTool.getItemType(value, th), Saxon.Hej.om.SequenceTool.getCardinality(value)));
        }
        public override Saxon.Hej.type.UType getKeyUType() {
            return key.getUType();
        }
        private Saxon.Impl.DataModel.ImmMap toHashTrieMap() {
            Saxon.Impl.DataModel.ImmMap target = new Saxon.Impl.DataModel.ImmMap();
            target.initialPut(key, value);
            return target;
        }
    
    }
}