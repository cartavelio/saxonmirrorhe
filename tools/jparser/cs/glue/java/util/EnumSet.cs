using System;
using System.Collections.Generic;
namespace java.util {
    public class EnumSet<E> : Set<E> {
        internal System.Collections.Generic.HashSet<E> _set;
        public EnumSet() {
            _set = new System.Collections.Generic.HashSet<E>(); 
        }
        public int size() {
            return _set.Count;
        }
        public bool contains(object key) {
            return key is E && _set.Contains((E)key);
        }
        public void add(E entry) {
            _set.Add(entry);
        }

        public void remove(E entry) {
            _set.Remove(entry);
        }
        public static EnumSet<E> of (params E[] entries) {
            EnumSet<E> result = new EnumSet<E>();
            foreach (E e in entries) {
                result.add(e);
            }
            return result;
        }
        
        public static EnumSet<E> noneOf (Type t) {
            return new EnumSet<E>();
        }
        public static EnumSet<E> noneOf () {
            return new EnumSet<E>();
        }
    }
}