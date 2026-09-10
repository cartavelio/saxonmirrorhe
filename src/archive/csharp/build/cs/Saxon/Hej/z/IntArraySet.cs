using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntArraySet : Saxon.Hej.z.IntSet {
        public static readonly int[] EMPTY_INT_ARRAY = new int[0];
        private int[] contents;
        private int _hashCode = -1;
        public IntArraySet()  {
            contents = EMPTY_INT_ARRAY;
        }
        public IntArraySet(Saxon.Hej.z.IntHashSet input)  {
            contents = input.getValues();
            Saxon.Ejava.util.Arrays.sort(contents);
        }
        public IntArraySet(Saxon.Hej.z.IntArraySet input)  {
            contents = new int[input.contents.Length];
            Saxon.Ejava.lang.System.arraycopy(input.contents, 0, contents, 0, contents.Length);
        }
        public override Saxon.Hej.z.IntSet copy() {
            Saxon.Hej.z.IntArraySet i2 = new Saxon.Hej.z.IntArraySet();
            i2.contents = new int[contents.Length];
            Saxon.Ejava.lang.System.arraycopy(contents, 0, i2.contents, 0, contents.Length);
            return i2;
        }
        public override Saxon.Hej.z.IntSet mutableCopy() {
            return copy();
        }
        public override void clear() {
            contents = EMPTY_INT_ARRAY;
            _hashCode = -1;
        }
        public override int size() {
            return contents.Length;
        }
        public override bool isEmpty() {
            return (contents.Length == 0);
        }
        public int[] getValues() {
            return contents;
        }
        public override bool contains(int value) {
            return (Saxon.Ejava.util.Arrays.binarySearch(contents, value)>=0);
        }
        public override bool remove(int value) {
            _hashCode = -1;
            int pos = Saxon.Ejava.util.Arrays.binarySearch(contents, value);
            if ((pos<0)) {
                return false;
            }
            int[] newArray = new int[(contents.Length-1)];
            if ((pos>0)) {
                Saxon.Ejava.lang.System.arraycopy(contents, 0, newArray, 0, pos);
            }
            if ((pos<newArray.Length)) {
                Saxon.Ejava.lang.System.arraycopy(contents, (pos+1), newArray, pos, (contents.Length-pos));
            }
            contents = newArray;
            return true;
        }
        public override bool add(int value) {
            _hashCode = -1;
            if ((contents.Length == 0)) {
                contents = new int[]{value};
                return true;
            }
            int pos = Saxon.Ejava.util.Arrays.binarySearch(contents, value);
            if ((pos>=0)) {
                return false;
            }
            pos = (-(pos)-1);
            int[] newArray = new int[(contents.Length+1)];
            if ((pos>0)) {
                Saxon.Ejava.lang.System.arraycopy(contents, 0, newArray, 0, pos);
            }
            newArray[pos] = value;
            if ((pos<contents.Length)) {
                Saxon.Ejava.lang.System.arraycopy(contents, pos, newArray, (pos+1), (newArray.Length-pos));
            }
            contents = newArray;
            return true;
        }
        public int getFirst() {
            return contents[0];
        }
        public override Saxon.Hej.z.IntIterator iterator() {
            return new Saxon.Hej.z.IntArraySet.IntArrayIterator(contents, contents.Length);
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
            if ((other is Saxon.Hej.z.IntArraySet)) {
                int[] merged = new int[(size()+other.size())];
                int[] a = contents;
                int[] b = ((Saxon.Hej.z.IntArraySet)(other)).contents;
                int m = a.Length, n = b.Length;
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
        public static Saxon.Hej.z.IntArraySet make(int[] @in, int size) {
            int[] @out;
            if ((@in.Length == size)) {
                @out = @in;
            } else {
                @out = new int[size];
                Saxon.Ejava.lang.System.arraycopy(@in, 0, @out, 0, size);
            }
            return new Saxon.Hej.z.IntArraySet(@out);
        }
        private IntArraySet(int[] content)  {
            contents = content;
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder((contents.Length*4));
            for (int i = 0; (i<contents.Length); i++) {
                if ((i == (contents.Length-1))) {
                    sb.append((contents[i]+""));
                } else {
                    if (((contents[i]+1) != contents[(i+1)])) {
                        sb.append((contents[i]+","));
                    } else {
                        int j = (i+1);
                        while ((contents[j] == (contents[(j-1)]+1))) {
                            j++;
                            if ((j == contents.Length)) {
                                break ;
                            }
                        }
                        sb.append((((contents[i]+"-")+contents[(j-1)])+","));
                        i = (j-1);
                    }
                }
            }
            return sb.ToString();
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.z.IntArraySet)) {
                Saxon.Hej.z.IntArraySet s = (Saxon.Hej.z.IntArraySet)(other);
                return ((GetHashCode() == other.GetHashCode()) && Saxon.Ejava.util.Arrays.Equals(contents, s.contents));
            } else {
                return (((other is Saxon.Hej.z.IntSet) && (contents.Length == ((Saxon.Hej.z.IntSet)(other)).size())) && containsAll((Saxon.Hej.z.IntSet)(other)));
            }
        }
        public override int GetHashCode() {
            if ((_hashCode == -1)) {
                int h = 936247625;
                Saxon.Hej.z.IntIterator it = iterator();
                while (it.hasNext()) {
                    h += it.next();
                }
                _hashCode = h;
            }
            return _hashCode;
        }
        public class IntArrayIterator : Saxon.Hej.z.IntIterator {
            private readonly int[] contents;
            private readonly int limit;
            private int i = 0;
            public IntArrayIterator(int[] contents, int limit)  {
                i = 0;
                this.contents = contents;
                this.limit = limit;
            }
            public bool hasNext() {
                return (i<limit);
            }
            public int next() {
                return contents[i++];
            }
        
        }
    
    }
}