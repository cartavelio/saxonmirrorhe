package net.sf.saxon.ma.map;

import io.vavr.Tuple2;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.tree.jiter.MappingJavaIterator;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.UType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;

import java.util.Iterator;

/**
 * An implementation of XDM maps for use when all keys are strings, and when the map is ordered.
 */

public class OrderedStringMap extends StringMapItem {

    final io.vavr.collection.LinkedHashMap<UnicodeString, GroundedValue> baseMap;

    public OrderedStringMap() {
        baseMap = io.vavr.collection.LinkedHashMap.empty();
    }

    public OrderedStringMap(io.vavr.collection.LinkedHashMap<UnicodeString, GroundedValue> baseMap) {
        this.baseMap = baseMap;
    }

    @Override
    public GroundedValue get(AtomicValue key) {
        if (key instanceof StringValue) {
            return getU(key.getUnicodeStringValue());
        } else {
            return null;
        }
    }

    @Override
    public int size() {
        return baseMap.size();
    }

    @Override
    public Iterable<KeyValuePair> keyValuePairs() {
        return new Iterable<KeyValuePair>() {
            @Override
            public Iterator<KeyValuePair> iterator() {
                Iterator<Tuple2<UnicodeString, GroundedValue>> baseIter = baseMap.iterator();
                return new MappingJavaIterator<>(baseIter,
                                                 t2 -> new KeyValuePair(
                                                         new StringValue(t2._1()), t2._2()));
            }
        };
    }


    @Override
    public GroundedValue getU(UnicodeString key) {
        return baseMap.get(key).orNull();
    }

    @Override
    public boolean containsU(UnicodeString key) {
        return baseMap.containsKey(key);
    }

    @Override
    public OrderedStringMap putU(UnicodeString key, GroundedValue value) {
        return new OrderedStringMap(baseMap.put(key, value));
    }

    @Override
    public StringMapItem removeU(UnicodeString key) {
        return new OrderedStringMap(baseMap.remove(key));
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
        if (key.getItemType() == BuiltInAtomicType.STRING) {
            return putU(key.getUnicodeStringValue(), value);
        }
        return OrderedMap.copy(this).put(key, value);
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
        if (key.getUType().overlaps(UType.STRING_LIKE)) {
            return removeU(key.getUnicodeStringValue());
        } else {
            return this;
        }
    }

}

