using System;
using SysCollGen = System.Collections.Generic;
using System.Linq;
using NSec.Cryptography;
using Saxon.Hej.value;
using KeyValuePair = Saxon.Hej.ma.map.KeyValuePair;

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

        /// <summary>
        /// Custom method to support iteration over a DictionaryMap. The method returns an iteration
        /// of Saxon KeyValuePair objects, produced by converting the .NET KeyValuePair objects
        /// delivered from the dictionary iteration; the String keys in the .NET KVP are converted
        /// to StringValue keys in the Saxon KVP.
        /// </summary>
        /// <param name="map">The map to be interated over.</param>
        /// <returns>An iterator of key value pairs</returns>
        /// <remarks>
        /// <para>I tried to make this more generic but it got too complicated.
        /// </remarks>

        public static SysCollGen.IEnumerator<Saxon.Hej.ma.map.KeyValuePair> convertStringKeyValuePairs(
                SysCollGen.IDictionary<string, Saxon.Hej.om.GroundedValue> map) {
            foreach (SysCollGen.KeyValuePair<string, Saxon.Hej.om.GroundedValue> kvp in map) {
                yield return new Saxon.Hej.ma.map.KeyValuePair(new StringValue(kvp.Key), kvp.Value);
            }
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited