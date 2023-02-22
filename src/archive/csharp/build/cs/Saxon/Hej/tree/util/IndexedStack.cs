using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.util {
    public class IndexedStack<T>  : System.Collections.Generic.IEnumerable<T> {
        private readonly System.Collections.Generic.List<T> items;
        public IndexedStack()  {
            items = new System.Collections.Generic.List<T>(20);
        }
        public IndexedStack(int size)  {
            items = new System.Collections.Generic.List<T>(size);
        }
        public int size() {
            return items.Count;
        }
        public bool isEmpty() {
            return items.Count==0;
        }
        public void push(T item) {
            items.Add(item);
        }
        public T peek() {
            if (items.Count==0) {
                throw new Saxon.Ejava.util.EmptyStackException();
            } else {
                return items[(items.Count-1)];
            }
        }
        public T pop() {
            if (items.Count==0) {
                throw new Saxon.Ejava.util.EmptyStackException();
            } else {
                return Saxon.Impl.Helpers.ListUtils.Remove(items, (items.Count-1));
            }
        }
        public T get(int i) {
            return items[i];
        }
        public void set(int i, T value) {
            items[i] = value;
        }
        public bool contains(T value) {
            return items.Contains(value);
        }
        public int indexOf(T value) {
            return items.IndexOf(value);
        }
        public System.Collections.Generic.IEnumerator<T> GetEnumerator() {
            return items.GetEnumerator();
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    
    }
}