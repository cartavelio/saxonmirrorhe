using System;
using System.Runtime.Remoting.Messaging;

namespace java.util {
    public class LinkedList<E> : List<E> {
        private System.Collections.Generic.LinkedList<E> _list;
        public LinkedList() {
            this._list = new System.Collections.Generic.LinkedList<E>();
        }
        public LinkedList(int alloc) {
            this._list = new System.Collections.Generic.LinkedList<E>(alloc);
        }
        
        public LinkedList(LinkedList<E> existing) {
            this._list = new System.Collections.Generic.LinkedList<E>(existing.size());
            addAll(existing);
        }

        internal LinkedList(System.Collections.Generic.LinkedList<E> list) {
            this._list = list;
        }
        public int size() {
            return _list.Count;
        }

        public bool isEmpty()
        {
            return size() == 0;
        }
        public bool add(E entry) {
            _list.Add(entry);
            return true;
        }
        public void add(int index, E entry) {
            _list.Insert(index, entry);
        }

        public void addAll(List<E> entry)
        {
            throw new Exception("not implemented");
        }

        public void addFirst(E entry) {
            _list.AddFirst(entry);
        }

        public E removeAt(int index) {
            return _list.RemoveAt(index);
        }

        public E removeFirst() {
            E temp = _list.First;
            _list.RemoveFirst();
            return temp;
        }

        public void set(int index, E entry) {
            _list[index] = entry;
        }
        public E get(int index) {
            return _list[index];
        }
        public bool contains(E entry) {
            return _list.Contains(entry);
        }

        public List<E> subList(int start, int end) {
            return new LinkedList<E>(_list.GetRange(start, end - start));
        }

        public int indexOf(E entry) {
            return _list.IndexOf(entry);
        }

        public E[] toArray() {
            return _list.ToArray();
        }
        
        public E[] toArray(E[] e) {
            return _list.ToArray();
        }

        public System.Collections.IEnumerator GetEnumerator() {
            return _list.GetEnumerator();
        }
        


        public java.util.Iterator<E> iterate() {
            return new com.saxonica.glue.IteratorFromEnumerator<E>(_list.GetEnumerator());
        }

        public void trimToSize() {
            _list.TrimExcess();
        }

        public E peek() {
            throw new NotImplementedException();
        }

        public void offer(E) {
            throw new NotImplementedException();
        }

        public void remove() {
            throw new NotImplementedException();
        }
    }
}