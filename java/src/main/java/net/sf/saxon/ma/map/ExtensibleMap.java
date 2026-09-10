package net.sf.saxon.ma.map;

import io.vavr.collection.HashMap;
import io.vavr.control.Option;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.ma.zeno.ZenoChain;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.value.AtomicValue;

import java.util.Iterator;
import java.util.NoSuchElementException;

/**
 * A general-purpose XDM map implementation that is ordered, that holds any kind of key,
 * and that supports incremental modification. Any duplicates added during map construction follow "use-last" semantics -
 * the new value replaces the old. This structure is therefore unsuited to cases where
 * duplicates need custom handling.
 *
 * <p>The implementation is similar to that of {@link AbstractFixedMap}, but the arrays of keys
 * and values are replaced by {@link ZenoChain}s to allow functional modification,
 * and the map from atomic match keys to integer offsets is replaced by an immutable
 * map (currently from the VAVR library).</p>
 *
 * <p>Because the integer offset of an entry within the ordered map is held in the index
 * structure and must therefore remain intact, the {@link #remove(AtomicValue)} operation
 * does not physically remove entries, but instead adds them to a list of "logically removed"
 * entries.</p>
 */

public class ExtensibleMap extends MapItem implements Iterable<KeyValuePair> {

    private final ZenoChain<AtomicValue> keys;
    private final ZenoChain<GroundedValue> values;
    private final io.vavr.collection.HashMap<AtomicMatchKey, Integer> index;
    private final io.vavr.collection.HashSet<Integer> removals;

    public ExtensibleMap() {
        this.keys = new ZenoChain<>();
        this.values = new ZenoChain<>();
        this.index = io.vavr.collection.HashMap.empty();
        this.removals = io.vavr.collection.HashSet.empty();
    }

    private ExtensibleMap(ZenoChain<AtomicValue> keys,
                          ZenoChain<GroundedValue> values,
                          io.vavr.collection.HashMap<AtomicMatchKey, Integer> index,
                          io.vavr.collection.HashSet<Integer> removals
                          ) {
        this.keys = keys;
        this.values = values;
        this.index = index;
        this.removals = removals;
    }

    /**
     * Construct an {@code ExtensibleMap} from the contents of an existing map
     *
     * @param map an existing map
     */

    public static ExtensibleMap copyOf(MapItem map) {
        ZenoChain<AtomicValue> keys = new ZenoChain<>();
        ZenoChain<GroundedValue> values = new ZenoChain<>();
        io.vavr.collection.HashMap<AtomicMatchKey, Integer> index = io.vavr.collection.HashMap.empty();
        io.vavr.collection.HashSet<Integer> removals = io.vavr.collection.HashSet.empty();
        int offset = 0;
        for (KeyValuePair pair : map.keyValuePairs()) {
            keys = keys.add(pair.key);
            values = values.add(pair.value);
            index = index.put(pair.key.asMapKey(), offset++);
        }
        return new ExtensibleMap(keys, values, index, removals);
    }

    /**
     * Get an entry from the Map
     *
     * @param key the value of the key
     * @return the value associated with the given key, or null if the key is not present in the map.
     */
    @Override
    public GroundedValue get(AtomicValue key) {
        Option<Integer> offset = index.get(key.asMapKey());
        if (offset.isEmpty() || removals.contains(offset.get())) {
            return null;
        }
        return values.get(offset.get());
    }

    /**
     * Get the offset of an entry in the map (that is, its 0-based position
     * in the map ordering
     * @param key the required key
     * @return the 0-based position of the relevant entry in the map, or -1
     * if the key is not present
     */

    public int getOffset(AtomicValue key) {
        Option<Integer> offset = index.get(key.asMapKey());
        if (offset.isEmpty() || removals.contains(offset.get())) {
            return -1;
        }
        return offset.get();
    }

    /**
     * Get the size of the map
     *
     * @return the number of keys/entries present in this map
     */
    @Override
    public int size() {
        return keys.size() - removals.size();
    }

    /**
     * Get the content as an iterable collection of key value pairs
     * @return an iterable collection of key value pairs
     */

    @Override
    public Iterable<KeyValuePair> keyValuePairs() {
        return this;
    }

    /**
     * Return an iterator of key-value pairs.
     * @return an Iterator of key-value pairs.
     */
    @Override
    public Iterator<KeyValuePair> iterator() {
        return new KeyValuePairIterator();
    }

    /**
     * Private inner class for iterating key-value pairs.
     */

    private class KeyValuePairIterator implements Iterator<KeyValuePair> {

        Iterator<AtomicValue> keyIter = keys.iterator();
        Iterator<GroundedValue> valueIter = values.iterator();
        int offset = 0;

        /**
         * Returns {@code true} if the iteration has more elements.
         * (In other words, returns {@code true} if {@link #next} would
         * return an element rather than throwing an exception.)
         *
         * @return {@code true} if the iteration has more elements
         */
        @Override
        public boolean hasNext() {
            while (removals.contains(offset)) {
                if (keyIter.hasNext()) {
                    keyIter.next();
                    valueIter.next();
                    offset++;
                } else {
                    return false;
                }
            }
            return keyIter.hasNext();
        }

        /**
         * Returns the next element in the iteration.
         *
         * @return the next element in the iteration
         * @throws NoSuchElementException if the iteration has no more elements
         */
        @Override
        public KeyValuePair next() {
            offset++;
            return new KeyValuePair(keyIter.next(), valueIter.next());
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
        int offset = getOffset(key);
        if (offset >= 0) {
            // Key is already present. Modify the values array, leave the keys and the index
            // unchanged.
            ZenoChain<GroundedValue> values2 = values.replace(offset, value);
            return new ExtensibleMap(keys, values2, index, removals);
        } else {
            // New key, add it at the end after all existing entries
            int last = keys.size();
            ZenoChain<AtomicValue> keys2 = keys.add(key);
            ZenoChain<GroundedValue> values2 = values.add(value);
            HashMap<AtomicMatchKey, Integer> index2 = index.put(key.asMapKey(), last);
            return new ExtensibleMap(keys2, values2, index2, removals);
        }

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
        int offset = getOffset(key);
        if (offset == -1) {
            return this;
        }
        ExtensibleMap result = new ExtensibleMap(keys, values, index, removals.add(offset));
        if (removals.size() * 4 > result.size()) {
            // rebuild the map, thus clearing the removals list
            return ExtensibleMap.copyOf(result);
        }
        return result;
    }

}

