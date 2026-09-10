package net.sf.saxon.ma.map;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.tree.jiter.MappingJavaIterator;
import net.sf.saxon.value.AtomicValue;

import java.util.Iterator;

/**
 * Handles a simple delta view of a map with one entry added. This defers the potentially
 * high cost of copying a fixed map to an extensible map when only one entry is being added.
 * It is shamelessly motivated by the use case in test same-key-023
 */

public class MapReplacingOne extends MapItem implements Iterable<KeyValuePair>{

    private final MapItem baseMap;
    private final AtomicValue extraKey;
    private final GroundedValue extraValue;

    public MapReplacingOne(MapItem baseMap, AtomicValue extraKey, GroundedValue extraValue) {
        this.baseMap = baseMap;
        this.extraKey = extraKey;
        this.extraValue = extraValue;
    }

    /**
     * Get an entry from the Map
     *
     * @param key the value of the key
     * @return the value associated with the given key, or null if the key is not present in the map.
     */
    @Override
    public GroundedValue get(AtomicValue key) {
        if (key.asMapKey().equals(extraKey.asMapKey())) {
            return extraValue;
        } else {
            return baseMap.get(key);
        }
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
     * Get the set of all key-value pairs in the map
     *
     * @return an iterable containing all the key-value pairs
     */
    @Override
    public Iterable<KeyValuePair> keyValuePairs() {
        return this;
    }

    /**
     * Returns an iterator over elements of type {@code T}.
     *
     * @return an Iterator.
     */
    @Override
    public Iterator<KeyValuePair> iterator() {
        Iterator<KeyValuePair> baseIter = baseMap.keyValuePairs().iterator();
        java.util.function.Function<KeyValuePair, KeyValuePair> filter =
                (kvp) -> kvp.key.asMapKey().equals(extraKey.asMapKey()) ? new KeyValuePair(extraKey, extraValue) : kvp;
        return new MappingJavaIterator<>(baseIter, filter);
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
        return ExtensibleMap.copyOf(this).put(key, value);
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
        return ExtensibleMap.copyOf(this).remove(key);
    }
}


