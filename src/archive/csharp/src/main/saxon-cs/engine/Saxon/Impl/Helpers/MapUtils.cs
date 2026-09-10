using System;
using SysCollGen = System.Collections.Generic;
using System.Linq;
namespace Saxon.Impl.Helpers {
    public class MapUtils {
        public static V Get<K,V>(SysCollGen.IDictionary<K, V> map, K key) /*where V : class*/ {
            bool present = map.TryGetValue(key, out var value);
            return present ? value : default(V);
        }
        public static V Put<K,V>(SysCollGen.IDictionary<K, V> map, K key, V value) /*where V : class*/ {
            V existing = Get(map, key);
            map[key] = value;
            return existing;
        }
        public static void PutAll<K,V>(SysCollGen.IDictionary<K, V> map, SysCollGen.IDictionary<K, V> other)  {
            foreach (SysCollGen.KeyValuePair<K, V> pair in other) {
                map[pair.Key] = pair.Value;
            }
        }
        public static void Merge<K,V>(SysCollGen.IDictionary<K, V> map, K key, V value, Func<V, V, V> function) {
            throw new NotImplementedException();
        }
        public static SysCollGen.ISet<K> KeySet<K,V>(SysCollGen.IDictionary<K, V> map) {
            return new SysCollGen.HashSet<K>(map.Keys);
        }
        
        //If the specified key is not already associated with a value (or is mapped to null),
        //attempts to compute its value using the given mapping function and enters it into this map
        //unless null.

        public static V ComputeIfAbsent<K,V>(SysCollGen.IDictionary<K, V> map, K key,
                                             Func<K,V> mappingFunction) where V : class {
            if (map.ContainsKey(key)) {
                return map[key];
            } else {
                V value = mappingFunction(key);
                map[key] = value;
                return value;
            }
        }
        
        public static V GetOrDefault<K,V>(SysCollGen.IDictionary<K, V> map, K key, V defaultVal) {
            V val;
            bool found = map.TryGetValue(key, out val);
            if (found) {
                return val;
            } else {
                return defaultVal;
            }
        }
        
        public static V PutIfAbsent<K,V>(SysCollGen.IDictionary<K, V> map, K key, V newVal)  {
            if (map.ContainsKey(key)) {
                return map[key];
            } else {
                map[key] = newVal;
                return default(V);
            } 
        }
    }
}

// Copyright (c) 2021 Saxonica Limited