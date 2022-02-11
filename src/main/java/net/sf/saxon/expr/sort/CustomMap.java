package net.sf.saxon.expr.sort;

import net.sf.saxon.z.IntHashMap;

import java.util.ArrayList;
import java.util.List;

/**
 * A map implementation using a custom equality matcher, to avoid having to wrap
 * each entry in a wrapper object to achieve customised equality testing.
 *
 * <p>Note: this doesn't implement {@code Set<T>} to save the effort of implemented
 * methods like size() and iterator() that aren't required for our use cases. These
 * methods could easily be added without changing the basic design.
 * </p>
 */

public class CustomMap<K, V>  {

    private static class EntryChain<K, V> {
        public K key;
        public V value;
        public EntryChain<K, V> next;
        public EntryChain(K key, V value) {
            this.key = key;
            this.value = value;
            this.next = null;
        }
    }

    private final IntHashMap<EntryChain<K, V>> buckets;
    private final EqualityMatcher<K> equalityMatcher;

    public CustomMap(EqualityMatcher<K> matcher) {
        buckets = new IntHashMap<EntryChain<K, V>>();
        equalityMatcher = matcher;
    }

    /**
     * Add an entry to the map
     * @param key the new key
     * @param value the value to be added.
     * @return the previous value for the key if there was one, or null otherwise
     */
    public V put(K key, V value) {
        int h = equalityMatcher.hash(key);
        EntryChain<K, V> bucket = buckets.get(h);
        if (bucket == null) {
            EntryChain<K, V> entry = new EntryChain<>(key, value);
            buckets.put(h, entry);
            return null;
        } else {
            while (true) {
                if (equalityMatcher.equal(bucket.key, key)) {
                    V existing = bucket.value;
                    bucket.value = value;
                    return existing;
                } else if (bucket.next == null) {
                    bucket.next = new EntryChain<>(key, value);
                    return null;
                } else {
                    bucket = bucket.next;
                }
            }
        }
    }

    /**
     * Get the value associated with a given key, or null if absemt
     * @param key   the new key
     * @return the value for the key if there is one, or null otherwise
     */
    public V get(K key) {
        int h = equalityMatcher.hash(key);
        EntryChain<K, V> bucket = buckets.get(h);
        if (bucket == null) {
            return null;
        } else {
            while (true) {
                if (equalityMatcher.equal(bucket.key, key)) {
                    return bucket.value;
                } else if (bucket.next == null) {
                    return null;
                } else {
                    bucket = bucket.next;
                }
            }
        }
    }

    /**
     * Ask if a key exists in the set of keys
     * @return true if it exists
     */

    public boolean containsKey(K key) {
        int h = equalityMatcher.hash(key);
        EntryChain<K, V> bucket = buckets.get(h);
        if (bucket == null) {
            return false;
        } else {
            while (true) {
                if (equalityMatcher.equal(bucket.key, key)) {
                    return true;
                } else if (bucket.next == null) {
                    return false;
                } else {
                    bucket = bucket.next;
                }
            }
        }
    }

    /**
     * List all the keys.
     * @return a list of all the keys
     */
    public List<K> keys() {
        List<K> result = new ArrayList<>();
        for (EntryChain<K, V> entry : buckets.valueSet()) {
            EntryChain<K, V> e = entry;
            do {
                result.add(e.key);
                e = e.next;
            } while (e != null);
        }
        return result;
    }

    /**
     * List all the values.
     * @return a list of all the values
     */
    public List<V> values() {
        List<V> result = new ArrayList<>();
        for (EntryChain<K, V> entry : buckets.valueSet()) {
            EntryChain<K, V> e = entry;
            do {
                result.add(e.value);
                e = e.next;
            } while (e != null);
        }
        return result;
    }




}

