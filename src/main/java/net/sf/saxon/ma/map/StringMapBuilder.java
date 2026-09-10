package net.sf.saxon.ma.map;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trace.Instrumentation;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.BitSet;
import java.util.Iterator;
import java.util.List;

/**
 * Builder class for constructing ordered maps, where the keys are all known in advance to have
 * a type annotation of xs:string (so we don't need to retain the type annotation). The
 * builder simply accumulates keys and values
 * until it is asked to construct and return a map with those keys and values. At that point it
 * checks for duplicate keys, and carries out various optimizations to work out what map implementation
 * class to use.
 */
public class StringMapBuilder {

    private final List<UnicodeString> keys;
    private final List<GroundedValue> values;

    /**
     * Callback function to combine two values (for duplicate keys) into one. If
     * null, duplicates are rejected as an error.
     */
    private MapFunctionSet.OnDuplicatesAction combiner;

    /**
     * XPath dynamic context for evaluating the combiner function. Needed if and only
     * if the combiner invokes a user-defined XDM function
     */

    private XPathContext context;

    private boolean ordered = true;



    public StringMapBuilder() {
        keys = new ArrayList<>();
        values = new ArrayList<>();
    }

    public StringMapBuilder(int size) {
        keys = new ArrayList<>(size);
        values = new ArrayList<>(size);
    }

    /**
     * Supply a combiner function for combining entries with duplicate keys. If no combiner
     * is set, duplicate keys result in a dynamic error.
     * @param combiner a function that will be called with two grounded values (representing
     *                 two values for the same key) and returns a single grounded value to
     *                 be used in the result map.
     */

    public void setCombiner(MapFunctionSet.OnDuplicatesAction combiner) {
        this.combiner = combiner;
    }

    /**
     * Supply a dynamic evaluation context for evaluating the combiner function. Needed if
     * and only if the combiner calls a user-defined XDM function
     * @param context the XPath evaluation context
     */

    public void setContext(XPathContext context) {
        this.context = context;
    }

    /**
     * Say whether the required map should retain ordering. In general the 4.0 specification
     * requires maps to retain order. However, for maps constructed and used internally, order
     * is often not relevant, and Saxon will provide options for user-created maps to be
     * unordered. Maps are ordered by default
     * @param ordered true if the map is to be ordered, false if not.
     */

    public void setOrdered(boolean ordered) {
        this.ordered = ordered;
    }

    public StringMapBuilder unordered() {
        setOrdered(false);
        return this;
    }

    /**
     * Add a key-value pair
     *
     * @param key   the key
     * @param value the corresponding value
     */

    public void put(UnicodeString key, GroundedValue value) {
        keys.add(key);
        values.add(value);
    }

    /**
     * Get the constructed map. This method must only be called once, because the
     * lists of keys and values are used within the constructed map and are mutable.
     *
     * @return the constructed map
     * @throws XPathException if duplicate keys were found, and not detected earlier
     */

    public MapItem getCompletedMap() throws XPathException {
        if (keys.size() == 0) {
            return EmptyMap.getInstance();
        } else if (keys.size() == 1) {
            return new SingleEntryMap(new StringValue(keys.get(0)), values.get(0));
        }
        // quick scan for possible duplicates using a bloom filter
        boolean possibleDuplicates = hasPossibleDuplicates(keys);
        if (possibleDuplicates) {
            // If there are possible duplicates, then we build the map now, with a more thorough
            // check for duplicates as we build it.

            StringMapItem map = new OrderedStringMap();
            Iterator<GroundedValue> valueIter = values.iterator();
            for (UnicodeString key : keys) {
                GroundedValue value;
                if (valueIter.hasNext()) {
                    value = valueIter.next();
                } else {
                    throw new AssertionError();
                }
                GroundedValue existing = map.getU(key);
                if (existing == null) {
                    map = (OrderedStringMap)map.putU(key, value);
                } else if (combiner == null) {
                    throw new XPathException("Duplicate key " + Err.wrap(key) + " in map");
                } else {
                    GroundedValue newVal = combiner.combine(existing, value, context);
                    map = (OrderedStringMap)map.putU(key, newVal);
                }
            }
            return map;
        } else {
            // if we are sure there are no duplicates, then we delay indexing the map entries
            // until a map lookup operation is performed. This is because many maps are built
            // and immediately discarded or serialized without ever being queried.

            return new LazyStringMap(keys, values);
        }


    }

    /**
     * Get the constructed map. This method must only be called once, because the
     * lists of keys and values are used within the constructed map and are mutable.
     * This version of the method does not check for duplicated keys, and must be used
     * only when the caller is 100% confident that there are no duplicates.
     *
     * @return the constructed map
     */

    public MapItem getCompletedMapConfidently() {
        if (keys.size() == 0) {
            return EmptyMap.getInstance();
        } else if (keys.size() == 1) {
            return new SingleEntryMap(new StringValue(keys.get(0)), values.get(0));
        }
        return new LazyStringMap(keys,values);
    }

    private static boolean hasPossibleDuplicates(List<UnicodeString> keys) {
        int filterSize = keys.size() < 50 ? 1024 : 65536;
        int mask = filterSize - 1;
        BitSet bloom = new BitSet(filterSize);
        for (UnicodeString key : keys) {
            long hash = key.longHashCode();
            int hash0 = (int) (hash >> 40 & mask);
            int hash1 = (int) (hash >> 20 & mask);
            int hash2 = (int) hash & mask;
            if (bloom.get(hash0) && bloom.get(hash1) && bloom.get(hash2)) {
                Instrumentation.count("Bloom clash");
                return true;
            }
            bloom.set(hash0);
            bloom.set(hash1);
            bloom.set(hash2);
        }
        return false;
    }

}

