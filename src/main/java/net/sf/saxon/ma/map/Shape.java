package net.sf.saxon.ma.map;

import net.sf.saxon.str.Twine8;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.tree.iter.AtomicIterator;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;

import java.util.HashMap;

/**
 * A {@code Shape} represents the common structure of a set of {@code ShapedMap} objects,
 * each being a map with the same set of keys but different values. A {@code Shape} is essentially
 * a mapping from keys to integer slot positions.}
 */
public class Shape {

    private final UnicodeString[] keys;
    private final HashMap<UnicodeString, Integer> slotMap;

    public final static Shape KEY_VALUE_PAIR = new Shape(new Twine8("key"), new Twine8("value"));

    public Shape(UnicodeString... keys) {
        this.keys = keys;
        this.slotMap = new HashMap<>(keys.length);
        for (int i=0; i<keys.length; i++) {
            slotMap.put(keys[i], i);
        }
    }

    /**
     * Get the number of keys in the Shape
     * @return the number of distinct keys
     */
    public int size() {
        return keys.length;
    }

    /**
     * Get the integer slot number allocated to a particular key, if any
     * @param key the relevant key
     * @return the slot number used for the specified key, or -1 if the key is not recognised
     */
    public int getSlot(UnicodeString key) {
        return slotMap.getOrDefault(key, -1);
    }

    /**
     * Get the key associated with a particular slot number
     * @param slot the slot number
     * @return the associated key value
     */
    public UnicodeString getKey(int slot) {
        return keys[slot];
    }

    /**
     * Return an iterator over the keys in this Shape
     * @return an iterator over the keys, retaining the order in which they were originally supplied
     */
    public AtomicIterator iterateKeys() {
        return new KeyIterator(keys);
    }

    /**
     * Inner class to iterate over the keys
     */
    private static class KeyIterator implements AtomicIterator {
        final private UnicodeString[] keys;
        private int position = 0;
        public KeyIterator(UnicodeString[] keys) {
            this.keys = keys;
        }

        /**
         * Get the next atomic value in the sequence. <BR>
         *
         * @return the next Item. If there are no more items, return null.
         */
        @Override
        public AtomicValue next() {
            return position >= keys.length ? null : new StringValue(keys[position++]);
        }
    }
}

