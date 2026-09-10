using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public class Slice24 : Saxon.Hej.str.UnicodeString {
        private byte[] bytes;
        private int start;
        private int end;
        private int cachedHash;
        public Slice24(byte[] bytes, int start, int end)  {
            this.bytes = bytes;
            this.start = start;
            this.end = end;
        }
        public override long length() {
            return (end-start);
        }
        public override int getWidth() {
            return 24;
        }
        public override long indexOf(int codePoint, long from) {
            byte b0 = (byte)(((((codePoint>>16))&unchecked((int)0xff))));
            byte b1 = (byte)(((((codePoint>>8))&unchecked((int)0xff))));
            byte b2 = (byte)(((codePoint&unchecked((int)0xff))));
            for (int i = (((start+System.Math.Max(requireInt(from), 0)))*3); (i<(end*3)); i += 3) {
                if ((((bytes[(i+2)] == b2) && (bytes[(i+1)] == b1)) && (bytes[i] == b0))) {
                    return (((i-start))/3);
                }
            }
            return -1;
        }
        public override long indexWhere(Saxon.Ejava.util.function.IntPredicate predicate, long from) {
            for (int i = (((start+requireInt(from)))*3); (i<(end*3)); i += 3) {
                int cp = ((((((bytes[i]<<16)|(((bytes[(i+1)]&unchecked((int)0xff)))<<8)))|((bytes[(i+2)]&unchecked((int)0xff)))))&unchecked((int)0xffffff));
                if (predicate(cp)) {
                    return ((i/3)-start);
                }
            }
            return -1;
        }
        public override int codePointAt(long index) {
            int index32 = requireInt(index);
            if (((index32<0) || (index32>=length32()))) {
                throw new System.IndexOutOfRangeException();
            }
            int offset = (((start+index32))*3);
            return ((((((bytes[offset]<<16)|(((bytes[(offset+1)]&unchecked((int)0xff)))<<8)))|((bytes[(offset+2)]&unchecked((int)0xff)))))&unchecked((int)0xffffff));
        }
        public override Saxon.Hej.str.UnicodeString substring(long start, long end) {
            checkSubstringBounds(start, end);
            if ((end == start)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            } else {
                return new Saxon.Hej.str.Slice24(bytes, (requireInt(start)+this.start), (requireInt(end)+this.start));
            }
        }
        internal override void copy24bit(byte[] target, int offset) {
            Saxon.Ejava.lang.System.arraycopy(bytes, (start*3), target, offset, (((end-start))*3));
        }
        public override Saxon.Hej.z.IntIterator codePoints() {
            return new Inner_IntIterator_d1643e585(this);
        }
        public override int GetHashCode() {
            if ((cachedHash != 0)) {
                return cachedHash;
            }
            int h = 0;
            for (int i = (start*3); (i<(end*3)); i += 3) {
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
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(length32());
            Saxon.Hej.z.IntIterator iter = codePoints();
            while (iter.hasNext()) {
                int x = iter.next();
                sb.appendCodePoint(x);
            }
            return sb.ToString();
        }
    
        private class Inner_IntIterator_d1643e585 : Saxon.Hej.z.IntIterator {
            private readonly Slice24 outerInstance;
            public Inner_IntIterator_d1643e585(Slice24 outerInstance) {
                this.outerInstance = outerInstance;
                i = (outerInstance.start*3);
                j = (outerInstance.end*3);
            }
            internal int i;
            internal int j;
            public bool hasNext() {
                return (/*field*/i</*field*/j);
            }
            public int next() {
                int result = ((((((outerInstance.bytes[/*field*/i]&unchecked((int)0xff)))<<16))|((((outerInstance.bytes[(/*field*/i+1)]&unchecked((int)0xff)))<<8)))|(((outerInstance.bytes[(/*field*/i+2)]&unchecked((int)0xff)))));
                /*field*/i += 3;
                return /*localvar*/result;
            }
        }

    }
}