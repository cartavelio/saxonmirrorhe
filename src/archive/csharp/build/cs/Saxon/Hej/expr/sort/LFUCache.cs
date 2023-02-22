using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class LFUCache<K, V>  where V:class {
        private readonly bool concurrent;
        private int targetSize;
        private int retentionThreshold = 1;
        private System.Collections.Generic.IDictionary<K,Saxon.Hej.expr.sort.LFUCacheEntryWithCounter<V>> map;
        public LFUCache(int cacheSize) : this (cacheSize, false) {
            targetSize = cacheSize;
        }
        public LFUCache(int cacheSize, bool concurrent)  {
            this.concurrent = concurrent;
            targetSize = cacheSize;
            map = makeMap(cacheSize);
        }
        private System.Collections.Generic.IDictionary<K,Saxon.Hej.expr.sort.LFUCacheEntryWithCounter<V>> makeMap(int cacheSize) {
            if (concurrent) {
                return new /* java.util.concurrent.ConcurrentHashMap */ System.Collections.Concurrent.ConcurrentDictionary<K,Saxon.Hej.expr.sort.LFUCacheEntryWithCounter<V>>(16, cacheSize);
            } else {
                return new System.Collections.Generic.Dictionary<K,Saxon.Hej.expr.sort.LFUCacheEntryWithCounter<V>>(cacheSize);
            }
        }
        public V get(K key) {
            Saxon.Hej.expr.sort.LFUCacheEntryWithCounter<V> entry = Saxon.Impl.Helpers.MapUtils.Get(map, key);
            if ((entry == null)) {
                return null;
            } else {
                entry.counter++;
                return entry.value;
            }
        }
        public bool containsKey(K key) {
            Saxon.Hej.expr.sort.LFUCacheEntryWithCounter<V> entry = Saxon.Impl.Helpers.MapUtils.Get(map, key);
            if ((entry == null)) {
                return false;
            } else {
                entry.counter++;
                return true;
            }
        }
        public void put(K key, V value) {
            Saxon.Impl.Helpers.MapUtils.Put(map, key, new Saxon.Hej.expr.sort.LFUCacheEntryWithCounter<V>(value));
            if ((map.Count>(3*targetSize))) {
                rebuild();
            }
        }
        private void rebuild() {
            System.Collections.Generic.IDictionary<K,Saxon.Hej.expr.sort.LFUCacheEntryWithCounter<V>> m2 = makeMap(targetSize);
            int retained = 0;
            foreach (System.Collections.Generic.KeyValuePair<K,Saxon.Hej.expr.sort.LFUCacheEntryWithCounter<V>> entry in map) {
                if ((entry.Value.counter>retentionThreshold)) {
                    Saxon.Impl.Helpers.MapUtils.Put(m2, entry.Key, new Saxon.Hej.expr.sort.LFUCacheEntryWithCounter<V>(entry.Value.value));
                    retained++;
                }
            }
            if ((retained>(1.5*targetSize))) {
                retentionThreshold++;
            } else {
                if (((retentionThreshold>0) && (retained<targetSize))) {
                    retentionThreshold--;
                }
            }
            map = m2;
        }
        public void clear() {
            map.Clear();
        }
        public int size() {
            return map.Count;
        }
    
    }
}