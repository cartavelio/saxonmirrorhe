using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntToIntHashMap : Saxon.Hej.z.IntToIntMap {
        public IntToIntHashMap() : this (8, 0.25) {
        }
        public IntToIntHashMap(int capacity) : this (capacity, 0.25) {
        }
        public IntToIntHashMap(int capacity, double factor)  {
            _factor = factor;
            setCapacity(capacity);
        }
        public void setDefaultValue(int defaultValue) {
            _defaultValue = defaultValue;
        }
        public int getDefaultValue() {
            return _defaultValue;
        }
        public void clear() {
            _n = 0;
            for (int i = 0; (i<_nmax); ++i) {
                _filled[i] = false;
            }
        }
        public bool contains(int key) {
            return _filled[indexOf(key)];
        }
        public int get(int key) {
            int i = indexOf(key);
            return (_filled[i] ? _value[i] : _defaultValue);
        }
        public int size() {
            return _n;
        }
        public bool remove(int key) {
            int i = indexOf(key);
            if (!(_filled[i])) {
                return false;
            }
            --_n;
            for (; ; ) {
                _filled[i] = false;
                int j = i;
                int r;
                do {
                    i = (((i-1))&_mask);
                    if (!(_filled[i])) {
                        return true;
                    }
                    r = hash(_key[i]);
                } while ((((((i<=r) && (r<j))) || (((r<j) && (j<i)))) || (((j<i) && (i<=r)))));
                _key[j] = _key[i];
                _value[j] = _value[i];
                _filled[j] = _filled[i];
            }
        }
        public void put(int key, int value) {
            int i = indexOf(key);
            if (_filled[i]) {
                _value[i] = value;
            } else {
                _key[i] = key;
                _value[i] = value;
                _filled[i] = true;
                grow();
            }
        }
        public Saxon.Hej.z.IntIterator keyIterator() {
            return new Saxon.Hej.z.IntToIntHashMap.IntToIntHashMapKeyIterator(this);
        }
        private const int NBIT = 30;
        private const int NMAX = (1<<NBIT);
        private readonly double _factor;
        private int _defaultValue = System.Int32.MaxValue;
        private int _nmax;
        private int _n;
        private int _nlo;
        private int _nhi;
        private int _shift;
        private int _mask;
        private int[] _key;
        private int[] _value;
        private bool[] _filled;
        private int hash(int key) {
            return (((((1327217885*key))>>_shift))&_mask);
        }
        private int indexOf(int key) {
            int i = hash(key);
            while (_filled[i]) {
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
            int[] value = _value;
            bool[] filled = _filled;
            _n = 0;
            _key = new int[nmax];
            _value = new int[nmax];
            _filled = new bool[nmax];
            if ((key != null)) {
                for (int i = 0; (i<nold); ++i) {
                    if (filled[i]) {
                        put(key[i], value[i]);
                    }
                }
            }
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(256);
            buffer.append("{");
            Saxon.Hej.z.IntIterator keys = keyIterator();
            int count = 0;
            while (keys.hasNext()) {
                int k = keys.next();
                int v = get(k);
                buffer.append(((((" "+k)+":")+v)+","));
                if ((count++>=100)) {
                    buffer.append("....");
                    break ;
                }
            }
            buffer.setCharAt((buffer.length()-1), '}');
            return buffer.ToString();
        }
        private class IntToIntHashMapKeyIterator : Saxon.Hej.z.IntIterator {
            private readonly Saxon.Hej.z.IntToIntHashMap map;
            private int i = 0;
            public IntToIntHashMapKeyIterator(Saxon.Hej.z.IntToIntHashMap map)  {
                this.map = map;
                i = 0;
            }
            public bool hasNext() {
                while ((i<map._key.Length)) {
                    if (map._filled[i]) {
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
    
    }
}