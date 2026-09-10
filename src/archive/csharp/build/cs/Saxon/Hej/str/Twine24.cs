using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public class Twine24 : Saxon.Hej.str.UnicodeString {
        protected internal byte[] bytes;
        protected internal int cachedHash = 0;
        protected internal Twine24(byte[] bytes)  {
            this.bytes = bytes;
        }
        public Twine24(int[] codePoints)  {
            bytes = new byte[(codePoints.Length*3)];
            for (int i = 0, j = 0; (i<codePoints.Length); i++, j += 3) {
                int c = codePoints[i];
                bytes[j] = (byte)(((((c>>16))&unchecked((int)0xff))));
                bytes[(j+1)] = (byte)(((((c>>8))&unchecked((int)0xff))));
                bytes[(j+2)] = (byte)(((c&unchecked((int)0xff))));
            }
        }
        public byte[] getByteArray() {
            return bytes;
        }
        public override long length() {
            return (bytes.Length/3);
        }
        public override int length32() {
            return (bytes.Length/3);
        }
        public override Saxon.Hej.str.UnicodeString substring(long start, long end) {
            int start32 = Saxon.Hej.str.StringTool.requireInt(start);
            int end32 = Saxon.Hej.str.StringTool.requireInt(end);
            int len = length32();
            checkSubstringBounds(start, end);
            if ((end == start)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            } else {
                if (((start == 0) && (end == len))) {
                    return this;
                } else {
                    return new Saxon.Hej.str.Slice24(bytes, start32, end32);
                }
            }
        }
        public override int codePointAt(long index) {
            int index32 = Saxon.Hej.str.StringTool.requireInt(index);
            if (((index32<0) || (index32>=length32()))) {
                throw new System.IndexOutOfRangeException();
            }
            int offset = (index32*3);
            return ((((((bytes[offset]<<16)|(((bytes[(offset+1)]&unchecked((int)0xff)))<<8)))|((bytes[(offset+2)]&unchecked((int)0xff)))))&unchecked((int)0xffffff));
        }
        public override long indexOf(int code, long from) {
            int from32 = System.Math.Max(requireInt(from), 0);
            if ((from32>=length32())) {
                return -1;
            }
            int last = bytes.Length;
            if (((code<0) || (code>unchecked((int)0xffffff)))) {
                return -1;
            }
            byte a = (byte)((((code>>16)&unchecked((int)0xff))));
            byte b = (byte)((((code>>8)&unchecked((int)0xff))));
            byte c = (byte)(((code&unchecked((int)0xff))));
            for (int i = (from32*3); (i<last); i += 3) {
                if ((((bytes[(i+2)] == c) && (bytes[(i+1)] == b)) && (bytes[i] == a))) {
                    return ((i/3));
                }
            }
            return -1;
        }
        public override long indexOf(Saxon.Hej.str.UnicodeString other, long from) {
            int from32 = Saxon.Hej.str.StringTool.requireInt(from);
            if ((from32<0)) {
                from32 = 0;
            } else {
                if ((from32>=length32())) {
                    return -1;
                }
            }
            if (other.isEmpty()) {
                return from;
            }
            int initial = other.codePointAt(0);
            int len = Saxon.Hej.str.StringTool.requireInt(other.length());
            int lastPossible = (length32()-len);
            while ((from32<=lastPossible)) {
                int i = Saxon.Hej.str.StringTool.requireInt(indexOf(initial, from32));
                if ((i<0)) {
                    return -1;
                }
                if (hasSubstring(other, i)) {
                    return i;
                }
                from32 = (i+1);
            }
            return -1;
        }
        public override bool isEmpty() {
            return (bytes.Length == 0);
        }
        public override int getWidth() {
            return 24;
        }
        public override Saxon.Hej.z.IntIterator codePoints() {
            return new Inner_IntIterator_d1655e801(this);
        }
        public override int GetHashCode() {
            if ((cachedHash != 0)) {
                return cachedHash;
            }
            int h = 0;
            int end = bytes.Length;
            for (int i = 0; (i<end); i += 3) {
                int cp = ((((((bytes[i]<<16)|(((bytes[(i+1)]&unchecked((int)0xff)))<<8)))|((bytes[(i+2)]&unchecked((int)0xff)))))&unchecked((int)0xffffff));
                if ((((cp&unchecked((int)0xff0000))) != 0)) {
                    h = ((31*h)+Saxon.Hej.serialize.charcode.UTF16CharacterSet.highSurrogate(cp));
                    h = ((31*h)+Saxon.Hej.serialize.charcode.UTF16CharacterSet.lowSurrogate(cp));
                } else {
                    h = ((31*h)+cp);
                }
            }
            return cachedHash = h;
        }
        public override bool Equals(System.Object o) {
            if ((o is Saxon.Hej.str.Twine24)) {
                if ((GetHashCode() != o.GetHashCode())) {
                    return false;
                }
                return Saxon.Ejava.util.Arrays.Equals(bytes, ((Saxon.Hej.str.Twine24)(o)).bytes);
            }
            return base.Equals(o);
        }
        public override int compareTo(Saxon.Hej.str.UnicodeString other) {
            if ((other is Saxon.Hej.str.Twine24)) {
                Saxon.Hej.str.Twine24 o = (Saxon.Hej.str.Twine24)(other);
                byte[] a = bytes;
                byte[] b = o.bytes;
                int len = System.Math.Min(a.Length, b.Length);
                for (int i = 0; (i<len); i++) {
                    int diff = (((a[i]&unchecked((int)0xff)))-((b[i]&unchecked((int)0xff))));
                    if ((diff != 0)) {
                        return diff;
                    }
                }
                return a.Length.CompareTo(b.Length);
            } else {
                return base.compareTo(other);
            }
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(length32());
            Saxon.Hej.z.IntIterator iter = codePoints();
            while (iter.hasNext()) {
                int x = iter.next();
                sb.appendCodePoint(x);
            }
            return sb.ToString();
        }
        internal override void copy24bit(byte[] target, int offset) {
            Saxon.Ejava.lang.System.arraycopy(bytes, 0, target, offset, bytes.Length);
        }
        public override long indexWhere(Saxon.Ejava.util.function.IntPredicate predicate, long from) {
            for (int i = requireInt(from); (i<length()); i++) {
                int offset = (i*3);
                int cp = ((((((bytes[offset]<<16)|(((bytes[(offset+1)]&unchecked((int)0xff)))<<8)))|((bytes[(offset+2)]&unchecked((int)0xff)))))&unchecked((int)0xffffff));
                if (predicate(cp)) {
                    return i;
                }
            }
            return -1;
        }
        public string details() {
            return ("Twine24 bytes.length = "+bytes.Length);
        }
    
        private class Inner_IntIterator_d1655e801 : Saxon.Hej.z.IntIterator {
            private readonly Twine24 outerInstance;
            public Inner_IntIterator_d1655e801(Twine24 outerInstance) {
                this.outerInstance = outerInstance;
                i = 0;
            }
            internal int i;
            public bool hasNext() {
                return (/*field*/i<outerInstance.bytes.Length);
            }
            public int next() {
                int result = ((((((outerInstance.bytes[/*field*/i]&unchecked((int)0xff)))<<16))|((((outerInstance.bytes[(/*field*/i+1)]&unchecked((int)0xff)))<<8)))|(((outerInstance.bytes[(/*field*/i+2)]&unchecked((int)0xff)))));
                /*field*/i += 3;
                return /*localvar*/result;
            }
        }

    }
}