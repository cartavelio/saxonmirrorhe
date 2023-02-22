using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntRangeSet : Saxon.Hej.z.IntSet {
        private int[] startPoints;
        private int[] endPoints;
        private int used = 0;
        private int _hashCode = -1;
        private int count = 0;
        public IntRangeSet()  {
            startPoints = new int[4];
            endPoints = new int[4];
            used = 0;
            count = 0;
            _hashCode = -1;
        }
        public IntRangeSet(Saxon.Hej.z.IntRangeSet input)  {
            startPoints = new int[input.used];
            endPoints = new int[input.used];
            used = input.used;
            Saxon.Ejava.lang.System.arraycopy(input.startPoints, 0, startPoints, 0, used);
            Saxon.Ejava.lang.System.arraycopy(input.endPoints, 0, endPoints, 0, used);
            _hashCode = input._hashCode;
        }
        public IntRangeSet(int[] startPoints, int[] endPoints)  {
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
        public override void clear() {
            startPoints = new int[4];
            endPoints = new int[4];
            used = 0;
            _hashCode = -1;
        }
        public override Saxon.Hej.z.IntSet copy() {
            Saxon.Hej.z.IntRangeSet s = new Saxon.Hej.z.IntRangeSet();
            s.startPoints = new int[startPoints.Length];
            Saxon.Ejava.lang.System.arraycopy(startPoints, 0, s.startPoints, 0, startPoints.Length);
            s.endPoints = new int[endPoints.Length];
            Saxon.Ejava.lang.System.arraycopy(endPoints, 0, s.endPoints, 0, endPoints.Length);
            s.used = used;
            s.count = count;
            return s;
        }
        public override Saxon.Hej.z.IntSet mutableCopy() {
            return copy();
        }
        public override bool isMutable() {
            return false;
        }
        public override int size() {
            return count;
        }
        public override bool isEmpty() {
            return (count == 0);
        }
        public override bool contains(int value) {
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
        public override bool remove(int value) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("remove");
        }
        public override bool add(int value) {
            _hashCode = -1;
            if ((used == 0)) {
                ensureCapacity(1);
                startPoints[(used-1)] = value;
                endPoints[(used-1)] = value;
                count++;
                return true;
            }
            if ((value>endPoints[(used-1)])) {
                if ((value == (endPoints[(used-1)]+1))) {
                    endPoints[(used-1)]++;
                } else {
                    ensureCapacity((used+1));
                    startPoints[(used-1)] = value;
                    endPoints[(used-1)] = value;
                }
                count++;
                return true;
            }
            if ((value<startPoints[0])) {
                if ((value == (startPoints[0]-1))) {
                    startPoints[0]--;
                } else {
                    ensureCapacity((used+1));
                    Saxon.Ejava.lang.System.arraycopy(startPoints, 0, startPoints, 1, (used-1));
                    Saxon.Ejava.lang.System.arraycopy(endPoints, 0, endPoints, 1, (used-1));
                    startPoints[0] = value;
                    endPoints[0] = value;
                }
                count++;
                return true;
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
                        return false;
                    }
                }
            } while ((i != j));
            if (((i>0) && ((endPoints[(i-1)]+1) == value))) {
                i--;
            } else {
                if (((i<(used-1)) && ((startPoints[(i+1)]-1) == value))) {
                    i++;
                }
            }
            if (((endPoints[i]+1) == value)) {
                if ((value == (startPoints[(i+1)]-1))) {
                    endPoints[i] = endPoints[(i+1)];
                    Saxon.Ejava.lang.System.arraycopy(startPoints, (i+2), startPoints, (i+1), ((used-i)-2));
                    Saxon.Ejava.lang.System.arraycopy(endPoints, (i+2), endPoints, (i+1), ((used-i)-2));
                    used--;
                } else {
                    endPoints[i]++;
                }
                count++;
                return true;
            } else {
                if (((startPoints[i]-1) == value)) {
                    if ((value == (endPoints[(i-1)]+1))) {
                        endPoints[(i-1)] = endPoints[i];
                        Saxon.Ejava.lang.System.arraycopy(startPoints, (i+1), startPoints, i, ((used-i)-1));
                        Saxon.Ejava.lang.System.arraycopy(endPoints, (i+1), endPoints, i, ((used-i)-1));
                        used--;
                    } else {
                        startPoints[i]--;
                    }
                    count++;
                    return true;
                } else {
                    if ((value>endPoints[i])) {
                        i++;
                    }
                    ensureCapacity((used+1));
                    try {
                        Saxon.Ejava.lang.System.arraycopy(startPoints, i, startPoints, (i+1), ((used-i)-1));
                        Saxon.Ejava.lang.System.arraycopy(endPoints, i, endPoints, (i+1), ((used-i)-1));
                    } catch (System.Exception err) {
                        System.Console.Error.WriteLine(err);
                    }
                    startPoints[i] = value;
                    endPoints[i] = value;
                    count++;
                    return true;
                }
            }
        }
        private void ensureCapacity(int n) {
            if ((startPoints.Length<n)) {
                int[] s = new int[(startPoints.Length*2)];
                int[] e = new int[(startPoints.Length*2)];
                Saxon.Ejava.lang.System.arraycopy(startPoints, 0, s, 0, used);
                Saxon.Ejava.lang.System.arraycopy(endPoints, 0, e, 0, used);
                startPoints = s;
                endPoints = e;
            }
            used = n;
        }
        public override Saxon.Hej.z.IntIterator iterator() {
            return new Saxon.Hej.z.IntRangeSet.IntRangeSetIterator(this);
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder((used*8));
            for (int i = 0; (i<used); i++) {
                sb.append((((startPoints[i]+"-")+endPoints[i])+","));
            }
            return sb.ToString();
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.z.IntSet)) {
                if ((other is Saxon.Hej.z.IntRangeSet)) {
                    return (((used == ((Saxon.Hej.z.IntRangeSet)(other)).used) && Saxon.Ejava.util.Arrays.Equals(startPoints, ((Saxon.Hej.z.IntRangeSet)(other)).startPoints)) && Saxon.Ejava.util.Arrays.Equals(endPoints, ((Saxon.Hej.z.IntRangeSet)(other)).endPoints));
                } else {
                    return containsAll((Saxon.Hej.z.IntSet)(other));
                }
            } else {
                return false;
            }
        }
        public override int GetHashCode() {
            if ((_hashCode == -1)) {
                int h = 0x436a89f1;
                for (int i = 0; (i<used); i++) {
                    h ^= (startPoints[i]+((endPoints[i]<<3)));
                }
                _hashCode = h;
            }
            return _hashCode;
        }
        public void addRange(int low, int high) {
            if ((low == high)) {
                add(low);
                return ;
            }
            _hashCode = -1;
            if ((used == 0)) {
                ensureCapacity(1);
                startPoints[(used-1)] = low;
                endPoints[(used-1)] = high;
                count += (((high-low)+1));
            } else {
                if ((low>endPoints[(used-1)])) {
                    if ((low == (endPoints[(used-1)]+1))) {
                        endPoints[(used-1)] = high;
                    } else {
                        ensureCapacity((used+1));
                        startPoints[(used-1)] = low;
                        endPoints[(used-1)] = high;
                    }
                    count += (((high-low)+1));
                } else {
                    if ((high<startPoints[0])) {
                        ensureCapacity((used+1));
                        Saxon.Ejava.lang.System.arraycopy(startPoints, 0, startPoints, 1, (used-1));
                        Saxon.Ejava.lang.System.arraycopy(endPoints, 0, endPoints, 1, (used-1));
                        startPoints[0] = low;
                        endPoints[0] = high;
                    } else {
                        for (int i = 1; (i<used); i++) {
                            if (((startPoints[i]>high) && (endPoints[(i-1)]<low))) {
                                ensureCapacity((used+1));
                                Saxon.Ejava.lang.System.arraycopy(startPoints, i, startPoints, (i+1), ((used-i)-1));
                                Saxon.Ejava.lang.System.arraycopy(endPoints, i, endPoints, (i+1), ((used-i)-1));
                                startPoints[i] = low;
                                endPoints[i] = high;
                                return ;
                            }
                        }
                        for (int i = low; (i<=high); i++) {
                            add(i);
                        }
                    }
                }
            }
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
        private class IntRangeSetIterator : Saxon.Hej.z.IntIterator {
            private Saxon.Hej.z.IntRangeSet intRangeSet;
            private int i = 0;
            private int current = 0;
            public IntRangeSetIterator(Saxon.Hej.z.IntRangeSet intRangeSet)  {
                this.intRangeSet = intRangeSet;
                i = -1;
                current = System.Int32.MinValue;
            }
            public bool hasNext() {
                if ((i<0)) {
                    return (intRangeSet.count>0);
                } else {
                    return (current<intRangeSet.endPoints[(intRangeSet.used-1)]);
                }
            }
            public int next() {
                if ((i<0)) {
                    i = 0;
                    current = intRangeSet.startPoints[0];
                    return current;
                }
                if ((current == intRangeSet.endPoints[i])) {
                    current = intRangeSet.startPoints[++i];
                    return current;
                } else {
                    return ++current;
                }
            }
        
        }
    
    }
}