////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.ma.map;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.transpile.CSharpSuppressWarnings;
import net.sf.saxon.tree.iter.AtomicIterator;
import net.sf.saxon.tree.jiter.IterableUsingIteratorSupplier;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;

/**
 * A simple implementation of MapItem where the keys are strings, and modification is unlikely.
 * A DictionaryMap does not maintain order, so in 4.0 it is suitable only in cases where it
 * is known that order does not matter (for example, for the result of a function
 * such as parse-uri()).
 */

public class DictionaryMap extends StringMapItem {

    protected final Map<String, GroundedValue> map;

    /**
     * Create an empty dictionary.
     */

    public DictionaryMap() {
        map = new HashMap<>();
    }

    /**
     * Create an empty dictionary.
     * @param size estimated final size of the dictionary
     */

    public DictionaryMap(int size) {
        map = new HashMap<>(size);
    }

    /**
     * Create a dictionary backed by an existing map. It is the caller's responsibility to ensure that
     * this map is not subsequently modified.
     *
     * @param underlyingMap the initial contents of the dictionary
     */
    public DictionaryMap(Map<String, GroundedValue> underlyingMap) {
        map = underlyingMap;
    }

    /**
     * Ask whether a given string is present as a key in the map
     * @param key the key being tested
     * @return true if the key is present
     */
    public boolean contains(String key) {
        return map.containsKey(key);
    }

    /**
     * Ask whether a given string is present as a key in the map
     *
     * @param key the key being tested
     * @return true if the key is present
     */
    public boolean containsU(UnicodeString key) {
        return map.containsKey(key.toString());
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
            return map.get(key.getStringValue());
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
        return map.get(key);
    }

    /**
     * Get an entry from the map, supplying a {@link UnicodeString} as the key
     *
     * @param key the key
     * @return the relevant entry if present, or null if absent.
     */
    @Override
    public GroundedValue getU(UnicodeString key) {
        return map.get(key.toString());
    }

    /**
     * Get the size of the map
     *
     * @return the number of keys/entries present in this map
     */
    @Override
    public int size() {
        return map.size();
    }

    /**
     * Ask whether the map is empty
     *
     * @return true if and only if the size of the map is zero
     */
    @Override
    public boolean isEmpty() {
        return map.isEmpty();
    }

    /**
     * Get the set of all key values in the map.
     *
     * @return a set containing all the key values present in the map, in unpredictable order
     */
    @Override
    public AtomicIterator keys() {
        return new KeyIteratorImpl(map);
    }

    /**
     * Get the set of all key-value pairs in the map
     *
     * @return an iterable containing all the key-value pairs
     */
    @Override
    @CSharpReplaceBody(code="return new Saxon.Hej.tree.jiter.IterableUsingIteratorSupplier<Saxon.Hej.ma.map.KeyValuePair>(\n"
            + "                () => Saxon.Impl.Helpers.MapUtils.convertStringKeyValuePairs(map));")
    public Iterable<KeyValuePair> keyValuePairs() {
        return new IterableUsingIteratorSupplier<>(() -> {
            Iterator<Map.Entry<String, GroundedValue>> entries = map.entrySet().iterator();

            return new net.sf.saxon.tree.jiter.MappingJavaIterator<>(
                    entries,
                    e -> new KeyValuePair(new StringValue(e.getKey()), e.getValue()));
        });
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
    public StringMapItem removeU(UnicodeString key) {
        throw new UnsupportedOperationException();
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
        for (GroundedValue val : map.values()) {
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
        for (Map.Entry<String, GroundedValue> entry : map.entrySet()) {
            GroundedValue val = entry.getValue();
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
        return map.isEmpty() ? UType.VOID : UType.STRING;
    }

    /**
     * Convert to a HashTrieMap
     */

    private MapItem toGeneralMap() {
        //System.err.println("Dictionary rewrite!!!!");
        MapBuilder target = HashTrieMap.getBuilder();
        for (Map.Entry<String, GroundedValue> entry : map.entrySet()) {
            target.put(new StringValue(entry.getKey()), entry.getValue());
        }
        return (HashTrieMap)target.getCompletedMap();
    }

    protected static class KeyIteratorImpl implements AtomicIterator {
        Iterator<String> keyIter;

        public KeyIteratorImpl(Map<String, GroundedValue> hashMap) {
            this.keyIter = hashMap.keySet().iterator();
        }

        @CSharpSuppressWarnings("UnsafeIteratorConversion")
        public AtomicValue next() {
            if (this.keyIter.hasNext()) {
                return new StringValue(this.keyIter.next());
            } else {
                return null;
            }
        }
    }
}

