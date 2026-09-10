package net.sf.saxon.ma.map;

import net.sf.saxon.transpile.CSharpReplaceBody;

import java.util.Iterator;
import java.util.function.BiConsumer;

/**
 * Utility class containing static methods for use with Vavr - designed to facilitate transpilation
 */

public class Vavr {

//    @CSharpReplaceBody(code="return System.Collections.Immutable.ImmutableDictionary<K,V>.Empty;")
//    public static <K, V> io.vavr.collection.LinkedHashMap<K, V> emptyLinkedHashMap() {
//        return LinkedHashMap.empty();
//    }

    @CSharpReplaceBody(code="bool present = map.TryGetValue(key, out var value);\n"
              + "            return present ? value : default(V);")
    public static <K, V> V get(io.vavr.collection.Map<K, V> map, K key) {
        return map.get(key).orNull();
    }

    public static <K, V> void forEach(io.vavr.collection.Map<K, V> map, BiConsumer<K, V> mapper) {
        map.forEach(mapper);
    }

    public static <K, V> Iterator<V> valuesIterator(io.vavr.collection.Map<K, V> map) {
        return map.valuesIterator();
    }
}

