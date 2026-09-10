package net.sf.saxon.ma.map;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.AtomicValue;

import java.util.ArrayList;
import java.util.BitSet;
import java.util.Iterator;
import java.util.List;

/**
 * Builder class for constructing ordered maps. The builder simply accumulates keys and values
 * until it is asked to construct and return a map with those keys and values. At that point it
 * checks for duplicate keys, and carries out various optimizations to work out what map implementation
 * class to use.
 */
public class GeneralMapBuilder {

    private final List<AtomicValue> keys = new ArrayList<>();
    private final List<GroundedValue> values = new ArrayList<>();

    /**
     * Callback function to combine two values (for duplicate keys) into one. If
     * null, duplicates are rejected as an error.
     */
    private MapFunctionSet.OnDuplicatesAction combiner;

    private boolean allStrings = true;

    // TODO: this design is not ideal for the case where there are many duplicates. Perhaps we should
    //  start building the final data structure as soon as we hit a potential duplicate while updating
    //  the Bloom filter.

    public GeneralMapBuilder() {

    }

    /**
     * Set a combiner function for combining entries with duplicate keys. If no combiner
     * is set, duplicate keys result in a dynamic error.
     * @param combiner a function that will be called with two grounded values (representing
     *                 two values for the same key) and returns a single grounded value to
     *                 be used in the result map.
     */

    public void setDuplicatesAction(MapFunctionSet.OnDuplicatesAction combiner) {
        this.combiner = combiner;
    }

    /**
     * Add a key-value pair
     *
     * @param key   the key
     * @param value the corresponding value
     */

    public void put(AtomicValue key, GroundedValue value) {
        keys.add(key);
        values.add(value);
        if (allStrings && key.getItemType() != BuiltInAtomicType.STRING) {
            allStrings = false;
        }
    }

    /**
     * Copy all the keys and values from an existing map
     * @param map the map to be copied
     */

    public void copy(MapItem map) {
        for (KeyValuePair pair : map.keyValuePairs()) {
            put(pair.key, pair.value);
        }
    }

    /**
     * Get the constructed map. This method must only be called once, because the
     * lists of keys and values are used within the constructed map and are mutable.
     * This method cannot be called if there is a combiner function, because calling
     * the function potentially requires a run-time context.
     *
     * @return the constructed map
     * @throws XPathException if duplicate keys were found, and not detected earlier
     */

    public MapItem getCompletedMap() throws XPathException {
        if (combiner != null) {
            throw new AssertionError("needs XPathContext");
        }
        return getCompletedMap(null);
    }

    /**
     * Get the constructed map. This method must only be called once, because the
     * lists of keys and values are used within the constructed map and are mutable.
     * @param context The dynamic context: needed only when calling a combiner function
     *                to combine duplicate entries
     * @return the constructed map
     * @throws XPathException if duplicate keys were found, and not detected earlier
     */

    public MapItem getCompletedMap(XPathContext context) throws XPathException {
        if (keys.size() == 0) {
            return EmptyMap.getInstance();
        } else if (keys.size() == 1) {
            return new SingleEntryMap(keys.get(0), values.get(0));
        }
        // quick scan for possible duplicates using a bloom filter
        boolean possibleDuplicates = hasPossibleDuplicates(keys);
        if (possibleDuplicates) {
            // If there are possible duplicates, then we build the map now, with a more thorough
            // check for duplicates as we build it.

            MapItem map = new OrderedMap();
            Iterator<GroundedValue> valueIter = values.iterator();
            for (AtomicValue key : keys) {
                GroundedValue value;
                if (valueIter.hasNext()) {
                    value = valueIter.next();
                } else {
                    throw new AssertionError();
                }
                GroundedValue existing = map.get(key);
                if (existing == null) {
                    map = map.put(key, value);
                } else if (combiner == null) {
                    throw new XPathException("Duplicate key " + Err.depict(key) + " in map");
                } else {
                    GroundedValue newVal = combiner.combine(existing, value, context);
                    map = map.put(key, newVal);
                }
            }
            return map;
        } else {
            return getCompletedMapConfidently();
        }


    }

    /**
     * Get the constructed map, knowing that there are no duplicate keys.
     * This method must only be called once, because the
     * lists of keys and values are used within the constructed map and are mutable.
     *
     * @return the constructed map
     */

    public MapItem getCompletedMapConfidently() {
        if (keys.size() == 0) {
            return EmptyMap.getInstance();
        } else if (keys.size() == 1) {
            return new SingleEntryMap(keys.get(0), values.get(0));
        } else if (allStrings) {
            List<UnicodeString> stringKeys = new ArrayList<>(keys.size());
            for (AtomicValue k : keys) {
                stringKeys.add(k.getUnicodeStringValue());
            }
            return new LazyStringMap(stringKeys, values);
        } else {
            MapItem map = new OrderedMap();
            Iterator<GroundedValue> valueIter = values.iterator();
            for (AtomicValue key : keys) {
                GroundedValue value;
                if (valueIter.hasNext()) {
                    value = valueIter.next();
                } else {
                    throw new AssertionError();
                }
                map = map.put(key, value);
            }
            return map;
        }
    }


    private static boolean hasPossibleDuplicates(List<AtomicValue> keys) {
        int filterSize = keys.size() < 50 ? 1024 : 65536;
        int mask = filterSize - 1;
        BitSet bloom = new BitSet(filterSize);
        for (AtomicValue key : keys) {
            AtomicMatchKey matchKey = key.asMapKey();
            long hash = matchKey.longHashCode();
            int hash0 = (int) (hash >> 40 & mask);
            int hash1 = (int) (hash >> 20 & mask);
            int hash2 = (int) hash & mask;
            if (bloom.get(hash0) && bloom.get(hash1) && bloom.get(hash2)) {
                return true;
            }
            bloom.set(hash0);
            bloom.set(hash1);
            bloom.set(hash2);
        }
        return false;
    }

}

