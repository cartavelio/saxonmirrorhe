using System.Collections.Immutable;
using System.Collections.Generic;
using Saxon.Hej.expr.sort;
using Saxon.Hej.ma.map;
using Saxon.Hej.om;
using Saxon.Hej.tree.iter;
using Saxon.Hej.value;
using SaxonKVP = Saxon.Hej.ma.map.KeyValuePair;

#nullable enable

namespace Saxon.Impl.DataModel {
    /// <summary>
    /// This class implements an ordered XDM map. Unfortunately .NET lacks an immutable
    /// ordered map implementation, so we improvise using an ImmutableDictionary for the
    /// lookup, combined with an ImmutableList of KeyValuePairs for the ordering.
    /// </summary>
    public class ImmOrderedMap : ImmMap {
        
        // This class extends the unordered ImmMap by adding a two-way chain through the map entries
        // to represent the key/entry ordering. Whereas the "value" part of the base map is a key-value
        // pair, this class extends the entries to hold next-key and prior-key fields. These fields are
        // null in the last and first entries, respectively.

        private class ExtendedKeyValuePair : SaxonKVP {
            public readonly AtomicMatchKey? nextKey;
            public readonly AtomicMatchKey? priorKey;

            public ExtendedKeyValuePair(AtomicValue key, GroundedValue value, 
                    AtomicMatchKey? nextKey,
                    AtomicMatchKey? priorKey) : base(key, value) {
                this.nextKey = nextKey;
                this.priorKey = priorKey;
            }
        }
        
        // Pointers to the first and last key within the map, in key order. Both are null if the map is empty.
        
        private AtomicMatchKey? firstKey;
        private AtomicMatchKey? lastKey;
        
        protected class OrderedMapBuilder : ImmMap.GeneralMapBuilder {
            private AtomicMatchKey? firstKey = null;
            private AtomicMatchKey? lastKey = null;
            
            public override void put(Saxon.Hej.value.AtomicValue key, Saxon.Hej.om.GroundedValue value) {
                AtomicMatchKey k = key.asMapKey();
                if (imap.IsEmpty) {
                    ExtendedKeyValuePair quad = new (key, value, null, lastKey);
                    imap = imap.SetItem(k, quad);
                    firstKey = k;
                    lastKey = k;
                    return;
                }
                ExtendedKeyValuePair existing = (ExtendedKeyValuePair)imap[k];
                if (existing == null) {
                    // create a new entry
                    ExtendedKeyValuePair quad = new (key, value, null, lastKey);
                    imap = imap.SetItem(k, quad);
                    // update the last existing entry to point to it
                    ExtendedKeyValuePair lastQuad = (ExtendedKeyValuePair)imap[lastKey];
                    ExtendedKeyValuePair replacement =
                        new (lastQuad.key, lastQuad.value, k, lastQuad.priorKey);
                    imap = imap.SetItem(lastKey, replacement);
                    lastKey = k;
                }
                else {
                    // replace the existing entry
                    ExtendedKeyValuePair quad = new (key, value, existing.nextKey, existing.priorKey);
                    imap = imap.SetItem(k, quad);
                }
            }
            
            public override Saxon.Hej.om.GroundedValue get(Saxon.Hej.value.AtomicValue key) {
                SaxonKVP kvp = imap[key.asMapKey()];
                return kvp == null ? null : kvp.value;
            }
            
            public override Saxon.Hej.ma.map.MapItem getCompletedMap() {
                Saxon.Impl.DataModel.ImmOrderedMap result = new (imap);
                imap = null;
                return result;
            }
        
        }
        public new static Saxon.Hej.ma.map.MapBuilder getBuilder() {
            return new OrderedMapBuilder();
        }

        /**
         * Create an empty map
         */
        public ImmOrderedMap() {

        }

        public ImmOrderedMap(ImmutableDictionary<AtomicMatchKey, SaxonKVP> imap) :
            base(imap) {
        }
        
        /**
         * Create a new map containing the existing entries in the map plus an additional entry,
         * without modifying the original. If there is already an entry with the specified key,
         * this entry is replaced by the new entry.
         *
         * @param key     the key of the new entry
         * @param value   the value associated with the new entry
         * @return the new map containing the additional entry
         */
        public override MapItem addEntry(AtomicValue key, GroundedValue value) {
            AtomicMatchKey k = key.asMapKey();
            ImmutableDictionary<AtomicMatchKey, SaxonKVP> imap2;
            ImmOrderedMap t2;
            if (imap.IsEmpty) {
                ExtendedKeyValuePair quad = new (key, value, null, lastKey);
                imap2 = imap.SetItem(k, quad);
                t2 = new (imap2);
                t2.copyTypeInfoFrom(this);
                t2.updateTypeInformation(key, value, isEmpty());
                t2.firstKey = k;
                t2.lastKey = k;
                return t2;
            }
            else {
                if (!imap.TryGetValue(k, out SaxonKVP? existing)) {
                    // key not present, create a new entry at the end
                    ExtendedKeyValuePair quad = new (key, value, null, lastKey);
                    imap2 = imap.SetItem(k, quad);
                    // update the last existing entry to point to it
                    ExtendedKeyValuePair lastQuad = (ExtendedKeyValuePair)imap[lastKey];
                    ExtendedKeyValuePair replacement =
                        new(lastQuad.key, lastQuad.value, k, lastQuad.priorKey);
                    imap2 = imap2.SetItem(lastKey, replacement);
                    t2 = new (imap2);
                    t2.copyTypeInfoFrom(this);
                    t2.updateTypeInformation(key, value, isEmpty());
                    t2.firstKey = firstKey;
                    t2.lastKey = k;
                    return t2;
                }
                else {
                    // replace the existing entry
                    ExtendedKeyValuePair quad0 = (ExtendedKeyValuePair)existing;
                    ExtendedKeyValuePair quad1 = new (key, value, quad0.nextKey, quad0.priorKey);
                    imap2 = imap.SetItem(k, quad1);
                    t2 = new (imap2);
                    t2.copyTypeInfoFrom(this);
                    t2.updateTypeInformation(key, value, isEmpty());
                    t2.firstKey = firstKey;
                    t2.lastKey = lastKey;
                    return t2;
                }
            }

            
        }

        /**
         * Remove an entry from the map
         *
         * @param key     the key of the entry to be removed
         * @return a new map in which the requested entry has been removed; or this map
         *         unchanged if the specified key was not present
         */
        public override MapItem remove(AtomicValue key) {

            AtomicMatchKey amk = key.asMapKey();
            ExtendedKeyValuePair quad = (ExtendedKeyValuePair)imap[amk];
            if (quad == null) {
                // key not present, return map unchanged
                return this;
            }

            if (quad.priorKey == null) {
                // we're removing the first entry
                firstKey = quad.nextKey;
            }
            else {
                // update the forwards pointer from the previous entry
                ExtendedKeyValuePair prev = (ExtendedKeyValuePair)imap[quad.priorKey];
                ExtendedKeyValuePair prev2 = new(prev.key, prev.value, quad.nextKey, prev.priorKey);
                imap = imap.SetItem(quad.priorKey, prev2);
            }
            
            if (quad.nextKey == null) {
                // we're removing the last entry
                lastKey = quad.priorKey;
            }
            else {
                // update the backwards pointer from the following entry
                ExtendedKeyValuePair foll = (ExtendedKeyValuePair)imap[quad.nextKey];
                ExtendedKeyValuePair foll2 = new(foll.key, foll.value, foll.nextKey, quad.priorKey);
                imap = imap.SetItem(quad.nextKey, foll2);
            }
            
            // Now drop the actual entry
            imap = imap.Remove(amk);

            ImmOrderedMap result = new ImmOrderedMap(imap);
            result.copyTypeInfoFrom(this);
            return result;
        }
        
        /**
        * Get the set of all key values in the map
        * @return an iterator over the keys, in order
        */
        public override AtomicIterator keys() {
            return firstKey == null ? EmptyIterator.ofAtomic() : new OrderedKeyIterator(imap, firstKey);
        }

        public override System.Collections.Generic.IEnumerable<SaxonKVP> keyValuePairs() {
            // TODO: avoid materializing
            List<SaxonKVP> list = new List<SaxonKVP>();
            AtomicMatchKey? k = firstKey;
            while (k != null) {
                SaxonKVP kvp = imap[k];
                list.Add(kvp);
                k = ((ExtendedKeyValuePair)kvp).nextKey;
            }

            return list;
        }
        
        

        public class OrderedKeyIterator : AtomicIterator {
            private readonly ImmutableDictionary<AtomicMatchKey, SaxonKVP> imap;
            private AtomicMatchKey? nextMatchKey;

            public OrderedKeyIterator(ImmutableDictionary<AtomicMatchKey, SaxonKVP> imap, AtomicMatchKey nextMatchKey) {
                this.imap = imap;
                this.nextMatchKey = nextMatchKey;
            }

            public Item next() {
                if (nextMatchKey == null) {
                    return null;
                }

                ExtendedKeyValuePair quad = (ExtendedKeyValuePair) imap[nextMatchKey];
                nextMatchKey = quad.nextKey;
                return quad.key;
            }

            public void close() {
                // no action
            }

            public void discharge() {
                // no action
            }
            
        }
        
    }
}

// Copyright (c) 2024 Saxonica Limited