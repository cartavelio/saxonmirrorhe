using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    internal class CustomMapEntryChain<K, V>  {
        public K key;
        public V value;
        public Saxon.Hej.expr.sort.CustomMapEntryChain<K,V> next;
        public CustomMapEntryChain(K key, V value)  {
            this.key = key;
            this.value = value;
            this.next = null;
        }
    
    }
}