package net.sf.saxon.ma.map;

import io.vavr.collection.LinkedHashMap;
import io.vavr.control.Option;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.tree.iter.AtomicIterator;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.List;

/**
 * An XDM Map implementation using string keys, that maintains map entries in insertion order (new entries at the end)
 */
public class OrderedStringMap extends StringMap {

    private static class OrderedDictionaryBuilder implements MapBuilder {

        private LinkedHashMap<String, GroundedValue> map;

        public OrderedDictionaryBuilder() {
            map = LinkedHashMap.empty();
        }

        /**
         * During initial construction of the map, add a key-value pair
         *
         * @param key   the key
         * @param value the corresponding value
         */
        @Override
        public void put(AtomicValue key, GroundedValue value) {
            map = map.put(key.getStringValue(), value);
        }

        /**
         * During initial construction of the map, add a key-value pair where the key is a string
         *
         * @param key   the key
         * @param value the corresponding value
         */
        @Override
        public void put(String key, GroundedValue value) {
            map = map.put(key, value);
        }

        /**
         * Ask whether the map under construction contains a given key
         */

        public boolean contains(String key) {
            return map.get(key) != null;
        }

        /**
         * Get a value from the map under construction
         */

        public GroundedValue get(String key) {
            return map.get(key).getOrNull();
        }

        /**
         * Get a value from the map under construction
         */

        public GroundedValue get(AtomicValue key) {
            return map.get(key.getStringValue()).getOrNull();
        }

        /**
         * Get the constructed map. This method must only be called once; subsequent calls will return null
         *
         * @return the constructed map
         */
        @Override
        public OrderedStringMap getCompletedMap() {
            OrderedStringMap result = new OrderedStringMap(map);
            map = null;
            return result;
        }
    }

    public static MapBuilder getBuilder() {
        return new OrderedStringMap.OrderedDictionaryBuilder();
    }


    private final LinkedHashMap<String, GroundedValue> baseMap;

    public OrderedStringMap() {
        baseMap = LinkedHashMap.empty();

    }

    private OrderedStringMap(LinkedHashMap<String, GroundedValue> baseMap) {
        this.baseMap = baseMap;
    }

    /**
     * Get an entry from the Map
     *
     * @param key the value of the key
     * @return the value associated with the given key, or null if the key is not present in the map.
     */
    @Override
    public GroundedValue get(AtomicValue key) {
        Option<GroundedValue> val = baseMap.get(key.getStringValue());
        return (val.isEmpty() ? null : val.get());
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
        return false;
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
        for (GroundedValue val : baseMap.values()) {
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
     * empty, return UType.VOID
     */
    @Override
    public UType getKeyUType() {
        return UType.STRING;
    }

    /**
     * Get an entry from the map, supplying a Java String as the key
     *
     * @param key the key
     * @return the relevant entry if present, or null if absent.
     */
    @Override
    public GroundedValue get(String key) {
        Option<GroundedValue> val = baseMap.get(key);
        return (val.isEmpty() ? null : val.get());
    }

    /**
     * Get an entry from the map, supplying a {@link UnicodeString} as the key
     *
     * @param key the key
     * @return the relevant entry if present, or null if absent.
     */
    @Override
    public GroundedValue get(UnicodeString key) {
        Option<GroundedValue> val = baseMap.get(key.toString());
        return (val.isEmpty() ? null : val.get());
    }

    /**
     * Ask whether a given string is present as a key in the map
     *
     * @param key the key being tested
     * @return true if the key is present
     */
    @Override
    public boolean contains(String key) {
        Option<GroundedValue> val = baseMap.get(key);
        return (!val.isEmpty());
    }

    /**
     * Ask whether a given string is present as a key in the map
     *
     * @param key the key being tested
     * @return true if the key is present
     */
    @Override
    public boolean contains(UnicodeString key) {
        Option<GroundedValue> val = baseMap.get(key.toString());
        return (!val.isEmpty());
    }

    /**
     * Get the size of the map
     *
     * @return the number of keys/entries present in this map
     */
    @Override
    public int size() {
        return baseMap.size();
    }

    /**
     * Get the set of all key values in the map.
     *
     * @return a set containing all the key values present in the map, in unpredictable order
     */
    @Override
    public AtomicIterator keys() {
        // TODO: this is inefficient
        return new DictionaryMap.KeyIterator(baseMap.toJavaMap());
    }

    /**
     * Get the set of all key-value pairs in the map
     *
     * @return an iterable containing all the key-value pairs
     */
    @Override
    public Iterable<KeyValuePair> keyValuePairs() {
        List<KeyValuePair> pairs = new ArrayList<>();
        baseMap.forEach((k,v) -> pairs.add(new KeyValuePair(new StringValue(k), v)));
        return pairs;
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
        return new OrderedStringMap(baseMap.put(key.getStringValue(), value));
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
        return new OrderedStringMap(baseMap.remove(key.getStringValue()));
    }

}

