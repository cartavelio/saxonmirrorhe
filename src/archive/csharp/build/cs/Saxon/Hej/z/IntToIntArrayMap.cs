using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntToIntArrayMap : Saxon.Hej.z.IntToIntMap {
        private int[] keys;
        private int[] values;
        private int used = 0;
        private int defaultValue = System.Int32.MinValue;
        public IntToIntArrayMap()  {
            keys = new int[8];
            values = new int[8];
        }
        public IntToIntArrayMap(int capacity)  {
            if ((capacity<=0)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("capacity <= 0");
            }
            keys = new int[capacity];
            values = new int[capacity];
        }
        public void clear() {
            used = 0;
        }
        public bool contains(int key) {
            for (int i = 0; (i<used); i++) {
                if ((keys[i] == key)) {
                    return true;
                }
            }
            return false;
        }
        public int get(int key) {
            for (int i = 0; (i<used); i++) {
                if ((keys[i] == key)) {
                    return values[i];
                }
            }
            return defaultValue;
        }
        public int getDefaultValue() {
            return defaultValue;
        }
        public Saxon.Hej.z.IntIterator keyIterator() {
            return new Saxon.Hej.z.IntToIntArrayMap.KeyIterator(this);
        }
        public void put(int key, int value) {
            for (int i = 0; (i<used); i++) {
                if ((keys[i] == key)) {
                    values[i] = value;
                    return ;
                }
            }
            if ((used>=keys.Length)) {
                int[] k2 = new int[(used*2)];
                Saxon.Ejava.lang.System.arraycopy(keys, 0, k2, 0, used);
                keys = k2;
                int[] v2 = new int[(used*2)];
                Saxon.Ejava.lang.System.arraycopy(values, 0, v2, 0, used);
                values = v2;
            }
            keys[used] = key;
            values[used++] = value;
        }
        public bool remove(int key) {
            for (int i = 0; (i<used); i++) {
                if ((keys[i] == key)) {
                    values[i] = defaultValue;
                    return true;
                }
            }
            return false;
        }
        public void setDefaultValue(int defaultValue) {
            this.defaultValue = defaultValue;
        }
        public int size() {
            return used;
        }
        private class KeyIterator : Saxon.Hej.z.IntIterator {
            private Saxon.Hej.z.IntToIntArrayMap map;
            private int i = 0;
            public KeyIterator(Saxon.Hej.z.IntToIntArrayMap map)  {
                this.map = map;
                i = 0;
            }
            public bool hasNext() {
                return (i<map.used);
            }
            public int next() {
                return map.keys[i++];
            }
        
        }
    
    }
}