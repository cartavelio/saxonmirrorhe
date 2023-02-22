using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class MonotonicIntSet : Saxon.Hej.z.IntSet {
        private int[] contents;
        private int used = 0;
        public MonotonicIntSet()  {
            contents = new int[4];
            used = 0;
        }
        public override Saxon.Hej.z.IntSet copy() {
            Saxon.Hej.z.MonotonicIntSet i2 = new Saxon.Hej.z.MonotonicIntSet();
            i2.contents = Saxon.Ejava.util.Arrays.copyOf(contents, used);
            i2.used = used;
            return i2;
        }
        public override Saxon.Hej.z.IntSet mutableCopy() {
            return copy();
        }
        public override bool isMutable() {
            return false;
        }
        public override void clear() {
            if ((contents.Length>(used+20))) {
                contents = new int[4];
            }
            used = 0;
        }
        public override int size() {
            return used;
        }
        public override bool isEmpty() {
            return (used == 0);
        }
        public override bool contains(int value) {
            return (Saxon.Ejava.util.Arrays.binarySearch(contents, 0, used, value)>=0);
        }
        public override bool remove(int value) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public override bool add(int value) {
            if ((used>0)) {
                int last = contents[(used-1)];
                if ((value == last)) {
                    return false;
                } else {
                    if ((value<last)) {
                        throw new Saxon.Ejava.lang.UnsupportedOperationException("Values must be added in monotonic order");
                    }
                }
            }
            if ((used == contents.Length)) {
                contents = Saxon.Ejava.util.Arrays.copyOf(contents, ((used == 0) ? 4 : (used*2)));
            }
            contents[used++] = value;
            return true;
        }
        public override Saxon.Hej.z.IntIterator iterator() {
            return new Saxon.Hej.z.IntArraySet.IntArrayIterator(contents, used);
        }
        public override Saxon.Hej.z.IntSet union(Saxon.Hej.z.IntSet other) {
            if ((size() == 0)) {
                return other.copy();
            } else {
                if (other.isEmpty()) {
                    return copy();
                } else {
                    if ((other == Saxon.Hej.z.IntUniversalSet.getInstance())) {
                        return other;
                    } else {
                        if ((other is Saxon.Hej.z.IntComplementSet)) {
                            return other.union(this);
                        }
                    }
                }
            }
            if (Equals(other)) {
                return copy();
            }
            if ((other is Saxon.Hej.z.MonotonicIntSet)) {
                int[] merged = new int[(size()+other.size())];
                int[] a = contents;
                int[] b = ((Saxon.Hej.z.MonotonicIntSet)(other)).contents;
                int m = used;
                int n = ((Saxon.Hej.z.MonotonicIntSet)(other)).used;
                int o = 0, i = 0, j = 0;
                while (true) {
                    if ((a[i]<b[j])) {
                        merged[o++] = a[i++];
                    } else {
                        if ((b[j]<a[i])) {
                            merged[o++] = b[j++];
                        } else {
                            merged[o++] = a[i++];
                            j++;
                        }
                    }
                    if ((i == m)) {
                        Saxon.Ejava.lang.System.arraycopy(b, j, merged, o, (n-j));
                        o += ((n-j));
                        return make(merged, o);
                    } else {
                        if ((j == n)) {
                            Saxon.Ejava.lang.System.arraycopy(a, i, merged, o, (m-i));
                            o += ((m-i));
                            return make(merged, o);
                        }
                    }
                }
            } else {
                return base.union(other);
            }
        }
        public static Saxon.Hej.z.MonotonicIntSet make(int[] @in, int size) {
            return new Saxon.Hej.z.MonotonicIntSet(@in, size);
        }
        private MonotonicIntSet(int[] content, int used)  {
            this.contents = content;
            this.used = used;
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder((used*4));
            for (int i = 0; (i<used); i++) {
                if ((i == (used-1))) {
                    sb.append(contents[i]);
                } else {
                    if (((contents[i]+1) != contents[(i+1)])) {
                        sb.append(contents[i]).append(",");
                    } else {
                        int j = (i+1);
                        while ((contents[j] == (contents[(j-1)]+1))) {
                            j++;
                            if ((j == used)) {
                                break ;
                            }
                        }
                        sb.append(contents[i]).append("-").append(contents[(j-1)]).append(",");
                        i = j;
                    }
                }
            }
            return sb.ToString();
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.z.MonotonicIntSet)) {
                Saxon.Hej.z.MonotonicIntSet s = (Saxon.Hej.z.MonotonicIntSet)(other);
                if ((used != s.used)) {
                    return false;
                }
                for (int i = 0; (i<used); i++) {
                    if ((contents[i] != s.contents[i])) {
                        return false;
                    }
                }
                return true;
            } else {
                return (((other is Saxon.Hej.z.IntSet) && (used == ((Saxon.Hej.z.IntSet)(other)).size())) && containsAll((Saxon.Hej.z.IntSet)(other)));
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
    
    }
}