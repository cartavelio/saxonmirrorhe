package net.sf.saxon.ma.map;

import io.vavr.Tuple2;
import io.vavr.collection.LinkedHashMap;
import io.vavr.control.Option;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.transpile.CSharpSuppressWarnings;
import net.sf.saxon.tree.iter.AtomicIterator;
import net.sf.saxon.value.AtomicValue;

import java.util.Iterator;

/**
 * An XDM Map implementation that maintains map entries in insertion order (new entries at the end)
 */
public class OrderedMap extends MapTypeMaintainer implements BuildableMap {

    private final LinkedHashMap<AtomicValue, GroundedValue> baseMap;

    public OrderedMap() {
        baseMap = LinkedHashMap.empty();

    }

    private OrderedMap(LinkedHashMap<AtomicValue, GroundedValue> baseMap) {
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
        Option<GroundedValue> val = baseMap.get(key);
        return (val.isEmpty() ? null : val.get());
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
        return new AtomicIterator() {

            final io.vavr.collection.Iterator<Tuple2<AtomicValue, GroundedValue>> baseIter = baseMap.iterator();

            @Override
            @CSharpSuppressWarnings("UnsafeIteratorConversion")
            public AtomicValue next() {
                if (baseIter.hasNext()) {
                    return baseIter.next()._1;
                } else {
                    return null;
                }
            }
        };

    }

    /**
     * Get the set of all key-value pairs in the map
     *
     * @return an iterable containing all the key-value pairs
     */
    @Override
    public Iterable<KeyValuePair> keyValuePairs() {
        return new Iterable<KeyValuePair>() {
            @Override
            @CSharpSuppressWarnings("UnsafeIteratorConversion")
            public Iterator<KeyValuePair> iterator() {
                return new Iterator<>() {
                    final Iterator<Tuple2<AtomicValue, GroundedValue>> baseIter = baseMap.iterator();

                    @Override
                    public boolean hasNext() {
                        return baseIter.hasNext();
                    }

                    @Override
                    public KeyValuePair next() {
                        Tuple2<AtomicValue, GroundedValue> next = baseIter.next();
                        return new KeyValuePair(next._1, next._2);
                    }

                    @Override
                    public void remove() {
                        baseIter.remove();
                    }
                };
            }
        };
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
        return new OrderedMap(baseMap.put(key, value));
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
        return new OrderedMap(baseMap.remove(key));
    }

}

