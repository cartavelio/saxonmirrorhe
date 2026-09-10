package net.sf.saxon.ma.map;

import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.value.AtomicValue;

import java.util.HashMap;
import java.util.Map;

/**
 * A general-purpose XDM map implementation that is unordered, that holds any kind of key,
 * and that supports incremental modification only by copying the whole map to a different
 * structure. Any duplicates added during map construction follow "use-last" semantics -
 * the new value replaces the old. This structure is therefore unsuited to cases where
 * duplicates need custom handling.
 *
 * <p>The implementation is based on a mutable Java {@code java.util.HashMap} (but encapsulated
 * so no mutation can take place after the map has been built). If put or remove operations
 * are subsequently attempted, the map is copied to a different implementation structure
 * that supports functional modification.</p>
 *
 * <p>The underlying implementation uses an instance of {@code HashMap<AtomicMatchKey, KeyValuePair>}
 * For many data types this means that the key is held twice, once as the key, and once as part
 * of the value. The reason for this is that the <code>keys()</code> function needs to return
 * the original keys as supplied, complete with type annotation, which would not (always) have
 * the same equality semantics. The {@code AtomicMatchKey} is computed from the actual key
 * and in some cases is simply a wrapper with a different <code>equals()</code> method.</p>
 */
public class SimpleMap extends MapItem {

    private static class SimpleMapBuilder implements MapBuilder {

        private final Map<AtomicMatchKey, KeyValuePair> baseMap = new HashMap<>();

        public SimpleMapBuilder() {}

        @Override
        public void put(AtomicValue key, GroundedValue value) {
            baseMap.put(key.asMapKey(), new KeyValuePair(key, value));
        }

        @Override
        public MapItem getCompletedMap() {
            return new SimpleMap(baseMap);
        }
    }

    private final Map<AtomicMatchKey, KeyValuePair> baseMap;

    /**
     * Factory method to get a MapBuilder which can be used to construct an instance
     * of a {@link SimpleMap}
     * @return a class for building simple maps
     */
    public static MapBuilder getBuilder() {
        return new SimpleMapBuilder();
    }

    /**
     * Private constructor: the class can only be instantiated by using a map builder
     * @param baseMap the underlying Java HashMap
     */
    private SimpleMap(Map<AtomicMatchKey, KeyValuePair> baseMap) {
        this.baseMap = baseMap;
    }

    @Override
    public GroundedValue get(AtomicValue key) {
        KeyValuePair kvp = baseMap.get(key.asMapKey());
        if (kvp == null) {
            return null;
        }
        return kvp.value;
    }

    @Override
    public int size() {
        return baseMap.size();
    }

    @Override
    public Iterable<KeyValuePair> keyValuePairs() {
        return baseMap.values();
    }

    /**
     * Add a new entry, returning a new map. This performs a bulk copy
     * to a different data structure
     * @param key   the key of the new entry
     * @param value the value associated with the new entry
     * @return a map with the added entry
     */
    @Override
    public MapItem put(AtomicValue key, GroundedValue value) {
        return makeFunctionalMap().put(key, value);
    }

    private MapItem makeFunctionalMap() {
        return OrderedMap.copy(this);
    }

    /**
     * Remove an entry, returning a new map. This performs a bulk copy
     * to a different data structure
     * @param key     the key of the entry to be removed
     * @return the map with the selected entry removed
     */

    @Override
    public MapItem remove(AtomicValue key) {
        if (baseMap.containsKey(key.asMapKey())) {
            return makeFunctionalMap().remove(key);
        } else {
            return this;
        }
    }

}

