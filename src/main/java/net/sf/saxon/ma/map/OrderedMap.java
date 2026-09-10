package net.sf.saxon.ma.map;

import io.vavr.collection.LinkedHashMap;
import io.vavr.control.Option;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.tree.jiter.IterableUsingIteratorSupplier;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;

/**
 * An XDM Map implementation using arbitrary atomic keys, that maintains map entries
 * in insertion order (new entries at the end)
 */
public class OrderedMap extends MapItem {

    private static class OrderedMapBuilder implements MapBuilder {

        private LinkedHashMap<AtomicMatchKey, KeyValuePair> map;

        public OrderedMapBuilder() {
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
//            if (counter++ % 1000 == 0) {
//                System.err.println("Put " + counter + " at size " + map.size());
//            }
            map = map.put(key.asMapKey(), new KeyValuePair(key, value));
        }
        private static int counter = 0;

        /**
         * During initial construction of the map, add a key-value pair where the key is a string
         *
         * @param key   the key
         * @param value the corresponding value
         */

        public void put(String key, GroundedValue value) {
            put(new StringValue(key), value);
        }

        /**
         * Get a value from the map under construction
         */

        public GroundedValue get(AtomicValue key) {
            Option<KeyValuePair> optionalKVP = map.get(key.asMapKey());
            if (optionalKVP.isEmpty()) {
                return null;
            }
            return optionalKVP.get().value;
        }

        /**
         * Get the constructed map. This method must only be called once; subsequent calls will return null
         *
         * @return the constructed map
         */
        @Override
        public OrderedMap getCompletedMap() {
            OrderedMap result = new OrderedMap(map);
            map = null;
            return result;
        }
    }

    public static MapBuilder getBuilder() {
        return new OrderedMapBuilder();
    }

    /**
     * Create a map whose entries are copies of the entries in an existing MapItem
     *
     * @param map the existing map to be copied
     * @return the new map
     */

    public static OrderedMap copy(MapItem map) {
        if (map instanceof OrderedMap) {
            return (OrderedMap) map;
        }
        OrderedMap m2 = new OrderedMap();
        for (net.sf.saxon.ma.map.KeyValuePair pair : map.keyValuePairs()) {
            m2 = (OrderedMap)m2.put(pair.key, pair.value);
        }
        return m2;
    }


    private final LinkedHashMap<AtomicMatchKey, KeyValuePair> baseMap;

    public OrderedMap() {
        baseMap = LinkedHashMap.empty();

    }

    private OrderedMap(LinkedHashMap<AtomicMatchKey, KeyValuePair> baseMap) {
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
        Option<KeyValuePair> optionalKVP = baseMap.get(key.asMapKey());
        if (optionalKVP.isEmpty()) {
            return null;
        }
        return optionalKVP.get().value;
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

    @Override
    public Iterable<net.sf.saxon.ma.map.KeyValuePair> keyValuePairs() {
        return new IterableUsingIteratorSupplier<>(baseMap::valuesIterator);
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
        return new OrderedMap(baseMap.put(key.asMapKey(), new KeyValuePair(key, value)));
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
        return new OrderedMap(baseMap.remove(key.asMapKey()));
    }

}

