using Sys = System;
using SysCollGen = System.Collections.Generic;
namespace java.util {
    public interface Map<K, V> : java.lang.Iterable<java.util.Map_Entry<K, V>> {
        int size();
        bool isEmpty();
        V get(object key);
        bool containsKey(object key);
        V computeIfAbsent(K key, System.Func<K, V> mappingFn);
        V put(K key, V value);
        void remove(K key);
        Set<K> keySet();
        Collection<V> values();
        Set<Map_Entry<K, V>> entrySet();
        void putAll(Map<K, V> map);
        void clear();
    }
    public class Map_Entry<K, V> {
        private K _key;
        private V _value;

        public Map_Entry(K key, V value) {
            this._key = key;
            this._value = value;
        }

        public K getKey() {
            return _key;
        }

        public V getValue() {
            return _value;
        }
    }
}