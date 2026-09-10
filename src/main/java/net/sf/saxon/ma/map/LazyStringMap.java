////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.ma.map;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trace.Instrumentation;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.tree.iter.AtomicIterator;
import net.sf.saxon.tree.iter.MappedAtomicIterator;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;

import java.util.*;

/**
 * An implementation of MapItem where the strings are keys, and query and modification are both unlikely.
 * This implementation is designed particularly for the result of parsing JSON, where it is quite likely
 * that many of the nested maps will either be ignored entirely, or used only for serialization. The
 * complication is that it's necessary to detect duplicate keys, which is done with the help of a
 * Bloom filter.
 */

public class LazyStringMap extends StringMapItem implements Iterable<KeyValuePair> {

//    public static class Builder {
//
//        private List<UnicodeString> keys = new ArrayList<>();
//        private List<GroundedValue> values = new ArrayList<>();
//        private final JsonParser parser;
//        private final int flags;
//        public Builder(JsonParser parser, int flags) {
//            this.parser = parser;
//            this.flags = flags;
//        }
//
//        /**
//         * Add a key-value pair
//         *
//         * @param key   the key
//         * @param value the corresponding value
//         */
//
//        public void put(UnicodeString key, GroundedValue value) throws XPathException {
//            //Instrumentation.count("LazyStringMapBuilder.put()");
//            keys.add(key);
//            values.add(value);
//        }
//
//        /**
//         * Get the constructed map. This method must only be called once, because the
//         * lists of keys and values are used within the constructed map and are mutable.
//         *
//         * @return the constructed map
//         * @throws XPathException if duplicate keys were found, and not detected earlier
//         */
//
//        public MapItem getCompletedMap() throws XPathException {
//            if (keys.size() == 0) {
//                return EmptyMap.getInstance();
//            } else if (keys.size() == 1) {
//                return new SingleEntryMap(new StringValue(keys.get(0)),values.get(0));
//            }
//            // quick scan for possible duplicates using a bloom filter
//            boolean possibleDuplicates = false;
//            int filterSize = keys.size() < 50 ? 1024 : 65536;
//            int mask = filterSize - 1;
//            BitSet bloom = new BitSet(filterSize);
//            for (UnicodeString key : keys) {
//                long hash = StringTool.longHashCode(key);
//                int hash0 = (int)(hash >> 40 & mask);
//                int hash1 = (int)(hash >> 20 & mask);
//                int hash2 = (int)hash & mask;
//                if (bloom.get(hash0) && bloom.get(hash1) && bloom.get(hash2)) {
//                    Instrumentation.count("Bloom clash");
//                    possibleDuplicates = true;
//                    break;
//                }
//                bloom.set(hash0);
//                bloom.set(hash1);
//                bloom.set(hash2);
//            }
//            if (possibleDuplicates) {
//                // look for duplicates more thoroughly
//                boolean reverseOrder = (flags & JsonParser.DUPLICATES_LAST) != 0;
//
//                List<UnicodeString> retainedKeys = new ArrayList<>(keys.size());
//                List<GroundedValue> retainedValues = new ArrayList<>(keys.size());
//
//                Iterator<UnicodeString> keyIter =
//                        (reverseOrder ? reverse(keys) : keys).iterator();
//                Iterator<GroundedValue> valueIter =
//                        (reverseOrder ? reverse(values) : values).iterator();
//                Set<UnicodeString> keySet = new HashSet<>();
//
//                while (keyIter.hasNext() && valueIter.hasNext()) {
//                    UnicodeString key = keyIter.next();
//                    GroundedValue value = valueIter.next();
//                    boolean duplicate = !keySet.add(key);
//                    if (duplicate) {
//                        // Duplicate key found
//                        Instrumentation.count("Duplicate key");
//                        if ((flags & JsonParser.DUPLICATES_REJECTED) != 0) {
//                            Instrumentation.count("Duplicate rejected");
//                            throw new XPathException("Duplicate key value '" + key + "' in JSON object", "FOJS0003");
//                        } else {
//                            // ignore this entry
//                            Instrumentation.count("Duplicate ignored");
//                            continue;
//                        }
//                    } else {
//                        retainedKeys.add(key);
//                        retainedValues.add(value);
//                    }
//                }
//                if (reverseOrder) {
//                    Collections.reverse(retainedKeys);
//                    Collections.reverse(retainedValues);
//                }
//                keys = retainedKeys;
//                values = retainedValues;
//            }
//            if (keys.size() <= 20) {
//                Instrumentation.count("Shaped maps");
//                Shape shape = parser.obtainShape(keys);
//                return new ShapedMap(shape, values.toArray(values.toArray(new GroundedValue[0])));
//            }
//            Instrumentation.count("Lazy string maps");
//            return new LazyStringMap(keys, values);
//        }
//
//
//    }
//
//    @CSharpReplaceBody(code="return list.Reverse();")
//    private static <T> Iterable<T> reverse(List<T> list) {
//        return new ReverseListIterable<T>(list);
//    }

    private final List<UnicodeString> keyList;
    private final List<GroundedValue> valueList;
    private Map<UnicodeString, GroundedValue> baseMap; // constructed lazily on demand

    private static boolean DEBUG = true;

    /**
     * Create a lazy string map.
     * @param keys the string-valued keys. The caller warrants that there are no duplicate keys
     *             in the list.
     * @param values the corresponding values
     */

    public LazyStringMap(List<UnicodeString> keys, List<GroundedValue> values) {
        this.keyList = keys;
        this.valueList = values;
//        if (DEBUG) {
//            HashSet<UnicodeString> keySet = new HashSet<>();
//            for (UnicodeString s : keys) {
//                if (keySet.contains(s)) {
//                    throw new IllegalArgumentException("Duplicate key " + s);
//                }
//                keySet.add(s);
//            }
//        }
    }

    /**
     * When required, build the underlying map
     */

    private synchronized void prepare() {
        if (baseMap == null) {
            Instrumentation.count("Map indexed");
            baseMap = new HashMap<>(keyList.size());
            for (int i = 0; i < keyList.size(); i++) {
                baseMap.put(keyList.get(i), valueList.get(i));
            }
        }
    }

    /**
     * Ask whether a given string is present as a key in the map
     * @param key the key being tested
     * @return true if the key is present
     */
    public synchronized boolean contains(String key) {
        prepare();
        return baseMap.containsKey(StringView.of(key));
    }

    /**
     * Ask whether a given string is present as a key in the map
     *
     * @param key the key being tested
     * @return true if the key is present
     */
    public boolean containsU(UnicodeString key) {
        prepare();
        return baseMap.containsKey(key);
    }

    /**
     * Get an entry from the Map
     *
     * @param key the value of the key
     * @return the value associated with the given key, or null if the key is not present in the map.
     */
    @Override
    public GroundedValue get(AtomicValue key) {
        if (key instanceof StringValue) {
            return getU(key.getUnicodeStringValue());
        } else {
            return null;
        }
    }

    /**
     * Get an entry from the map, supplying a Java String as the key
     * @param key the key
     * @return the relevant entry if present, or null if absent.
     */
    public GroundedValue get(String key) {
        return getU(StringView.of(key));
    }

    /**
     * Get an entry from the map, supplying a {@link UnicodeString} as the key
     *
     * @param key the key
     * @return the relevant entry if present, or null if absent.
     */
    @Override
    public GroundedValue getU(UnicodeString key) {
        prepare();
        return baseMap.get(key);
    }

    /**
     * Get the size of the map
     *
     * @return the number of keys/entries present in this map
     */
    @Override
    public int size() {
        return keyList.size();
    }

    /**
     * Ask whether the map is empty
     *
     * @return true if and only if the size of the map is zero
     */
    @Override
    public boolean isEmpty() {
        return keyList.isEmpty();
    }

    /**
     * Get the set of all key values in the map.
     *
     * @return a set containing all the key values present in the map, in order
     */
    @Override
    public AtomicIterator keys() {
        // Written this way for transpilation
        return new MappedAtomicIterator<UnicodeString>(keyList.iterator(), u -> new StringValue(u));
    }

    /**
     * Get the set of all key-value pairs in the map
     *
     * @return an iterable containing all the key-value pairs
     */
    @Override
    public Iterator<KeyValuePair> iterator() {
        return new KeyValuePairIterator(keyList, valueList);
    }

    /**
     * Get the set of all key-value pairs in the map
     *
     * @return an iterable containing all the key-value pairs
     */
    @Override
    public Iterable<KeyValuePair> keyValuePairs() {
        return this;
    }

    private static class KeyValuePairIterator implements Iterator<KeyValuePair> {
        private int i = 0;
        private final List<UnicodeString> keys;
        private final List<GroundedValue> values;
        public KeyValuePairIterator(List<UnicodeString> keys, List<GroundedValue> values) {
            this.keys = keys;
            this.values = values;
        }

        @Override
        public boolean hasNext() {
            return i < keys.size();
        }

        /**
         * Returns the next element in the iteration.
         *
         * @return the next element in the iteration
         * @throws NoSuchElementException if the iteration has no more elements
         */
        @Override
        public KeyValuePair next() {
            int j = i++;
            return new KeyValuePair(new StringValue(keys.get(j)), values.get(j));
        }
    }



    /**
     * Create a new map containing the existing entries in the map plus an additional entry,
     * without modifying the original. If there is already an entry with the specified key,
     * this entry is replaced by the new entry.
     *
     * @param key   the key of the new entry
     * @param value the value associated with the new entry
     * @return the new map containing the additional entry
     */
    @Override
    public MapItem put(AtomicValue key, GroundedValue value) {
        return toGeneralMap().put(key, value);
    }

    @Override
    public MapItem putU(UnicodeString key, GroundedValue value) {
        return put(new StringValue(key), value);
    }



    /**
     * Remove an entry from the map
     *
     * @param key the key of the entry to be removed
     * @return a new map in which the requested entry has been removed; or this map
     * unchanged if the specified key was not present
     */
    @Override
    public MapItem remove(AtomicValue key) {
        return toGeneralMap().remove(key);
    }

    @Override
    public MapItem removeU(UnicodeString key) {
        return remove(new StringValue(key));
    }

    /**
     * Convert to a HashTrieMap
     */

    private MapItem toGeneralMap() {
        //System.err.println("Dictionary rewrite!!!!");
        Instrumentation.count("Map rewrite");
        io.vavr.collection.LinkedHashMap<UnicodeString, GroundedValue> baseMap = emptyBaseMap();
        for (int i = 0; i < keyList.size(); i++) {
            baseMap = baseMap.put(keyList.get(i), valueList.get(i));
        }
        return new OrderedStringMap(baseMap);
    }

    @CSharpReplaceBody(code="return System.Collections.Immutable.ImmutableDictionary<Saxon.Hej.str.UnicodeString,Saxon.Hej.om.GroundedValue>.Empty;")
    private static io.vavr.collection.LinkedHashMap<UnicodeString, GroundedValue> emptyBaseMap() {
        return io.vavr.collection.LinkedHashMap.empty();
    }
}

