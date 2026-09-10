using SysCollGen = System.Collections.Generic;
namespace java.util {
    public class TreeSet<E> : Set<E> {
        private SysCollGen.SortedSet<E> _set;
        public TreeSet () {
            this._set = new SysCollGen.SortedSet<E>();
        }
        
    }
}