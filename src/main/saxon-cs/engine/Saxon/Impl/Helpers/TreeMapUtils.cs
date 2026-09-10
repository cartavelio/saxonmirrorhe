using System;
using System.Linq;
using SysCollGen = System.Collections.Generic;
namespace Saxon.Impl.Helpers {
    public class TreeMapUtils {
        
        /*
         * Methods available on Java TreeMap that are not available on a C# SortedDictionary.
         * Implementation is not trivial, and not very efficient, but it's not that important
         * either. We can use the Keys property to get a KeyCollection, which we can copy to
         * an array, and then use a binary-chop search of the array. Not very nice. LINQ doesn't
         * seem to help much either.
         */
        
        public static K FirstKey<K,V>(SysCollGen.SortedDictionary<K, V> map) where K : class where V : class {
            return map.Count == 0 ? null : map.Keys.First();
        }
        
        public static K LastKey<K,V>(SysCollGen.SortedDictionary<K, V> map) where K : class where V : class {
            return map.Count == 0 ? null : map.Keys.Last();
        }
        
        public static K FloorKey<K,V>(SysCollGen.SortedDictionary<K, V> map, K key) where K : class where V : class {
            if (map.ContainsKey(key)) {
                return key;
            }
            else {
                throw new NotImplementedException();
            }
        }
        
        public static K CeilingKey<K,V>(SysCollGen.SortedDictionary<K, V> map, K key) where K : class where V : class {
            if (map.ContainsKey(key)) {
                return key;
            }
            else {
                throw new NotImplementedException();
            }
        }
        
        public static K HigherKey<K,V>(SysCollGen.SortedDictionary<K, V> map, K key) where K : class where V : class {
            throw new NotImplementedException();
        }
        
    }
}

// Copyright (c) 2021 Saxonica Limited