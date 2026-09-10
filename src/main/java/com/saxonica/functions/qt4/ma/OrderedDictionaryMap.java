package com.saxonica.functions.qt4.ma;

import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapBuilder;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.value.AtomicValue;

import java.util.LinkedHashMap;

/**
 * A subclass of DictionaryMap that retains the order of keys. This class only handles
 * keys annotated as xs:string, and it cannot handle incremental modification: any attempt
 * to modify the map (using map:put or map:remove) requires the data to be copied to a more
 * general implementation. The primary use case for this class is therefore when
 * parsing JSON with the option retain-order=true.
 */

public class OrderedDictionaryMap extends DictionaryMap {

    protected static class OrderedDictionaryMapBuilder extends DictionaryMap.DictionaryMapBuilder {

        public OrderedDictionaryMapBuilder() {
            super(new LinkedHashMap<String, GroundedValue>());
        }

        public OrderedDictionaryMapBuilder(int initialSize) {
            super(new LinkedHashMap<String, GroundedValue>(initialSize));
        }

        /**
         * Get the constructed map. This method must only be called once; subsequent calls will return null
         *
         * @return the constructed map
         */
        @Override
        public DictionaryMap getCompletedMap() {
            OrderedDictionaryMap result = new OrderedDictionaryMap((LinkedHashMap<String, GroundedValue>)map);
            map = null;
            return result;
        }
    }

    protected OrderedDictionaryMap(LinkedHashMap<String, GroundedValue> map) {
        super(map);
    }

    /**
     * Get a builder for this kind of map
     * @return a suitable builder. Note that this will only accept string-valued keys.
     */
    public static MapBuilder getBuilder() {
        return new OrderedDictionaryMapBuilder();
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
        return toGeneralMap().addEntry(key, value);
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

    /**
     * Convert to an OrderedMap
     */

    private OrderedMap toGeneralMap() {
        //System.err.println("Dictionary rewrite!!!!");
        MapBuilder target = OrderedMap.getBuilder();
        for (KeyValuePair kvp : keyValuePairs()) {
            target.put(kvp.key, kvp.value);
        }
        return (OrderedMap) target.getCompletedMap();
    }

}

