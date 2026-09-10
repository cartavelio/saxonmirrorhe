package net.sf.saxon.ma.map;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.str.UnicodeString;

/**
 * A StringMap represents a map whose keys are all instances of xs:string. It supplements
 * the standard map interface with methods to get values using a Java String key,
 * or a Saxon {@link UnicodeString}
 */

public abstract class StringMapItem extends MapItem {

//    /**
//     * Get an entry from the map, supplying a Java String as the key
//     *
//     * @param key the key
//     * @return the relevant entry if present, or null if absent.
//     */
//    public abstract GroundedValue get(String key);

    /**
     * Get an entry from the map, supplying a {@link UnicodeString} as the key
     *
     * @param key the key
     * @return the relevant entry if present, or null if absent.
     */
    public abstract GroundedValue getU(UnicodeString key);

//    /**
//     * Ask whether a given string is present as a key in the map
//     *
//     * @param key the key being tested
//     * @return true if the key is present
//     */
//    public abstract boolean contains(String key);

    /**
     * Ask whether a given string is present as a key in the map
     *
     * @param key the key being tested
     * @return true if the key is present
     */
    public abstract boolean containsU(UnicodeString key);

    /**
     * Add or replace an entry whose key is a string
     * @param key the key to be added or replaced
     * @param value the corresponding value
     * @return the new map (the original is unchanged)
     */

    public abstract MapItem putU(UnicodeString key, GroundedValue value);

    /**
     * Remove an entry whose key is a string
     *
     * @param key   the key to be added or replaced
     * @return the new map (the original is unchanged)
     */

    public abstract MapItem removeU(UnicodeString key);

}

