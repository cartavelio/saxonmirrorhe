using System;
using System.Collections.Generic;
namespace java.util {
    public class HashSet<E> : Set<E> {
        internal System.Collections.Generic.HashSet<E> _set;
        public HashSet() {
            _set = new System.Collections.Generic.HashSet<E>(); 
        }
        public HashSet(int capacity) {
            _set = new System.Collections.Generic.HashSet<E>(capacity); 
        }
        public int size() {
            return _set.Count;
        }

        public bool isEmpty() {
            return _set.Count == 0;
        }

        public bool contains(object key) {
            return key is E && _set.Contains((E)key);
        }
        public bool add(E entry) {
            return _set.Add(entry);
        }

        public void addAll(Collection<E> other) {
            foreach (E e in other) {
                add(e);
            }
        }
    }
}