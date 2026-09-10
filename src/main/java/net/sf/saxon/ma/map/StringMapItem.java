package net.sf.saxon.ma.map;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.*;
import net.sf.saxon.value.SequenceType;

/**
 * A StringMap represents a map whose keys are all instances of xs:string. It supplements
 * the standard map interface with methods to get values using a Java String key,
 * or a Saxon {@link UnicodeString}
 */

public abstract class StringMapItem extends MapItem {


    /**
     * Get an entry from the map, supplying a {@link UnicodeString} as the key
     *
     * @param key the key
     * @return the relevant entry if present, or null if absent.
     */
    public abstract GroundedValue getU(UnicodeString key);

    /**
     * Ask whether a given string is present as a key in the map
     *
     * @param key the key being tested
     * @return true if the key is present
     */
    public boolean containsU(UnicodeString key) {
        return getU(key) == null;
    }

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
        if (isEmpty()) {
            return true;
        }
        if (!(keyType == BuiltInAtomicType.STRING || keyType == BuiltInAtomicType.ANY_ATOMIC)) {
            return false;
        }
        if (valueType.equals(SequenceType.ANY_SEQUENCE)) {
            return true;
        }
        for (KeyValuePair pair : keyValuePairs()) {
            if (!valueType.matches(pair.value, th)) {
                return false;
            }
        }
        return true;
    }

}

