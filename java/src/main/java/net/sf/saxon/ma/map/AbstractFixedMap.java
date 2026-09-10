package net.sf.saxon.ma.map;

import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;

import java.util.HashMap;
import java.util.Iterator;
import java.util.NoSuchElementException;

/**
 * A general-purpose XDM map implementation that is ordered, that holds any kind of key,
 * and that supports incremental modification only by copying the whole map to a different
 * structure. Any duplicates added during map construction follow "use-last" semantics -
 * the new value replaces the old. This structure is therefore unsuited to cases where
 * duplicates need custom handling.
 *
 * <p>The implementation is based on an array of keys, an array of corresponding values,
 * and a mutable Java {@code java.util.HashMap} (but encapsulated
 * so no mutation can take place after the map has been built) that maps atomic match keys
 * to integer positions in the array. If put or remove operations
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

public abstract class FixedMap extends MapItem implements Iterable<KeyValuePair> {

    protected GroundedValue[] values;
    protected HashMap<AtomicMatchKey, Integer> index;

    private static class FixedMapBuilder extends GeneralMapBuilder {

        public FixedMapBuilder() {
        }

        @Override
        public MapItem getCompletedMap() throws XPathException {
            AtomicValue[] keyArray = keys.toArray(new AtomicValue[0]);
            GroundedValue[] valueArray = values.toArray(new GroundedValue[0]);
            HashMap<AtomicMatchKey, Integer> index = new HashMap<>(keyArray.length);
            for (int i = 0; i < keyArray.length; i++) {
                Integer prev = index.put(keyArray[i].asMapKey(), i);
                if (prev != null) {
                    throw new XPathException("Duplicate key " + Err.depict(keyArray[i]) + " in map", "FOJS0003");
                }
            }
            return new FixedMapWithAnyKeys(keyArray, valueArray, index);
        }

        /**
         * Get the constructed map, knowing that there are no duplicate keys.
         *
         * @return the constructed map
         */

        @Override
        public MapItem getCompletedMapConfidently() {
            AtomicValue[] keyArray = keys.toArray(new AtomicValue[0]);
            GroundedValue[] valueArray = values.toArray(new GroundedValue[0]);
            HashMap<AtomicMatchKey, Integer> index = new HashMap<>(keyArray.length);
            for (int i = 0; i < keyArray.length; i++) {
                index.put(keyArray[i].asMapKey(), i);
            }
            return new FixedMapWithAnyKeys(keyArray, valueArray, index);
        }
    }

    /**
     * Factory method to get a MapBuilder which can be used to construct an instance
     * of a {@link FixedMap}
     *
     * @return a class for building fixed maps
     */
    public static GeneralMapBuilder getBuilder() {
        return new FixedMap.FixedMapBuilder();
    }

    /**
     * Get an entry from the Map
     *
     * @param key the value of the key
     * @return the value associated with the given key, or null if the key is not present in the map.
     */
    @Override
    public GroundedValue get(AtomicValue key) {
        ensureIndex();
        Integer offset = index.get(key.asMapKey());
        if (offset == null) {
            return null;
        }
        return values[offset];
    }

    public int getIndex(AtomicValue key) {
        ensureIndex();
        Integer offset = index.get(key.asMapKey());
        if (offset == null) {
            return -1;
        }
        return offset;
    }

    protected void ensureIndex() {
        if (index == null) {
            index = new HashMap<>(size());
            for (int i = 0; i < size(); i++) {
                index.put(getKey(i).asMapKey(), i);
            }
        }
    }

    /**
     * Get the size of the map
     *
     * @return the number of keys/entries present in this map
     */
    @Override
    public int size() {
        return values.length;
    }

    protected abstract AtomicValue getKey(int offset);

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

    private class KeyValuePairIterator implements Iterator<KeyValuePair> {

        int position = 0;

        /**
         * Returns {@code true} if the iteration has more elements.
         * (In other words, returns {@code true} if {@link #next} would
         * return an element rather than throwing an exception.)
         *
         * @return {@code true} if the iteration has more elements
         */
        @Override
        public boolean hasNext() {
            return position < values.length;
        }

        /**
         * Returns the next element in the iteration.
         *
         * @return the next element in the iteration
         * @throws NoSuchElementException if the iteration has no more elements
         */
        @Override
        public KeyValuePair next() {
            if (position >= values.length) {
                throw new NoSuchElementException();
            }
            KeyValuePair kvp = new KeyValuePair(getKey(position), values[position]);
            position++;
            return kvp;
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

