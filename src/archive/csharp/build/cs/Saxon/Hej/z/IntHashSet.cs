using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntHashSet : Saxon.Hej.z.IntSet {
        private const int NBIT = 30;
        private const int MAX_SIZE = (1<<NBIT);
        private readonly int ndv;
        private int _nmax;
        private int _size;
        private int _nlo;
        private int _nhi;
        private int _shift;
        private int _mask;
        private int[] _values;
        public IntHashSet() : this (8, System.Int32.MinValue) {
        }
        public IntHashSet(int capacity) : this (capacity, System.Int32.MinValue) {
        }
        public IntHashSet(int capacity, int noDataValue)  {
            ndv = noDataValue;
            setCapacity(capacity);
        }
        public override Saxon.Hej.z.IntSet copy() {
            if ((_size == 0)) {
                return Saxon.Hej.z.IntEmptySet.getInstance();
            } else {
                Saxon.Hej.z.IntHashSet s = new Saxon.Hej.z.IntHashSet(_size, ndv);
                s._nmax = _nmax;
                s._size = _size;
                s._nlo = _nlo;
                s._nhi = _nhi;
                s._shift = _shift;
                s._mask = _mask;
                s._values = new int[_values.Length];
                Saxon.Ejava.lang.System.arraycopy(_values, 0, s._values, 0, _values.Length);
                return s;
            }
        }
        public override Saxon.Hej.z.IntSet mutableCopy() {
            return copy();
        }
        public override void clear() {
            _size = 0;
            for (int i = 0; (i<_nmax); ++i) {
                _values[i] = ndv;
            }
        }
        public override int size() {
            return _size;
        }
        public override bool isEmpty() {
            return (_size == 0);
        }
        public int[] getValues() {
            int index = 0;
            int[] values = new int[_size];
            foreach (int _value in _values) {
                if ((_value != ndv)) {
                    values[index++] = _value;
                }
            }
            return values;
        }
        public override bool contains(int value) {
            return ((_values[indexOf(value)] != ndv));
        }
        public override bool remove(int value) {
            int i = indexOf(value);
            if ((_values[i] == ndv)) {
                return false;
            }
            --_size;
            for (; ; ) {
                _values[i] = ndv;
                int j = i;
                int r;
                do {
                    i = (((i-1))&_mask);
                    if ((_values[i] == ndv)) {
                        return true;
                    }
                    r = hash(_values[i]);
                } while ((((((i<=r) && (r<j))) || (((r<j) && (j<i)))) || (((j<i) && (i<=r)))));
                _values[j] = _values[i];
            }
        }
        public override bool add(int value) {
            if ((value == ndv)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Can't add the 'no data' value");
            }
            int i = indexOf(value);
            if ((_values[i] == ndv)) {
                ++_size;
                _values[i] = value;
                if ((_size>MAX_SIZE)) {
                    throw new System.Exception((("Too many elements (> "+MAX_SIZE)+')'));
                }
                if (((_nlo<_size) && (_size<=_nhi))) {
                    setCapacity(_size);
                }
                return true;
            } else {
                return false;
            }
        }
        private int hash(int key) {
            return (((((1327217885*key))>>_shift))&_mask);
        }
        private int indexOf(int value) {
            int i = hash(value);
            while ((_values[i] != ndv)) {
                if ((_values[i] == value)) {
                    return i;
                }
                i = (((i-1))&_mask);
            }
            return i;
        }
        private void setCapacity(int capacity) {
            if ((capacity<_size)) {
                capacity = _size;
            }
            int nbit, nmax;
            for (nbit = 1, nmax = 2; ((nmax<(capacity*4)) && (nmax<MAX_SIZE)); ++nbit, nmax *= 2) {
            }
            int nold = _nmax;
            if ((nmax == nold)) {
                return ;
            }
            _nmax = nmax;
            _nlo = (nmax/4);
            _nhi = (MAX_SIZE/4);
            _shift = ((1+NBIT)-nbit);
            _mask = (nmax-1);
            _size = 0;
            int[] values = _values;
            _values = new int[nmax];
            Saxon.Ejava.util.Arrays.fill(_values, ndv);
            if ((values != null)) {
                for (int i = 0; (i<nold); ++i) {
                    int value = values[i];
                    if ((value != ndv)) {
                        ++_size;
                        _values[indexOf(value)] = value;
                    }
                }
            }
        }
        public override Saxon.Hej.z.IntIterator iterator() {
            return new Saxon.Hej.z.IntHashSet.IntHashSetIterator(this);
        }
        public static bool containsSome(Saxon.Hej.z.IntSet one, Saxon.Hej.z.IntSet two) {
            if ((two is Saxon.Hej.z.IntEmptySet)) {
                return false;
            }
            if ((two is Saxon.Hej.z.IntUniversalSet)) {
                return !(one.isEmpty());
            }
            if ((two is Saxon.Hej.z.IntComplementSet)) {
                return !(((Saxon.Hej.z.IntComplementSet)(two)).getExclusions().containsAll(one));
            }
            Saxon.Hej.z.IntIterator it = two.iterator();
            while (it.hasNext()) {
                if (one.contains(it.next())) {
                    return true;
                }
            }
            return false;
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.z.IntSet)) {
                Saxon.Hej.z.IntHashSet s = (Saxon.Hej.z.IntHashSet)(other);
                return (((size() == s.size()) && containsAll(s)));
            } else {
                return false;
            }
        }
        public override int GetHashCode() {
            int h = 936247625;
            Saxon.Hej.z.IntIterator it = iterator();
            while (it.hasNext()) {
                h += it.next();
            }
            return h;
        }
        public override string ToString() {
            return stringify(iterator());
        }
        public static string stringify(Saxon.Hej.z.IntIterator it) {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(100);
            while (it.hasNext()) {
                if ((sb.length() == 0)) {
                    sb.append(it.next());
                } else {
                    sb.append(' ').append(it.next());
                }
            }
            return sb.ToString();
        }
        public static Saxon.Hej.z.IntHashSet of(params int[]  members) {
            Saxon.Hej.z.IntHashSet @is = new Saxon.Hej.z.IntHashSet(members.Length);
            foreach (int i in members) {
                @is.add(i);
            }
            return @is;
        }
        private class IntHashSetIterator : Saxon.Hej.z.IntIterator {
            private readonly Saxon.Hej.z.IntHashSet container;
            private int i;
            internal IntHashSetIterator(Saxon.Hej.z.IntHashSet container)  {
                this.container = container;
                i = 0;
            }
            public bool hasNext() {
                while ((i<container._values.Length)) {
                    if ((container._values[i] != container.ndv)) {
                        return true;
                    } else {
                        i++;
                    }
                }
                return false;
            }
            public int next() {
                return container._values[i++];
            }
        
        }
    
    }
}