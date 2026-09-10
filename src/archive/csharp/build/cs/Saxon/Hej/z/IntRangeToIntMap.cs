using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntRangeToIntMap : Saxon.Hej.z.IntToIntMap {
        private int[] startPoints;
        private int[] endPoints;
        private int[] values;
        private int used = 0;
        private int count = 0;
        private int defaultValue = System.Int32.MinValue;
        public IntRangeToIntMap(int capacity)  {
            startPoints = new int[capacity];
            endPoints = new int[capacity];
            values = new int[capacity];
            used = 0;
            count = 0;
        }
        public void setDefaultValue(int defaultValue) {
            this.defaultValue = defaultValue;
        }
        public int getDefaultValue() {
            return this.defaultValue;
        }
        public IntRangeToIntMap(int[] startPoints, int[] endPoints)  {
            if ((startPoints.Length != endPoints.Length)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Array lengths differ");
            }
            this.startPoints = startPoints;
            this.endPoints = endPoints;
            used = startPoints.Length;
            for (int i = 0; (i<used); i++) {
                count += (((endPoints[i]-startPoints[i])+1));
            }
        }
        public void clear() {
            startPoints = new int[4];
            endPoints = new int[4];
            values = new int[4];
            used = 0;
        }
        public int size() {
            return count;
        }
        public bool isEmpty() {
            return (count == 0);
        }
        public bool contains(int value) {
            if ((used == 0)) {
                return false;
            }
            if ((value>endPoints[(used-1)])) {
                return false;
            }
            if ((value<startPoints[0])) {
                return false;
            }
            int i = 0;
            int j = used;
            do {
                int mid = (i+(((j-i))/2));
                if ((endPoints[mid]<value)) {
                    i = System.Math.Max(mid, (i+1));
                } else {
                    if ((startPoints[mid]>value)) {
                        j = System.Math.Min(mid, (j-1));
                    } else {
                        return true;
                    }
                }
            } while ((i != j));
            return false;
        }
        public int get(int value) {
            if ((used == 0)) {
                return getDefaultValue();
            }
            if ((value>endPoints[(used-1)])) {
                return getDefaultValue();
            }
            if ((value<startPoints[0])) {
                return getDefaultValue();
            }
            int i = 0;
            int j = used;
            do {
                int mid = (i+(((j-i))/2));
                if ((endPoints[mid]<value)) {
                    i = System.Math.Max(mid, (i+1));
                } else {
                    if ((startPoints[mid]>value)) {
                        j = System.Math.Min(mid, (j-1));
                    } else {
                        return values[mid];
                    }
                }
            } while ((i != j));
            return getDefaultValue();
        }
        public bool remove(int value) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("remove");
        }
        public void addEntry(int start, int end, int value) {
            ensureCapacity((used+1));
            startPoints[used] = start;
            endPoints[used] = end;
            values[used] = value;
            used++;
            count += (((end-start)+1));
        }
        private void ensureCapacity(int n) {
            if ((startPoints.Length<n)) {
                int[] s = new int[(startPoints.Length*2)];
                int[] e = new int[(startPoints.Length*2)];
                int[] v = new int[(startPoints.Length*2)];
                Saxon.Ejava.lang.System.arraycopy(startPoints, 0, s, 0, used);
                Saxon.Ejava.lang.System.arraycopy(endPoints, 0, e, 0, used);
                Saxon.Ejava.lang.System.arraycopy(values, 0, v, 0, used);
                startPoints = s;
                endPoints = e;
                values = v;
            }
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder((used*8));
            for (int i = 0; (i<used); i++) {
                sb.append(startPoints[i]).append("-").append(endPoints[i]).append(":").append(values[i]).append(",");
            }
            return sb.ToString();
        }
        public int[] getStartPoints() {
            return startPoints;
        }
        public int[] getEndPoints() {
            return endPoints;
        }
        public int getNumberOfRanges() {
            return used;
        }
        public void put(int key, int value) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public Saxon.Hej.z.IntIterator keyIterator() {
            Saxon.Hej.z.IntRangeSet irs = new Saxon.Hej.z.IntRangeSet(startPoints, endPoints);
            return irs.iterator();
        }
    
    }
}