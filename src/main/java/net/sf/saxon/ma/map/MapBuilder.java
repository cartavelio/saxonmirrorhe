package net.sf.saxon.ma.map;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.value.AtomicValue;

/**
 * Interface implemented by map items that allow incremental construction. The map may be initialized
 * using a sequence of calls on {@link #put(AtomicValue, GroundedValue)}, which modify the content.
 */

public interface MapBuilder {

    /**
     * Add a key-value pair
     *
     * @param key   the key
     * @param value the corresponding value
     */

    void put(AtomicValue key, GroundedValue value);

    /**
     * Get the constructed map. This method must only be called once; subsequent calls should return null
     * @return the constructed map
     */

    MapItem getCompletedMap();

}

