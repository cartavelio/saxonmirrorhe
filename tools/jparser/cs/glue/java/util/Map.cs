using System;
using System.Collections.Generic;
namespace java.util {
    public interface Map<K, V>  {
        int size();
        bool isEmpty();
        V get(object key);
        V put(K key, V value);
        Set<K> keySet();
        Collection<K> values();
        Set<Map_Entry<K, V>> entries();
    }
    public interface Map_Entry<K, V> {
    }
}