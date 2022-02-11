using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class CustomMap<K, V>  where V:class {
        private readonly Saxon.Hej.z.IntHashMap<Saxon.Hej.expr.sort.CustomMapEntryChain<K,V>> buckets;
        private readonly Saxon.Hej.expr.sort.EqualityMatcher<K> equalityMatcher;
        public CustomMap(Saxon.Hej.expr.sort.EqualityMatcher<K> matcher)  {
            buckets = new Saxon.Hej.z.IntHashMap<Saxon.Hej.expr.sort.CustomMapEntryChain<K, V>>();
            equalityMatcher = matcher;
        }
        public V put(K key, V value) {
            int h = equalityMatcher.hash(key);
            Saxon.Hej.expr.sort.CustomMapEntryChain<K,V> bucket = buckets.get(h);
            if ((bucket == null)) {
                Saxon.Hej.expr.sort.CustomMapEntryChain<K,V> entry = new Saxon.Hej.expr.sort.CustomMapEntryChain<K, V>(key, value);
                buckets.put(h, entry);
                return null;
            } else {
                while (true) {
                    if (equalityMatcher.equal(bucket.key, key)) {
                        V existing = bucket.value;
                        bucket.value = value;
                        return existing;
                    } else {
                        if ((bucket.next == null)) {
                            Saxon.Hej.expr.sort.CustomMapEntryChain<K,V> entry = new Saxon.Hej.expr.sort.CustomMapEntryChain<K, V>(key, value);
                            bucket.next = entry;
                            return null;
                        } else {
                            bucket = bucket.next;
                        }
                    }
                }
            }
        }
        public V get(K key) {
            int h = equalityMatcher.hash(key);
            Saxon.Hej.expr.sort.CustomMapEntryChain<K,V> bucket = buckets.get(h);
            if ((bucket == null)) {
                return null;
            } else {
                while (true) {
                    if (equalityMatcher.equal(bucket.key, key)) {
                        return bucket.value;
                    } else {
                        if ((bucket.next == null)) {
                            return null;
                        } else {
                            bucket = bucket.next;
                        }
                    }
                }
            }
        }
        public bool containsKey(K key) {
            int h = equalityMatcher.hash(key);
            Saxon.Hej.expr.sort.CustomMapEntryChain<K,V> bucket = buckets.get(h);
            if ((bucket == null)) {
                return false;
            } else {
                while (true) {
                    if (equalityMatcher.equal(bucket.key, key)) {
                        return true;
                    } else {
                        if ((bucket.next == null)) {
                            return false;
                        } else {
                            bucket = bucket.next;
                        }
                    }
                }
            }
        }
        public System.Collections.Generic.IList<K> keys() {
            System.Collections.Generic.IList<K> result = new System.Collections.Generic.List<K>();
            foreach (Saxon.Hej.expr.sort.CustomMapEntryChain<K,V> entry in buckets.valueSet()) {
                Saxon.Hej.expr.sort.CustomMapEntryChain<K,V> e = entry;
                do {
                    result.Add(e.key);
                    e = e.next;
                } while ((e != null));
            }
            return result;
        }
        public System.Collections.Generic.IList<V> values() {
            System.Collections.Generic.IList<V> result = new System.Collections.Generic.List<V>();
            foreach (Saxon.Hej.expr.sort.CustomMapEntryChain<K,V> entry in buckets.valueSet()) {
                Saxon.Hej.expr.sort.CustomMapEntryChain<K,V> e = entry;
                do {
                    result.Add(e.value);
                    e = e.next;
                } while ((e != null));
            }
            return result;
        }
    
    }
}