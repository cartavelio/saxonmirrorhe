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
        this.slotMap = new HashMap<>();
        for (int i=0; i<keys.length; i++) {
            slotMap.put(keys[i], i);
        }
    }

    public int size() {
        return keys.length;
    }

    public int getSlot(UnicodeString key) {
        return slotMap.getOrDefault(key, -1);
    }

    public UnicodeString getKey(int slot) {
        return keys[slot];
    }

    public AtomicIterator iterateKeys() {
        return new KeyIterator(keys);
    }

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

