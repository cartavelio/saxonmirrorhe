using System;
using System.Text;
using Saxon.Hej.trans;


namespace Saxon.Impl.Overrides {
    public class Vavr {

        public static V get<K, V> (System.Collections.Immutable.ImmutableDictionary<K,V> map, K key) {
            bool present = map.TryGetValue(key, out var value);
            return present ? value : default(V);
        }
        
        public static void forEach<K, V>(System.Collections.Immutable.ImmutableDictionary<K,V> map, Action<K, V> mapper) {
            var e = map.GetEnumerator();
            while (e.MoveNext()) {
                mapper(e.Current.Key, e.Current.Value);
            }
        }

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