package net.sf.saxon.ma.map;

import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.value.AtomicValue;

import java.util.HashMap;

public class FixedMapWithAnyKeys extends FixedMap {

    private final AtomicValue[] keys;

    @Override
    protected AtomicValue getKey(int offset) {
        return keys[offset];
    }

    /**
     * Construct a {@code FixedMap} supplying the entries as an array
     * of keys and a corresponding array of value. The caller is responsible
     * for ensuring that there are no duplicate keys. The two arrays (of keys and values)
     * must have the same length. The content of the arrays must not be subsequently
     * modified.
     *
     * @param keys   an array of keys
     * @param values a corresponding array of values
     * @param index  a hash map indexing the values
     */

    protected FixedMapWithAnyKeys(AtomicValue[] keys, GroundedValue[] values, HashMap<AtomicMatchKey, Integer> index) {
        assert keys.length == values.length;
        this.keys = keys;
        this.values = values;
        this.index = index;
    }


}


