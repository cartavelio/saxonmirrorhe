package net.sf.saxon.ma.map;

import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.ma.trie.ImmutableHashTrieMap;
import net.sf.saxon.ma.zeno.ZenoChain;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.transpile.CSharpReplaceBody;
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

    private final ZenoChain<AtomicValue> keyList;
    private final ZenoChain<GroundedValue> valueList;
    private final ImmutableHashTrieMap<AtomicMatchKey, Integer> index;
    private final ImmutableIntSet removals;

    public ExtensibleMap() {
        this.keyList = new ZenoChain<>();
        this.valueList = new ZenoChain<>();
        this.index = makeEmptyIndex();
        this.removals = new ImmutableIntSet();
    }

    private ExtensibleMap(ZenoChain<AtomicValue> keys,
                          ZenoChain<GroundedValue> values,
                          ImmutableHashTrieMap<AtomicMatchKey, Integer> index,
                          ImmutableIntSet removals
                          ) {
        this.keyList = keys;
        this.valueList = values;
        this.index = index;
        this.removals = removals;
    }

    @CSharpReplaceBody(code="return System.Collections.Immutable.ImmutableDictionary.Create<Saxon.Hej.expr.sort.AtomicMatchKey,int>();")
    private static ImmutableHashTrieMap<AtomicMatchKey, Integer> makeEmptyIndex() {
        return ImmutableHashTrieMap.empty();
    }

    /**
     * Construct an {@code ExtensibleMap} from the contents of an existing map
     *
     * @param map an existing map
     */

    public static ExtensibleMap copyOf(MapItem map) {
        ZenoChain<AtomicValue> keys = new ZenoChain<>();
        ZenoChain<GroundedValue> values = new ZenoChain<>();
        ImmutableHashTrieMap<AtomicMatchKey, Integer> index = makeEmptyIndex();
        ImmutableIntSet removals = new ImmutableIntSet();
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
        Integer offset = index.get(key.asMapKey());
        if (offset == null || removals.contains(offset)) {
            return null;
        }
        return valueList.get(offset);
    }

    /**
     * Get the offset of an entry in the map (that is, its 0-based position
     * in the map ordering
     * @param key the required key
     * @return the 0-based position of the relevant entry in the map, or -1
     * if the key is not present
     */

    public int getOffset(AtomicValue key) {
        Integer offset = index.get(key.asMapKey());
        if (offset == null || removals.contains(offset)) {
            return -1;
        }
        return offset;
    }

    /**
     * Get the size of the map
     *
     * @return the number of keys/entries present in this map
     */
    @Override
    public int size() {
        return keyList.size() - removals.size();
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
        return new KeyValuePairIterator(keyList, valueList, removals);
    }

    /**
     * Private inner class for iterating key-value pairs. Implemented as a static class
     * for ease of C# transpilation
     */

    private static class KeyValuePairIterator implements Iterator<KeyValuePair> {

        private final ImmutableIntSet removals;
        private final Iterator<AtomicValue> keyIter;
        private final Iterator<GroundedValue> valueIter;
        private int offset = 0;

        public KeyValuePairIterator(ZenoChain<AtomicValue> keyList,
                                    ZenoChain<GroundedValue> valueList,
                                    ImmutableIntSet removals) {
            this.removals = removals;
            this.keyIter = keyList.iterator();
            this.valueIter = valueList.iterator();
        }
        

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
                if (keyIter.hasNext() && valueIter.hasNext()) {
                    // redundant code for transpilability...
                    AtomicValue a = keyIter.next();
                    GroundedValue v = valueIter.next();
                    offset++;
                } else {
                    return false;
                }
            }
            return keyIter.hasNext() && valueIter.hasNext();
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
            ZenoChain<GroundedValue> values2 = valueList.replace(offset, value);
            return new ExtensibleMap(keyList, values2, index, removals);
        } else {
            // New key, add it at the end after all existing entries
            int last = keyList.size();
            ZenoChain<AtomicValue> keys2 = keyList.add(key);
            ZenoChain<GroundedValue> values2 = valueList.add(value);
            ImmutableHashTrieMap<AtomicMatchKey, Integer> index2 = index.put(key.asMapKey(), last);
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
        ExtensibleMap result = new ExtensibleMap(keyList, valueList, index, removals.add(offset));
        if (removals.size() * 4 > result.size()) {
            // rebuild the map, thus clearing the removals list
            return ExtensibleMap.copyOf(result);
        }
        return result;
    }

}

