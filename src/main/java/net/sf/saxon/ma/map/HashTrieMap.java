////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.ma.map;

import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.ma.trie.ImmutableHashTrieMap;
import net.sf.saxon.ma.trie.ImmutableMap;
import net.sf.saxon.ma.trie.TrieKVP;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.transpile.CSharpSuppressWarnings;
import net.sf.saxon.tree.iter.AtomicIterator;
import net.sf.saxon.tree.jiter.IterableUsingIteratorSupplier;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;

import java.util.Iterator;

/**
 * An immutable map. This implementation, which uses a hash trie, was introduced in Saxon 9.6
 */
public class HashTrieMap extends MapTypeMaintainer {

    // The underlying trie holds key-value pairs, but these do not correspond directly
    // to the key value pairs in the XDM map. Instead, the key in the trie is an AtomicMatchKey
    // based on the XDM key, which allows equality matching to differ from the Java-level
    // equals method (to take account of collations, etc). The value in the trie is
    // actually a tuple holding both the real key, and the value.

    private ImmutableMap<AtomicMatchKey, KeyValuePair> imap;

    // The number of entries in the map; -1 if unknown
    private int entries = -1;

    protected static class HashTrieMapBuilder implements MapBuilder {

        HashTrieMap map = new HashTrieMap();
        public HashTrieMapBuilder() {

        }

        /**
         * During initial construction of the map, add a key-value pair
         *
         * @param key   the key
         * @param value the corresponding value
         */
        @Override
        public void put(AtomicValue key, GroundedValue value) {
            map.initialPut(key, value);
        }

        /**
         * During initial construction of the map, add a key-value pair where the key is a string
         *
         * @param key   the key
         * @param value the corresponding value
         */
        @Override
        public void put(String key, GroundedValue value) {
            put(new StringValue(key), value);
        }

        /**
         * Ask whether the map under construction contains a given key
         */

        public boolean contains(String key) {
            return map.get(new StringValue(key)) != null;
        }

        /**
         * Get the value corresponding to a given xs:string key in the map under construction
         *
         * @param key
         * @return the value if it exists, or null otherwise.
         */
        @Override
        public GroundedValue get(String key) {
            return get(new StringValue(key));
        }

        /**
         * Get the value corresponding to a given xs:string key in the map under construction
         *
         * @param key
         * @return the value if it exists, or null otherwise.
         */
        @Override
        public GroundedValue get(AtomicValue key) {
            return map.get(key);
        }

        /**
         * Get the constructed map. This method must only be called once; subsequent calls will return null
         *
         * @return the constructed map
         */
        @Override
        public HashTrieMap getCompletedMap() {
            HashTrieMap result = map;
            map = null;
            return result;
        }
    }


    /**
     * Create an empty map
     */

    public HashTrieMap() {
        this.imap = ImmutableHashTrieMap.empty();
        this.entries = 0;
    }

    public static MapBuilder getBuilder() {
        return new HashTrieMapBuilder();
    }

    /**
     * Create a singleton map with a single key and value
     * @param key the key value
     * @param value the associated value
     * @return a singleton map
     */

    public static HashTrieMap singleton(AtomicValue key, GroundedValue value) {
        return new HashTrieMap().addEntry(key, value);
    }

    /**
     * Create a map whose contents are a copy of an existing immutable map
     * @param imap the map to be copied
     */

    public HashTrieMap(ImmutableMap<AtomicMatchKey, net.sf.saxon.ma.map.KeyValuePair> imap) {
        this.imap = imap;
    }

    /**
     * Create a map whose entries are copies of the entries in an existing MapItem
     *
     * @param map       the existing map to be copied
     * @return the new map
     */

    public static HashTrieMap copy(MapItem map) {
        if (map instanceof HashTrieMap) {
            return (HashTrieMap)map;
        }
        HashTrieMap m2 = new HashTrieMap();
        for (net.sf.saxon.ma.map.KeyValuePair pair : map.keyValuePairs()) {
            m2 = m2.addEntry(pair.key, pair.value);
        }
        return m2;
    }

    /**
     * Get the size of the map
     */

    @Override
    public int size() {
        if (entries >= 0) {
            return entries;
        }
        int count = 0;
        //noinspection UnusedDeclaration
        for (net.sf.saxon.ma.map.KeyValuePair entry: keyValuePairs()) {
            count++;
        }
        return entries = count;
    }

    /**
     * Ask whether the map is empty
     *
     * @return true if and only if the size of the map is zero
     */
    @Override
    @CSharpSuppressWarnings("UnsafeIteratorConversion")
    public boolean isEmpty() {
        return entries == 0 || !imap.iterator().hasNext();
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

    @Override
    public HashTrieMap addEntry(AtomicValue key, GroundedValue value) {
        AtomicMatchKey amk = makeKey(key);

        boolean isNew = imap.get(amk) == null;
        boolean empty = isEmpty();
        ImmutableMap<AtomicMatchKey, net.sf.saxon.ma.map.KeyValuePair> imap2 = imap.put(amk, new net.sf.saxon.ma.map.KeyValuePair(key, value));
        HashTrieMap t2 = new HashTrieMap(imap2);
        t2.copyTypeInfoFrom(this);
        t2.updateTypeInformation(key, value, empty);
        if (entries >= 0) {
            t2.entries = isNew ? entries + 1 : entries;
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

    private boolean initialPut(AtomicValue key, GroundedValue value) {
//        if (Instrumentation.ACTIVE) {
//            Instrumentation.count("initialPut");
//        }
        boolean empty = isEmpty();
        AtomicMatchKey amk = makeKey(key);
        boolean exists = imap.get(amk) != null;
        imap = imap.put(amk, new net.sf.saxon.ma.map.KeyValuePair(key, value));
        updateTypeInformation(key, value, empty);
        entries = -1;
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

    @Override
    public HashTrieMap remove(AtomicValue key) {
        // This code used to assume that if the key wasn't in the
        // map, imap.remove() would return the original object
        // unchanged. But that was only true if the hash bucket
        // that would have contained the value was empty. That
        // won't be the case if other values happen to have been
        // assigned that bucket. So now we do an explicit check.
        // This is probably slower, but remove() is an uncommon
        // operation. And it gives the correct result!
        if (imap.get(makeKey(key)) == null) {
            // The key is not present; the map is unchanged
            return this;
        }

        ImmutableMap<AtomicMatchKey, net.sf.saxon.ma.map.KeyValuePair> m2 = imap.remove(makeKey(key));
        HashTrieMap result = new HashTrieMap(m2);
        result.copyTypeInfoFrom(this);
        result.entries = entries-1;
        return result;
    }

    /**
     * Get an entry from the Map
     *
     * @param key     the value of the key
     * @return the value associated with the given key, or null if the key is not present in the map
     */

    @Override
    public GroundedValue get(AtomicValue key)  {
        net.sf.saxon.ma.map.KeyValuePair o = imap.get(makeKey(key));
        return o==null ? null : o.value;
    }

    /**
     * Get an key/value pair from the Map
     *
     * @param key     the value of the key
     * @return the key-value-pair associated with the given key, or null if the key is not present in the map
     */

    public net.sf.saxon.ma.map.KeyValuePair getKeyValuePair(AtomicValue key) {
        return imap.get(makeKey(key));
    }

    /**
     * Get the set of all key values in the map
     * @return an iterator over the keys, in undefined order
     */

    @Override
    public AtomicIterator keys() {
        return new AtomicIterator() {

            final Iterator<TrieKVP<AtomicMatchKey, KeyValuePair>> baseIter = imap.iterator();

            @Override
            @CSharpSuppressWarnings("UnsafeIteratorConversion")
            public AtomicValue next() {
                if (baseIter.hasNext()) {
                    return baseIter.next().value.key;
                } else {
                    return null;
                }
            }
        };

    }

    /**
     * Get the set of all key-value pairs in the map
     *
     * @return an iterator over the key-value pairs
     */
    @Override
    public Iterable<net.sf.saxon.ma.map.KeyValuePair> keyValuePairs() {
        return new IterableUsingIteratorSupplier<>(this::iterateKeyValuePairs);
    }

    protected Iterator<net.sf.saxon.ma.map.KeyValuePair> iterateKeyValuePairs() {
        return new Iterator<>() {
            final Iterator<TrieKVP<AtomicMatchKey, KeyValuePair>> baseIter = imap.iterator();

            @Override
            public boolean hasNext() {
                return baseIter.hasNext();
            }

            @Override
            public net.sf.saxon.ma.map.KeyValuePair next() {
                return baseIter.next().value;
            }

            @Override
            public void remove() {
                baseIter.remove();
            }
        };
    }


    public void diagnosticDump() {
        System.err.println("Map details:");
        for (TrieKVP<AtomicMatchKey, KeyValuePair> entry : imap) {
            AtomicMatchKey k1 = entry.key;
            AtomicValue k2 = entry.value.key;
            Sequence v = entry.value.value;
            System.err.println(k1.getClass() + " " + k1 +
                                       " #:" + k1.hashCode() +
                                       " = (" + k2.getClass() + " " + k2 + " : " + v + ")");
        }
    }


    public String toString() {
        return MapItem.mapToString(this);
    }
}

// Copyright (c) 2018-2023 Saxonica Limited



