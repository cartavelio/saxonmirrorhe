using System;

namespace java.util {
    public abstract class AbstractList<E> : List<E> {
        private System.Collections.Generic.List<E> _list;
        public AbstractList() {
            this._list = new System.Collections.Generic.List<E>();
        }
        public AbstractList(int alloc) {
            this._list = new System.Collections.Generic.List<E>(alloc);
        }
        
        public AbstractList(AbstractList<E> existing) {
            this._list = new System.Collections.Generic.List<E>(existing.size());
            addAll(existing);
        }

        internal AbstractList(System.Collections.Generic.List<E> list) {
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

        public void addAll(java.util.Collection<E> entry)
        {
            throw new Exception("not implemented");
        }

        public void remove(int index) {
            _list.RemoveAt(index);
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
            return new AbstractList<E>(_list.GetRange(start, end - start));
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
    }
}