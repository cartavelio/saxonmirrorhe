package net.sf.saxon.ma.map;

import net.sf.saxon.om.GroundedValue;

/**
 * Interface implemented by map items that allow incremental construction. The map may be initialized
 * using a sequence of calls on {@link #initialPut(String, GroundedValue)}, which modify the content
 * in-situ; but this method must never be called once the map has been released into the wild.
 */

public interface BuildableMap {

    /**
     * During initial construction of the map, add a key-value pair
     *
     * @param key   the key
     * @param value the corresponding value
     */

    void initialPut(String key, GroundedValue value);
}

