package net.sf.saxon.ma.map;

import java.util.Iterator;
import java.util.function.BiConsumer;

/**
 * Utility class containing static methods for use with Vavr - designed to facilitate transpilation.
 * Saxon-CS has a different implementation of this module.
 */

public class Vavr {

    /**
     * Get a value from a VAVR map, returning null if the value is not present
     * @param map the VAVR map
     * @param key the supplied key
     * @return the corresponding value if present, or null if not.
     * @param <K> the key type of the map
     * @param <V> the value type of the map
     */
    public static <K, V> V get(io.vavr.collection.Map<K, V> map, K key) {
        return map.get(key).orNull();
    }

    /**
     * Process all the entries in a VAVR map using a supplied callback functoin
     * @param map the map to be processed
     * @param mapper the function to be applied to every entry in the map
     * @param <K> the key type of the map
     * @param <V> the value type of the map
     */

    public static <K, V> void forEach(io.vavr.collection.Map<K, V> map, BiConsumer<K, V> mapper) {
        map.forEach(mapper);
    }

    /**
     * Get an iterator over the values (the value part of the key value pair)
     * in a VAVR map
     *
     * @param map    the map to be processed
     * @param <K>    the key type of the map
     * @param <V>    the value type of the map
     */

    public static <K, V> Iterator<V> valuesIterator(io.vavr.collection.Map<K, V> map) {
        return map.valuesIterator();
    }
}

