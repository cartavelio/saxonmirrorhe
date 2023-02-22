using System;
using System.Collections.Generic;

/**
 * A C# emulation of java.util.HashMap, incomplete but sufficient for use in Saxon
 */
 
namespace java.util {
    public class HashMap<K, V> : Map<K, V> {
        protected Dictionary<K, V> dict;
        public HashMap() {
            dict = new Dictionary<K, V>();
        }
        public HashMap(int size) {
            dict = new Dictionary<K, V>(size);
        }
        public int size() {
            return dict.Count;
        }
        public bool isEmpty() {
            return dict.Count == 0;
        }
        public V get(object key) {
            V value;
            if (key is K && dict.TryGetValue((K)key, out value)) {
                return value;
            } else {
                return default(V);
            }
        }

        public bool containsKey(K key) {
            return dict.ContainsKey(key);
        }
        public V put(K key, V value) {
            V current = get(key);
            dict.Add(key, value);
            return current;
        }
        public Set<K> keySet() {
            return null; // TODO
        }
        public Collection<K> values() {
            return null; // TODO
        }
        public Set<Map_Entry<K, V>> entries() {
            return null; // TODO
        }
    }

}