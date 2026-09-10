using System.Collections.Generic;
using SysRunComp = System.Runtime.CompilerServices;
using java.lang;

namespace java.util {
    public class WeakHashMap<K, V> : Map<K, V> where K : referencetype {
        protected SysRunComp.ConditionalWeakTable<K, V> dict;
        public WeakHashMap() {
            dict = new Dictionary<K, V>();
        }
        public WeakHashMap(int size) {
            dict = new Dictionary<K, V>(size);
        }
        public WeakHashMap(Map<K, V> existing) {
            dict = new Dictionary<K, V>(existing.size());
            addAll(existing);
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

        public void clear() {
            dict.Clear();
        }
        public Collection<V> values() {
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