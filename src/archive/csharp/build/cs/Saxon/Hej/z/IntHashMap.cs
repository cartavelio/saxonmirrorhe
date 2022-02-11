using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntHashMap<T>  {
    // Injecting C# code
        
        public class IntHashMapValueSet<V> : System.Collections.Generic.IEnumerable<V> {
            internal readonly IntHashMap<V> container;
            public IntHashMapValueSet(IntHashMap<V> container) {this.container = container;}
            public System.Collections.Generic.IEnumerator<V> GetEnumerator() {return container.valueIterator();}
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {return GetEnumerator();}
        }
    // End of injected C# code
        public IntHashMap() : this (8, 0.25) {
        }
        public IntHashMap(int capacity) : this (capacity, 0.25) {
        }
        public IntHashMap(int capacity, double factor)  {
            _factor = factor;
            setCapacity(capacity);
        }
        public void clear() {
            _n = 0;
            for (int i = 0; (i<_nmax); ++i) {
                _value[i] = nullValue();
            }
        }
        private T nullValue() {
            return default(T);
        }
        private bool isNull(T value) {
            return object.Equals(value, default(T));
        }
        public T get(int key) {
            return _value[indexOf(key)];
        }
        public int size() {
            return _n;
        }
        public bool remove(int key) {
            int i = indexOf(key);
            if ((_value[i] == null)) {
                return false;
            }
            --_n;
            for (; ; ) {
                _value[i] = nullValue();
                int j = i;
                int r;
                do {
                    i = (((i-1))&_mask);
                    if (isNull(_value[i])) {
                        return true;
                    }
                    r = hash(_key[i]);
                } while ((((((i<=r) && (r<j))) || (((r<j) && (j<i)))) || (((j<i) && (i<=r)))));
                _key[j] = _key[i];
                _value[j] = _value[i];
            }
        }
        public T put(int key, T value) {
            if (isNull(value)) {
                throw new Saxon.Ejava.lang.NullPointerException("IntHashMap does not allow null values");
            }
            int i = indexOf(key);
            T old = _value[i];
            if (!(isNull(old))) {
                _value[i] = value;
            } else {
                _key[i] = key;
                _value[i] = value;
                grow();
            }
            return old;
        }
        private const int NBIT = 30;
        private const int NMAX = (1<<NBIT);
        private readonly double _factor;
        private int _nmax;
        private int _n;
        private int _nlo;
        private int _nhi;
        private int _shift;
        private int _mask;
        private int[] _key;
        private T[] _value;
        private int hash(int key) {
            return (((((1327217885*key))>>_shift))&_mask);
        }
        private int indexOf(int key) {
            int i = hash(key);
            while (!(isNull(_value[i]))) {
                if ((_key[i] == key)) {
                    return i;
                }
                i = (((i-1))&_mask);
            }
            return i;
        }
        private void grow() {
            ++_n;
            if ((_n>NMAX)) {
                throw new System.Exception(("number of keys mapped exceeds "+NMAX));
            }
            if (((_nlo<_n) && (_n<=_nhi))) {
                setCapacity(_n);
            }
        }
        private void setCapacity(int capacity) {
            if ((capacity<_n)) {
                capacity = _n;
            }
            double factor = (((_factor<0.01)) ? 0.01 : (((_factor>0.99)) ? 0.99 : _factor));
            int nbit, nmax;
            for (nbit = 1, nmax = 2; (((nmax*factor)<capacity) && (nmax<NMAX)); ++nbit, nmax *= 2) {
            }
            int nold = _nmax;
            if ((nmax == nold)) {
                return ;
            }
            _nmax = nmax;
            _nlo = (int)(((nmax*factor)));
            _nhi = (int)(((NMAX*factor)));
            _shift = ((1+NBIT)-nbit);
            _mask = (nmax-1);
            int[] key = _key;
            T[] value = _value;
            _n = 0;
            _key = new int[nmax];
            _value = makeValueArray(nmax);
            if ((key != null)) {
                for (int i = 0; (i<nold); ++i) {
                    if (!(isNull(value[i]))) {
                        put(key[i], value[i]);
                    }
                }
            }
        }
        private T[] makeValueArray(int size) {
            return new T[size];
        }
        public Saxon.Hej.z.IntIterator keyIterator() {
            return new Saxon.Hej.z.IntHashMap<T>.IntHashMapKeyIterator<T>(this);
        }
        public System.Collections.Generic.IEnumerator<T> valueIterator() {
            return new Saxon.Hej.z.IntHashMap<T>.IntHashMapValueIterator<T>(this);
        }
        public System.Collections.Generic.IEnumerable<T> valueSet() {
            return new Saxon.Hej.z.IntHashMap<T>.IntHashMapValueSet<T>(this);
        }
        public Saxon.Hej.z.IntHashMap<T> copy() {
            Saxon.Hej.z.IntHashMap<T> n = new Saxon.Hej.z.IntHashMap<T>(size());
            Saxon.Hej.z.IntIterator it = keyIterator();
            while (it.hasNext()) {
                int k = it.next();
                n.put(k, get(k));
            }
            return n;
        }
        private class IntHashMapKeyIterator<V>  : Saxon.Hej.z.IntIterator {
            private int i = 0;
            private readonly Saxon.Hej.z.IntHashMap<V> map;
            public IntHashMapKeyIterator(Saxon.Hej.z.IntHashMap<V> map)  {
                this.map = map;
                i = 0;
            }
            public bool hasNext() {
                while ((i<map._key.Length)) {
                    if (!(map.isNull(map._value[i]))) {
                        return true;
                    } else {
                        i++;
                    }
                }
                return false;
            }
            public int next() {
                return map._key[i++];
            }
        
        }
        private class IntHashMapValueIterator<W>  : System.Collections.Generic.IEnumerator<W> {
            private int i = 0;
            private readonly Saxon.Hej.z.IntHashMap<W> map;
            public IntHashMapValueIterator(Saxon.Hej.z.IntHashMap<W> map)  {
                this.map = map;
                i = 0;
            }
            public bool hasNext() {
                while ((i<map._key.Length)) {
                    if (!(map.isNull(map._value[i]))) {
                        return true;
                    } else {
                        i++;
                    }
                }
                return false;
            }
            public W next() {
                W temp = map._value[i++];
                if (map.isNull(temp)) {
                    throw new Saxon.Ejava.util.NoSuchElementException();
                }
                return temp;
            }
            public void remove() {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("remove");
            }
                
        /* Add methods to implement IEnumerable */

            object System.Collections.IEnumerator.Current => Current;
            public W Current { get; private set; }
            public bool MoveNext() {
                if (hasNext()) {Current = next(); return true;} else return false;
            }
            public void Reset() {throw new NotImplementedException();}
            public void Dispose() {}
      
        }
        public Saxon.Hej.z.IntSet keySet() {
            return new Saxon.Hej.z.IntHashMap<T>.IntHashMapKeySet<T>(this);
        }
        private class IntHashMapKeySet<U>  : Saxon.Hej.z.IntSet {
            private readonly Saxon.Hej.z.IntHashMap<U> map;
            public IntHashMapKeySet(Saxon.Hej.z.IntHashMap<U> map)  {
                this.map = map;
            }
            public override void clear() {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("Immutable set");
            }
            public override Saxon.Hej.z.IntSet copy() {
                Saxon.Hej.z.IntHashSet s = new Saxon.Hej.z.IntHashSet();
                Saxon.Hej.z.IntIterator ii = iterator();
                while (ii.hasNext()) {
                    s.add(ii.next());
                }
                return s;
            }
            public override Saxon.Hej.z.IntSet mutableCopy() {
                return copy();
            }
            public override bool isMutable() {
                return false;
            }
            public override int size() {
                return map._n;
            }
            public override bool isEmpty() {
                return (map._n == 0);
            }
            public override bool contains(int key) {
                return (map._value[map.indexOf(key)] != null);
            }
            public override bool remove(int value) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("Immutable set");
            }
            public override bool add(int value) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("Immutable set");
            }
            public override Saxon.Hej.z.IntIterator iterator() {
                return new Saxon.Hej.z.IntHashMap<U>.IntHashMapKeyIterator<U>(map);
            }
            public override Saxon.Hej.z.IntSet union(Saxon.Hej.z.IntSet other) {
                return copy().union(other);
            }
            public override Saxon.Hej.z.IntSet intersect(Saxon.Hej.z.IntSet other) {
                return copy().intersect(other);
            }
            public override Saxon.Hej.z.IntSet except(Saxon.Hej.z.IntSet other) {
                return copy().except(other);
            }
            public override bool containsAll(Saxon.Hej.z.IntSet other) {
                return copy().containsAll(other);
            }
            public override string ToString() {
                return Saxon.Hej.z.IntHashSet.stringify(iterator());
            }
        
        }
    
    }
}