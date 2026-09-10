using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.pedigree {
    public abstract class DelegatingMapItem : Saxon.Hej.ma.map.MapItem {
        private readonly Saxon.Hej.ma.map.MapItem @base;
        public DelegatingMapItem(Saxon.Hej.ma.map.MapItem @base)  {
            this.@base = @base;
        }
        public override Saxon.Hej.om.GroundedValue get(Saxon.Hej.value.AtomicValue key) {
            return @base.get(key);
        }
        public override int size() {
            return @base.size();
        }
        public override bool isEmpty() {
            return @base.isEmpty();
        }
        public override Saxon.Hej.tree.iter.AtomicIterator keys() {
            return @base.keys();
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.ma.map.KeyValuePair> keyValuePairs() {
            return @base.keyValuePairs();
        }
        public override Saxon.Hej.ma.map.MapItem addEntry(Saxon.Hej.value.AtomicValue key, Saxon.Hej.om.GroundedValue value) {
            return @base.addEntry(key, value);
        }
        public override Saxon.Hej.ma.map.MapItem remove(Saxon.Hej.value.AtomicValue key) {
            return @base.remove(key);
        }
        public override bool conforms(Saxon.Hej.type.AtomicType keyType, Saxon.Hej.value.SequenceType valueType, Saxon.Hej.type.TypeHierarchy th) {
            return @base.conforms(keyType, valueType, th);
        }
        public override Saxon.Hej.ma.map.MapType getItemType(Saxon.Hej.type.TypeHierarchy th) {
            return (Saxon.Hej.ma.map.MapType)(@base.getItemType(th));
        }
        public override Saxon.Hej.type.UType getKeyUType() {
            return @base.getKeyUType();
        }
        public Saxon.Hej.ma.map.MapItem getBaseItem() {
            return @base;
        }
        public override string ToString() {
            return @base.ToString();
        }
    
    }
}