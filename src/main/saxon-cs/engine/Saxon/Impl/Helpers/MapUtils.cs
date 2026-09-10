using System;
using SysCollGen = System.Collections.Generic;
using System.Linq;
namespace Saxon.Impl.Helpers {
    public class MapUtils {
        public static V Get<K,V>(SysCollGen.IDictionary<K, V> map, K key) /*where V : class*/ {
            bool present = map.TryGetValue(key, out var value);
            return present ? value : default(V);
        }
        public static void PutAll<K,V>(SysCollGen.IDictionary<K, V> map, SysCollGen.IDictionary<K, V> other)  {
            foreach (SysCollGen.KeyValuePair<K, V> pair in other) {
                map[pair.Key] = pair.Value;
            }
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
    }
}