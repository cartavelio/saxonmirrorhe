////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.ma.map;

import net.sf.saxon.ma.json.JsonParser;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.str.StringTool;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trace.Instrumentation;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.tree.iter.AtomicIterator;
import net.sf.saxon.tree.iter.MappedAtomicIterator;
import net.sf.saxon.tree.jiter.ReverseListIterable;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import java.util.*;

/**
 * An implementation of MapItem where the strings are keys, and query and modification are both unlikely.
 * This implementation is designed particularly for the result of parsing JSON, where it is quite likely
 * that many of the nested maps will either be ignored entirely, or used only for serialization. The
 * complication is that it's necessary to detect duplicate keys, which is done with the help of a
 * Bloom filter.
 */

public class LazyStringMap extends StringMap implements Iterable<KeyValuePair> {

    public static class Builder {

        private List<UnicodeString> keys = new ArrayList<>();
        private List<GroundedValue> values = new ArrayList<>();
        private final JsonParser parser;
        private final int flags;
        public Builder(JsonParser parser, int flags) {
            Instrumentation.count("LazyStringMapBuilder");
            this.parser = parser;
            this.flags = flags;
        }

        /**
         * Add a key-value pair
         *
         * @param key   the key
         * @param value the corresponding value
         */

        public void put(UnicodeString key, GroundedValue value) throws XPathException {
            Instrumentation.count("LazyStringMapBuilder.put()");
            keys.add(key);
            values.add(value);
        }

        /**
         * Get the constructed map. This method must only be called once, because the
         * lists of keys and values are used within the constructed map and are mutable.
         *
         * @return the constructed map
         * @throws XPathException if duplicate keys were found, and not detected earlier
         */

        public MapItem getCompletedMap() throws XPathException {
            if (keys.size() == 0) {
                return EmptyMap.getInstance();
            } else if (keys.size() == 1) {
                return new SingleEntryMap(new StringValue(keys.get(0)),values.get(0));
            }
            // quick scan for possible duplicates using a bloom filter
            boolean possibleDuplicates = false;
            int filterSize = keys.size() < 50 ? 1024 : 65536;
            int mask = filterSize - 1;
            BitSet bloom = new BitSet(filterSize);
            for (UnicodeString key : keys) {
                long hash = StringTool.longHashCode(key);
                int hash0 = (int)(hash >> 40 & mask);
                int hash1 = (int)(hash >> 20 & mask);
                int hash2 = (int)hash & mask;
                if (bloom.get(hash0) && bloom.get(hash1) && bloom.get(hash2)) {
                    Instrumentation.count("Bloom clash");
                    possibleDuplicates = true;
                    break;
                }
                bloom.set(hash0);
                bloom.set(hash1);
                bloom.set(hash2);
            }
            if (possibleDuplicates) {
                // look for duplicates more thoroughly
                boolean reverseOrder = (flags & JsonParser.DUPLICATES_LAST) != 0;

                List<UnicodeString> retainedKeys = new ArrayList<>(keys.size());
                List<GroundedValue> retainedValues = new ArrayList<>(keys.size());

                Iterator<UnicodeString> keyIter =
                        (reverseOrder ? reverse(keys) : keys).iterator();
                Iterator<GroundedValue> valueIter =
                        (reverseOrder ? reverse(values) : values).iterator();
                Set<UnicodeString> keySet = new HashSet<>();

                while (keyIter.hasNext() && valueIter.hasNext()) {
                    UnicodeString key = keyIter.next();
                    GroundedValue value = valueIter.next();
                    boolean duplicate = !keySet.add(key);
                    if (duplicate) {
                        // Duplicate key found
                        Instrumentation.count("Duplicate key");
                        if ((flags & JsonParser.DUPLICATES_REJECTED) != 0) {
                            Instrumentation.count("Duplicate rejected");
                            throw new XPathException("Duplicate key value '" + key + "' in JSON object", "FOJS0003");
                        } else {
                            // ignore this entry
                            Instrumentation.count("Duplicate ignored");
                            break;
                        }
                    } else {
                        retainedKeys.add(key);
                        retainedValues.add(value);
                    }
                }
                if (reverseOrder) {
                    Collections.reverse(retainedKeys);
                    Collections.reverse(retainedValues);
                }
                keys = retainedKeys;
                values = retainedValues;
            }
            if (keys.size() <= 20) {
                Instrumentation.count("Shaped maps");
                Shape shape = parser.obtainShape(keys);
                return new ShapedMap(shape, values.toArray(values.toArray(new GroundedValue[0])));
            }
            Instrumentation.count("Lazy string maps");
            return new LazyStringMap(keys, values);
        }


    }

    @CSharpReplaceBody(code="return list.Reverse();")
    private static <T> Iterable<T> reverse(List<T> list) {
        return new ReverseListIterable<T>(list);
    }

    private final List<UnicodeString> keyList;
    private final List<GroundedValue> valueList;
    private Map<UnicodeString, GroundedValue> baseMap; // constructed lazily on demand


    /**
     * Create a lazy string map.
     * @param keys the string-valued keys
     * @param values the corresponding values
     */

    public LazyStringMap(List<UnicodeString> keys, List<GroundedValue> values) {
        this.keyList = keys;
        this.valueList = values;
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
        return baseMap.containsKey(key);
    }

    /**
     * Ask whether a given string is present as a key in the map
     *
     * @param key the key being tested
     * @return true if the key is present
     */
    public boolean containsU(UnicodeString key) {
        return contains(key.toString());
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
        Instrumentation.count("Map keys()");
        return new MappedAtomicIterator<>(keyList.iterator(), StringValue::new);
    }

    /**
     * Get the set of all key-value pairs in the map
     *
     * @return an iterable containing all the key-value pairs
     */
    @Override
    public Iterator<KeyValuePair> iterator() {
        Instrumentation.count("Map keyValuePairs()");
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
    public MapItem addEntry(AtomicValue key, GroundedValue value) {
        return toHashTrieMap().addEntry(key, value);
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
        return toHashTrieMap().remove(key);
    }

    /**
     * Ask whether the map conforms to a given map type
     *
     * @param keyType   the required keyType
     * @param valueType the required valueType
     * @param th        the type hierarchy cache for the configuration
     * @return true if the map conforms to the required type
     */
    @Override
    public boolean conforms(PlainType keyType, SequenceType valueType, TypeHierarchy th) {
        if (isEmpty()) {
            return true;
        }
        if (!(keyType == BuiltInAtomicType.STRING || keyType == BuiltInAtomicType.ANY_ATOMIC)) {
            return false;
        }
        if (valueType.equals(SequenceType.ANY_SEQUENCE)) {
            return true;
        }
        for (GroundedValue val : valueList) {
            if (!valueType.matches(val, th)) {
                return false;
            }
        }
        return true;
    }

    /**
     * Get the type of the map. This method is used largely for diagnostics, to report
     * the type of a map when it differs from the required type.
     *
     * @param th the type hierarchy cache
     * @return the type of this map
     */
    @Override
    public ItemType getItemType(TypeHierarchy th) {
        ItemType valueType = null;
        int valueCard = 0;
        // we need to test the entries individually
        for (GroundedValue val : valueList) {
            if (valueType == null) {
                valueType = SequenceTool.getItemType(val, th);
                valueCard = SequenceTool.getCardinality(val);
            } else {
                valueType = Type.getCommonSuperType(valueType, SequenceTool.getItemType(val, th), th);
                valueCard = Cardinality.union(valueCard, SequenceTool.getCardinality(val));
            }
        }
        if (valueType == null) {
            // empty map
            return MapType.EMPTY_MAP_TYPE;
        } else {
            return new MapType(BuiltInAtomicType.STRING, SequenceType.makeSequenceType(valueType, valueCard));
        }
    }

    /**
     * Get the lowest common item type of the keys in the map
     *
     * @return the most specific type to which all the keys belong. If the map is
     * empty, return {@link UType#VOID}
     */
    @Override
    public UType getKeyUType() {
        return UType.STRING;
    }

    /**
     * Convert to a HashTrieMap
     */

    private HashTrieMap toHashTrieMap() {
        //System.err.println("Dictionary rewrite!!!!");
        Instrumentation.count("Map rewrite");
        MapBuilder target = HashTrieMap.getBuilder();
        for (int i = 0; i < keyList.size(); i++) {
            target.put(new StringValue(keyList.get(i)), valueList.get(i));
        }
        return (HashTrieMap)target.getCompletedMap();
    }
}

