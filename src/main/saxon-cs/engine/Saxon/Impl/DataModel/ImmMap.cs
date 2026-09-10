using System;
using System.Collections.Generic;
using SCI = System.Collections.Immutable;
using Saxon.Hej.expr.sort;
using Saxon.Hej.ma.map;
using Saxon.Hej.om;
using Saxon.Hej.tree.iter;
using Saxon.Hej.type;
using Saxon.Hej.value;
using SaxonKVP = Saxon.Hej.ma.map.KeyValuePair;
using Type = Saxon.Hej.type.Type;

#nullable enable

namespace Saxon.Impl.DataModel {
    /**
     * This is a re-implementation of the Java class Saxon.Hej.ma.map.HashTrieMap,
     * rewritten to use the .NET ImmutableDictionary class in place of the open source
     * immutable trie structure used in the Java product.
     */
    public class ImmMap : MapItem {
        // The underlying trie holds key-value pairs, but these do not correspond directly
        // to the key value pairs in the XDM map. Instead, the key in the trie is an AtomicMatchKey
        // based on the XDM key, which allows equality matching to differ from the Java-level
        // equals method (to take account of collations, etc). The value in the trie is
        // actually a tuple holding both the real key, and the value.

        protected SCI.ImmutableDictionary<AtomicMatchKey, Saxon.Hej.ma.map.KeyValuePair> imap;

        
        
        protected class GeneralMapBuilder : Saxon.Hej.ma.map.MapBuilder {
            protected SCI.ImmutableDictionary<Saxon.Hej.expr.sort.AtomicMatchKey,Saxon.Hej.ma.map.KeyValuePair>? imap 
                = SCI.ImmutableDictionary.Create<AtomicMatchKey, SaxonKVP>();

            public virtual void put(Saxon.Hej.value.AtomicValue key, Saxon.Hej.om.GroundedValue value) {
                imap = imap.Add(key.asMapKey(), new SaxonKVP(key, value));
            }
            public virtual void put(string key, Saxon.Hej.om.GroundedValue value) {
                put(new Saxon.Hej.value.StringValue(key), value);
            }

            protected virtual void put(SaxonKVP kvp) {
                imap = imap.Add(kvp.key.asMapKey(), kvp);
            }
            
            public virtual bool contains(string key) {
                return (get(key) != null);
            }
            public virtual Saxon.Hej.om.GroundedValue get(string key) {
                return get(new Saxon.Hej.value.StringValue(key));
            }
            public virtual Saxon.Hej.om.GroundedValue get(Saxon.Hej.value.AtomicValue key) {
                AtomicMatchKey matchKey = key.asMapKey();
                if (imap.ContainsKey(matchKey)) {
                    return imap[matchKey].value;
                }
                return null;
            }
            public virtual Saxon.Hej.ma.map.MapItem getCompletedMap() {
                Saxon.Impl.DataModel.ImmMap result = new Saxon.Impl.DataModel.ImmMap(imap);
                imap = null;
                return result;
            }
        
        }
        public static Saxon.Hej.ma.map.MapBuilder getBuilder() {
            return new GeneralMapBuilder();
        }

        /**
         * Create an empty map
         */
        public ImmMap() {
            this.imap = SCI.ImmutableDictionary.Create<AtomicMatchKey, Saxon.Hej.ma.map.KeyValuePair>();
        }
        

        /**
         * Create a map whose contents are a copy of an existing immutable map
         * @param imap the map to be copied
         */
        public ImmMap(SCI.ImmutableDictionary<AtomicMatchKey, SaxonKVP> imap) {
            this.imap = imap;
        }

        /**
         * Create a map whose entries are copies of the entries in an existing MapItem
         *
         * @param map       the existing map to be copied
         * @return the new map
         */
        public static ImmMap copy(MapItem map) {
            if (map is ImmMap) {
                return (ImmMap) map;
            }

            ImmMap m2 = new ImmMap();
            foreach (SaxonKVP pair in map.keyValuePairs()) {
                m2 = (ImmMap) m2.put(pair.key, pair.value);
            }

            return m2;
        }
        
        /**
         * Get the size of the map
         */
        public override int size() {
            return imap.Count;
        }

        /**
         * Ask whether the map is empty
         *
         * @return true if and only if the size of the map is zero
         */
        public override bool isEmpty() {
            return imap.IsEmpty;
        }

        
        /**
         * Get the lowest common item type of the keys in the map
         *
         * @return the most specific type to which all the keys belong. If the map is
         *         empty, return UType.VOID (the type with no instances)
         */
        public override UType getKeyUType() {
            return null;
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
        public override MapItem put(AtomicValue key, GroundedValue value) {
            AtomicMatchKey amk = makeKey(key);

            SCI.ImmutableDictionary<AtomicMatchKey, SaxonKVP> imap2 =
                imap.SetItem(amk, new SaxonKVP(key, value));
            ImmMap t2 = new(imap2);
            return t2;
        }

        /**
         * Add a new entry to this map. Since the map is supposed to be immutable, this method
         * must only be called while initially populating the map, and must not be called if
         * anyone else might already be using the map.
         *
         * @param key     the key of the new entry. Any existing entry with this key is replaced.
         * @param value   the value associated with the new entry
         * @return true if an existing entry with the same key was replaced
         */
        public bool initialPut(AtomicValue key, GroundedValue value) {
            AtomicMatchKey amk = makeKey(key);
            bool empty = isEmpty();
            bool exists = imap.ContainsKey(amk);
            imap = imap.SetItem(amk, new SaxonKVP(key, value));
            return exists;
        }

        private AtomicMatchKey makeKey(AtomicValue key) {
            return key.asMapKey();
        }


        /**
         * Remove an entry from the map
         *
         * @param key     the key of the entry to be removed
         * @return a new map in which the requested entry has been removed; or this map
         *         unchanged if the specified key was not present
         */
        public override MapItem remove(AtomicValue key) {
            SCI.ImmutableDictionary<AtomicMatchKey, SaxonKVP> m2
                = imap.Remove(makeKey(key));
            if (m2 == imap) {
                // The key is not present; the map is unchanged
                return this;
            }

            ImmMap result = new (m2);
            return result;
        }

        /**
         * Get an entry from the Map
         *
         * @param key     the value of the key
         * @return the value associated with the given key, or null if the key is not present in the map
         */
        public override GroundedValue get(AtomicValue key) {
            AtomicMatchKey matchKey = makeKey(key);
            if (imap.TryGetValue(matchKey, out SaxonKVP? value)) {
                return value.value;
            }

            return null;
        }

        /**
        * Get an key/value pair from the Map
        *
        * @param key     the value of the key
        * @return the key-value-pair associated with the given key, or null if the key is not present in the map
        */
        public Saxon.Hej.ma.map.KeyValuePair getKeyValuePair(AtomicValue key) {
            AtomicMatchKey matchKey = makeKey(key);
            if (imap.ContainsKey(matchKey)) {
                return imap[matchKey];
            }

            return null;
        }

        /**
        * Get the set of all key values in the map
        * @return an iterator over the keys, in undefined order
        */
        public override AtomicIterator keys() {
            return new KeyIteratorImpl(imap);
        }

        public override IEnumerable<SaxonKVP> keyValuePairs() {
            return imap.Values;
        }

        private class KeyIteratorImpl : AtomicIterator {
            private readonly IEnumerator<Saxon.Hej.ma.map.KeyValuePair> _enumerator;

            public KeyIteratorImpl(SCI.ImmutableDictionary<AtomicMatchKey, SaxonKVP> dictionary) {
                _enumerator = dictionary.Values.GetEnumerator();
            }

            public Item next() {
                bool more = _enumerator.MoveNext();
                if (more) {
                    return _enumerator.Current.key;
                }
                else {
                    return null;
                }
            }

            public void close() {
                // no action
            }

            public void discharge() {
                // no action
            }
            
        }

        /**
         * Get the set of all key-value pairs in the map
         *
         * @return an iterator over the key-value pairs
         */
        public override string ToString() {
            return MapItem.mapToString(this);
        }
    }
}

// Copyright (c) 2021 - 2024 Saxonica Limited