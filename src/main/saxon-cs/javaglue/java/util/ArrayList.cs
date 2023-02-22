using System;
using System.Linq;
//using System.Collections.Generic;
using SysCollGen = System.Collections.Generic;
using SysColl = System.Collections;
namespace java.util {
    public class ArrayList<E> : List<E> {
        private SysCollGen.List<E> _list;
        public ArrayList() {
            this._list = new SysCollGen.List<E>();
        }
        public ArrayList(int alloc) {
            this._list = new SysCollGen.List<E>(alloc);
        }
        
        public ArrayList(Collection<E> existing) {
            this._list = new SysCollGen.List<E>(existing.size());
            addAll(existing);
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

        // removeAt(int) corresponds to Java remove(int), which is ambiguous in C#
        public E removeAt(int index) {
            E temp = _list[index];
            _list.RemoveAt(index);
            return temp;
        }

        public bool remove(E entry) {
            _list.Remove(entry);
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

        public Iterator<E> iterator() {
            throw new NotImplementedException();
        }
        public List<E> subList(int start, int end) {
            return new ArrayList<E>(_list.GetRange(start, end - start));
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

        public SysColl.IEnumerator GetEnumerator() {
            return _list.GetEnumerator();
        }

        public void sort(Comparison<E> comparer) {
            _list.Sort(comparer);
        }
        


        public java.util.Iterator<E> iterate() {
            return new com.saxonica.glue.IteratorFromEnumerator<E>(_list.GetEnumerator());
        }

        public void trimToSize() {
            _list.TrimExcess();
        }
    }
}