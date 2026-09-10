package net.sf.saxon.ma.map;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.tree.iter.AtomicIterator;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

/**
 * A {@code ShapedMap} is an implementation of XDM maps optimised for the case
 * where many maps have the same set of string-valued keys.
 *
 * <p>It has the additional property that the keys() method delivers the keys
 * in a consistent order.</p>
 */

public class ShapedMap extends StringMap {

    private final GroundedValue[] values;
    private final Shape shape;

    /**
     * Create an instance of a {@code ShapedMap} whose structure is defined
     * by the supplied {@link Shape}
     * @param shape the shape of the map, identifying the keys that are present
     * @param values the values to appear in the map, corresponding one-to-one
     *               with the keys that are defined in the {@link Shape}
     */
    public ShapedMap(Shape shape, GroundedValue... values) {
        if (values.length != shape.size()) {
            throw new IllegalArgumentException();
        }
        this.shape = shape;
        this.values = values;
    }

    public void initialPut(int slot, GroundedValue value) {
        values[slot] = value;
    }

    /**
     * Get an entry from the Map
     *
     * @param key the value of the key
     * @return the value associated with the given key, or null if the key is not present in the map.
     */
    @Override
    public GroundedValue get(AtomicValue key) {
        if (key instanceof StringValue) {
            return get(key.getUnicodeStringValue());
        }
        return null;
    }

    /**
     * Get an entry from the map, supplying a Java String as the key
     *
     * @param key the key
     * @return the relevant entry if present, or null if absent.
     */
    @Override
    public GroundedValue get(String key) {
        return get(StringView.of(key));
    }

    /**
     * Get an entry from the map, supplying a {@link UnicodeString} as the key
     *
     * @param key the key
     * @return the relevant entry if present, or null if absent.
     */
    @Override
    public GroundedValue get(UnicodeString key) {
        int slot = shape.getSlot(key);
        if (slot >= 0) {
            return values[slot];
        }
        return null;
    }

    /**
     * Ask whether a given string is present as a key in the map
     *
     * @param key the key being tested
     * @return true if the key is present
     */
    @Override
    public boolean contains(String key) {
        return get(key) != null;
    }

    /**
     * Ask whether a given string is present as a key in the map
     *
     * @param key the key being tested
     * @return true if the key is present
     */
    @Override
    public boolean contains(UnicodeString key) {
        return get(key) != null;
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

    /**
     * Get the set of all key values in the map.
     *
     * @return a set containing all the key values present in the map. Normally the order
     * is unpredictable, but for a {@code ShapedMap} the order is defined by the {@link Shape}.
     */
    @Override
    public AtomicIterator keys() {
        return shape.iterateKeys();
    }

    /**
     * Get the set of all key-value pairs in the map
     *
     * @return an iterable containing all the key-value pairs
     */
    @Override
    public Iterable<KeyValuePair> keyValuePairs() {
        int length = size();
        List<KeyValuePair> list = new ArrayList<>(length);
        for (int i=0; i<length; i++) {
            list.add(new KeyValuePair(new StringValue(shape.getKey(i)), values[i]));
        }
        return list;
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
        if (key instanceof StringValue && size() < 20) {
            int slot = shape.getSlot(key.getUnicodeStringValue());
            if (slot >= 0) {
                GroundedValue[] v2 = Arrays.copyOf(values, values.length);
                v2[slot] = value;
                return new ShapedMap(shape, v2);
            }
        }
        return HashTrieMap.copy(this).addEntry(key, value);
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
        return HashTrieMap.copy(this).remove(key);
    }

    /**
     * Ask whether the map conforms to a given map type
     *
     * @param keyType   the required keyType
     * @param valueType the required valueType
     * @param th        the type hierarchy cache for the configuration
     * @return true if the map conforms to the required type
     */
    @Override
    public boolean conforms(PlainType keyType, SequenceType valueType, TypeHierarchy th) {
        if (!th.isSubType(BuiltInAtomicType.STRING, keyType)) {
            return false;
        }
        for (int i=0; i<size(); i++) {
            if (!valueType.matches(values[i], th)) {
                return false;
            }
        }
        return true;
    }

    /**
     * Get the type of the map. This method is used largely for diagnostics, to report
     * the type of a map when it differs from the required type.
     *
     * @param th the type hierarchy cache
     * @return the type of this map
     */
    @Override
    public ItemType getItemType(TypeHierarchy th) {
        ItemType valueType = null;
        int valueCard = 0;
        // we need to test the entries individually
        for (GroundedValue val : values) {
            if (valueType == null) {
                valueType = SequenceTool.getItemType(val, th);
                valueCard = SequenceTool.getCardinality(val);
            } else {
                valueType = Type.getCommonSuperType(valueType, SequenceTool.getItemType(val, th), th);
                valueCard = Cardinality.union(valueCard, SequenceTool.getCardinality(val));
            }
        }
        if (valueType == null) {
            // empty map
            return MapType.EMPTY_MAP_TYPE;
        } else {
            return new MapType(BuiltInAtomicType.STRING, SequenceType.makeSequenceType(valueType, valueCard));
        }
    }

    /**
     * Get the lowest common item type of the keys in the map
     *
     * @return the most specific type to which all the keys belong. If the map is
     * empty, return UType.VOID
     */
    @Override
    public UType getKeyUType() {
        return UType.STRING;
    }


}

