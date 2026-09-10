using System;
using System.Text;
using Saxon.Hej.trans;

// C# equivalent of utility functions for processing VAVR maps. In Saxon-CS the VAVR maps are replaced by
// Standard C# Immutable.ImmutableDictionary implementation.

namespace Saxon.Impl.Overrides {
    public class Vavr {
        

         // * Get a value from a VAVR map, returning null if the value is not present
         // * @param map the VAVR map
         // * @param key the supplied key
         // * @return the corresponding value if present, or null if not.
         // * @param <K> the key type of the map
         // * @param <V> the value type of the map

        public static V get<K, V> (System.Collections.Immutable.ImmutableDictionary<K,V> map, K key) {
            bool present = map.TryGetValue(key, out var value);
            return present ? value : default(V);
        }
        
        /**
         * Process all the entries in a VAVR map using a supplied callback functoin
         * @param map the map to be processed
         * @param mapper the function to be applied to every entry in the map
         * @param <K> the key type of the map
         * @param <V> the value type of the map
         */
        
        public static void forEach<K, V>(System.Collections.Immutable.ImmutableDictionary<K,V> map, Action<K, V> mapper) {
            var e = map.GetEnumerator();
            while (e.MoveNext()) {
                mapper(e.Current.Key, e.Current.Value);
            }
        }
        
        
        /**
         * Get an iterator over the values (the value part of the key value pair)
         * in a VAVR map
         *
         * @param map    the map to be processed
         * @param <K>    the key type of the map
         * @param <V>    the value type of the map
         */

        public static System.Collections.Generic.IEnumerator<V> valuesIterator<K, V>(
                System.Collections.Immutable.ImmutableDictionary<K, V> map) {
            var e = map.GetEnumerator();
            while (e.MoveNext()) {
                yield return e.Current.Value;
            }
        } 
        
        
    }
    
}

// Copyright (c) 2025 Saxonica Limited