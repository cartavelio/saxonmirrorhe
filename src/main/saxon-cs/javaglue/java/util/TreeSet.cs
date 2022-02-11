using net.sf.saxon.dotnet;
using net.sf.saxon.functions;
using net.sf.saxon.tree.jiter;
using SysCollGen = System.Collections.Generic;
namespace java.util {
    public class TreeSet<E> : Set<E> {
        private SysCollGen.SortedSet<E> _set;
        public TreeSet () {
            this._set = new SysCollGen.SortedSet<E>();
        }
        
        public TreeSet (Collection<E> existing) {
            this._set = new SysCollGen.SortedSet<E>(existing);
        }
        public TreeSet (Comparator<E> comparator) {
            this._set = new SysCollGen.SortedSet<E>(new DotNetComparator<(comparator));
        }

        public bool add(E e) {
            return this._set.Add(e);
        }

        public bool isEmpty() {
            return this._set.Count == 0;
        }

        public E pollFirst() {
            SysCollGen.IEnumerator<E> iter = this._set.GetEnumerator();
            if (iter.MoveNext()) {
                E result = iter.Current;
                this._set.Remove(result);
                return result;
            }
            else {
                return default(E);
            }
        }
        
    }
}