using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Saxon.Hej.expr.sort;
using Saxon.Hej.ma.map;
using Saxon.Hej.om;
using Saxon.Hej.tree.iter;
using Saxon.Hej.type;
using Saxon.Hej.value;
using KeyValuePair = Saxon.Hej.ma.map.KeyValuePair;
using Type = Saxon.Hej.type.Type;

namespace Saxon.Impl.DataModel {
    /**
     * This is a re-implementation of the Java class Saxon.Hej.ma.map.HashTrieMap,
     * rewritten to use the .NET ImmutableDictionary class in place of the homebrew
     * immutable trie structure used in the Java product.
     */
    public class ImmMap : MapItem {
        // The underlying trie holds key-value pairs, but these do not correspond directly
        // to the key value pairs in the XDM map. Instead, the key in the trie is an AtomicMatchKey
        // based on the XDM key, which allows equality matching to differ from the Java-level
        // equals method (to take account of collations, etc). The value in the trie is
        // actually a tuple holding both the real key, and the value.

        private ImmutableDictionary<AtomicMatchKey, Saxon.Hej.ma.map.KeyValuePair> imap;

        // The following values are maintained incrementally when
        // entries are added to the map. They are not changed when entries are removed,
        // so the actual type may be narrower than these values suggest. The purpose of
        // keeping this information is to try and avoid dynamic checking of the map
        // contents against a required type wherever possible. We therefore have
        // a guarantee that all entries in the map conform to the types maintained here;
        // but there is no guarantee that they do not also conform to some more specific
        // type.

        // The set of UTypes of keys in the map
        private UType keyUType = UType.VOID;

        // The set of UTypes of items in the values in the map
        private UType valueUType = UType.VOID;

        // The atomic type of all keys, if they are homogeneous; otherwise null
        private AtomicType keyAtomicType = ErrorType.getInstance();

        // The item type of all items in values, if they are homogeneous; otherwise null
        private ItemType valueItemType = ErrorType.getInstance();

        // The "envelope" cardinality of values in the map (0:1, 0:0, 1:1, 0:many, 1:many)
        private int valueCardinality = 0;

        // The number of entries in the map; -1 if unknown
        private int nrOfEntries = -1;

        /**
         * Create an empty map
         */
        public ImmMap() {
            this.imap = ImmutableDictionary.Create<AtomicMatchKey, Saxon.Hej.ma.map.KeyValuePair>();
            this.nrOfEntries = 0;
        }


        /**
         * Create a singleton map with a single key and value
         * @param key the key value
         * @param value the associated value
         * @return a singleton map
         */
        public static ImmMap singleton(AtomicValue key, GroundedValue value) {
            return (ImmMap) new ImmMap().addEntry(key, value);
        }

        /**
         * Create a map whose contents are a copy of an existing immutable map
         * @param imap the map to be copied
         */
        public ImmMap(ImmutableDictionary<AtomicMatchKey, Saxon.Hej.ma.map.KeyValuePair> imap) {
            this.imap = imap;
            nrOfEntries = -1;
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
            foreach (Saxon.Hej.ma.map.KeyValuePair pair in map.keyValuePairs()) {
                m2 = (ImmMap) m2.addEntry(pair.key, pair.value);
            }

            return m2;
        }

        /**
         * After adding an entry to the map, update the cached type information
         * @param key the new key
         * @param val the new associated value
         * @param wasEmpty true if the map was empty before adding these values
         */
        private void updateTypeInformation(AtomicValue key, Sequence val, bool wasEmpty) {
            if (wasEmpty) {
                keyUType = key.getUType();
                valueUType = SequenceTool.getUType(val);
                keyAtomicType = key.getItemType();
                valueItemType = MapItem.getItemTypeOfSequence(val);
                valueCardinality = SequenceTool.getCardinality(val);
            }
            else {
                keyUType = keyUType.union(key.getUType());
                valueUType = valueUType.union(SequenceTool.getUType(val));
                valueCardinality = Cardinality.union(valueCardinality, SequenceTool.getCardinality(val));
                if (key.getItemType() != keyAtomicType) {
                    keyAtomicType = null;
                }

                if (!MapItem.isKnownToConform(val, valueItemType)) {
                    valueItemType = null;
                }
            }
        }

        /**
         * Get the size of the map
         */
        public override int size() {
            if (nrOfEntries >= 0) {
                return nrOfEntries;
            }

            int count = 0;
            //noinspection UnusedDeclaration
            foreach (Saxon.Hej.ma.map.KeyValuePair entry in keyValuePairs()) {
                count++;
            }

            return nrOfEntries = count;
        }

        /**
         * Ask whether the map is empty
         *
         * @return true if and only if the size of the map is zero
         */
        public override bool isEmpty() {
            return nrOfEntries == 0 || !imap.GetEnumerator().MoveNext();
        }

        /**
         * Ask whether the map conforms to a given map type
         *
         * @param requiredKeyType   the required keyType
         * @param requiredValueType the required valueType
         * @param th        the type hierarchy cache for the configuration
         * @return true if the map conforms to the required type
         */
        public override bool conforms(AtomicType requiredKeyType, SequenceType requiredValueType, TypeHierarchy th) {
            if (isEmpty()) {
                return true;
            }

            if (keyAtomicType == requiredKeyType && valueItemType == requiredValueType.getPrimaryType() &&
                Cardinality.subsumes(requiredValueType.getCardinality(), valueCardinality)) {
                return true;
            }

            bool needFullCheck = false;
            if (requiredKeyType != BuiltInAtomicType.ANY_ATOMIC) {
                ItemType upperBoundKeyType = keyUType.toItemType();
                Affinity rel = th.relationship(requiredKeyType, upperBoundKeyType);
                if (rel == Affinity.SAME_TYPE || rel == Affinity.SUBSUMES) {
                    // The key type is matched
                }
                else if (rel == Affinity.DISJOINT) {
                    return false;
                }
                else {
                    needFullCheck = true;
                }
            }

            ItemType requiredValueItemType = requiredValueType.getPrimaryType();
            if (requiredValueItemType != BuiltInAtomicType.ANY_ATOMIC) {
                ItemType upperBoundValueType = valueUType.toItemType();
                Affinity rel = th.relationship(requiredValueItemType, upperBoundValueType);
                if (rel == Affinity.SAME_TYPE || rel == Affinity.SUBSUMES) {
                    // The value type is matched
                }
                else if (rel == Affinity.DISJOINT) {
                    return false;
                }
                else {
                    needFullCheck = true;
                }
            }

            int requiredValueCard = requiredValueType.getCardinality();
            if (!Cardinality.subsumes(requiredValueCard, valueCardinality)) {
                needFullCheck = true;
            }

            if (needFullCheck) {
                // we need to test the entries individually
                AtomicIterator keyIter = keys();
                AtomicValue key;
                while ((key = (AtomicValue) keyIter.next()) != null) {
                    if (!requiredKeyType.matches(key, th)) {
                        return false;
                    }

                    if (!requiredValueType.matches(get(key), th)) {
                        return false;
                    }
                }
            }

            return true;
        }

        /**
         * Get the type of the map. This method is used largely for diagnostics, to report
         * the type of a map when it differs from the required type. This method has the side-effect
         * of updating the internal type information held within the map.
         *
         * @param th The type hierarchy cache for the configuration
         * @return the type of this map
         */
        public override ItemType getItemType(TypeHierarchy th) {
            AtomicType keyType = null;
            ItemType valueType = null;
            int valueCard = 0;
            // we need to test the entries individually
            AtomicIterator keyIter = keys();
            AtomicValue key;
            while ((key = (AtomicValue) keyIter.next()) != null) {
                Sequence val = get(key);
                if (keyType == null) {
                    keyType = key.getItemType();
                    valueType = SequenceTool.getItemType(val, th);
                    valueCard = SequenceTool.getCardinality(val);
                }
                else {
                    keyType = (AtomicType) Type.getCommonSuperType(keyType, key.getItemType(), th);
                    valueType = Type.getCommonSuperType(valueType, SequenceTool.getItemType(val, th), th);
                    valueCard = Cardinality.union(valueCard, SequenceTool.getCardinality(val));
                }
            }

            if (keyType == null) {
                // implies the map is empty
                this.keyUType = UType.VOID;
                this.valueUType = UType.VOID;
                this.valueCardinality = 0;
                return MapType.ANY_MAP_TYPE;
            }
            else {
                this.keyUType = keyType.getUType();
                this.valueUType = valueType.getUType();
                this.valueCardinality = valueCard;
                return new MapType(keyType, SequenceType.makeSequenceType(valueType, valueCard));
            }
        }

        /**
         * Get the lowest common item type of the keys in the map
         *
         * @return the most specific type to which all the keys belong. If the map is
         *         empty, return UType.VOID (the type with no instances)
         */
        public override UType getKeyUType() {
            return keyUType;
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
            AtomicMatchKey amk = makeKey(key);

            bool isNew = !imap.ContainsKey(amk);
            bool empty = isEmpty();
            ImmutableDictionary<AtomicMatchKey, Saxon.Hej.ma.map.KeyValuePair> imap2 =
                imap.SetItem(amk, new Saxon.Hej.ma.map.KeyValuePair(key, value));
            ImmMap t2 = new ImmMap(imap2) {
                valueCardinality = this.valueCardinality,
                keyUType = keyUType,
                valueUType = valueUType,
                keyAtomicType = keyAtomicType,
                valueItemType = valueItemType
            };
            t2.updateTypeInformation(key, value, empty);
            if (nrOfEntries >= 0) {
                t2.nrOfEntries = isNew ? nrOfEntries + 1 : nrOfEntries;
            }

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
            imap = imap.SetItem(amk, new Saxon.Hej.ma.map.KeyValuePair(key, value));
            updateTypeInformation(key, value, empty);
            nrOfEntries = -1;
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
            ImmutableDictionary<AtomicMatchKey, Saxon.Hej.ma.map.KeyValuePair> m2
                = imap.Remove(makeKey(key));
            if (m2 == imap) {
                // The key is not present; the map is unchanged
                return this;
            }

            ImmMap result = new ImmMap(m2) {
                keyUType = keyUType,
                valueUType = valueUType,
                valueCardinality = valueCardinality,
                nrOfEntries = nrOfEntries - 1
            };
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
            if (imap.ContainsKey(matchKey)) {
                return imap[matchKey].value;
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
            return new KeyIterator(imap);
        }

        public override IEnumerable<KeyValuePair> keyValuePairs() {
            return imap.Values;
        }

        public class KeyIterator : AtomicIterator {
            private IEnumerator<Saxon.Hej.ma.map.KeyValuePair> _enumerator;
            private Saxon.Hej.ma.map.KeyValuePair _current;

            public KeyIterator(ImmutableDictionary<AtomicMatchKey, Saxon.Hej.ma.map.KeyValuePair> dictionary) {
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

// Copyright (c) 2021 Saxonica Limited