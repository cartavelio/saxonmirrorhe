using SysCollGen = System.Collections.Generic;
using SysRunComp = System.Runtime.CompilerServices;
using Saxon.Ejava.lang;

namespace Saxon.Ejava.util {
    public class WeakHashMap<K, V> where K : class where V : class {
        protected SysRunComp.ConditionalWeakTable<K, V> dict;
        public WeakHashMap() {
            dict = new SysRunComp.ConditionalWeakTable<K, V>();
        }
        public WeakHashMap(int size) {
            dict = new SysRunComp.ConditionalWeakTable<K, V>();
        }
 
        public V get(object key) {
            if (key is K kk && dict.TryGetValue(kk, out var value)) {
                return value;
            } else {
                return null;
            }
        }

        public V put(K key, V value) {
            V current = get(key);
            dict.AddOrUpdate(key, value);
            return current;
        }

        public void remove(K key) {
            dict.Remove(key);
        }
    }
}