using SysCollGen = System.Collections.Generic;
using java.lang;

namespace java.util.concurrent {
    public class ConcurrentHashMap<K, V> : Map<K, V> {
        protected SysCollGen.ConcurrentDictionary<K, V> dict;
        public ConcurrentHashMap() {
            dict = new SysCollGen.ConcurrentDictionary<K, V>();
        }
        public ConcurrentHashMap(int size) {
            dict = new SysCollGen.ConcurrentDictionary<K, V>(size);
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

        public void remove(K key) {
            dict.Remove(key);
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

        public void putAll(Map<K, V> map) {
            throw new UnsupportedOperationException();
        }
    }
}