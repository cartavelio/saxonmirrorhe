using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public class Slice16 : Saxon.Hej.str.UnicodeString {
        private char[] chars;
        private int start;
        private int end;
        private int cachedHash;
        public Slice16(char[] chars, int start, int end)  {
            this.chars = chars;
            this.start = start;
            this.end = end;
        }
        public override long length() {
            return (end-start);
        }
        public override int getWidth() {
            return 16;
        }
        public char[] getCharArray() {
            return chars;
        }
        public int getStart() {
            return start;
        }
        public int getEnd() {
            return end;
        }
        public override long indexOf(int codePoint, long from) {
            if ((codePoint>65535)) {
                return -1;
            }
            char b = (char)(((codePoint&unchecked((int)0xffff))));
            int limit = end;
            for (int i = (start+System.Math.Max(requireInt(from), 0)); (i<limit); i++) {
                if ((chars[i] == b)) {
                    return (i-start);
                }
            }
            return -1;
        }
        public override int codePointAt(long index) {
            return (chars[(start+requireInt(index))]);
        }
        public override Saxon.Hej.str.UnicodeString substring(long start, long end) {
            checkSubstringBounds(start, end);
            if ((end == start)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            } else {
                return new Saxon.Hej.str.Slice16(chars, (requireInt(start)+this.start), (requireInt(end)+this.start));
            }
        }
        private void write(System.IO.TextWriter writer, long start, long len) {
            writer.Write(chars, (this.start+requireInt(start)), requireInt(len));
        }
        public override long indexWhere(Saxon.Ejava.util.function.IntPredicate predicate, long from) {
            for (int i = (requireInt(from)+start); (i<end); i++) {
                if (predicate(chars[i])) {
                    return (i-start);
                }
            }
            return -1;
        }
        internal override void copy16bit(char[] target, int offset) {
            Saxon.Ejava.lang.System.arraycopy(chars, start, target, offset, (end-start));
        }
        internal override void copy24bit(byte[] target, int offset) {
            for (int i = start, j = offset; (i<end); ) {
                char c = chars[i++];
                target[j++] = 0;
                target[j++] = (byte)(((c>>8)));
                target[j++] = (byte)(((c&unchecked((int)0xff))));
            }
        }
        public override Saxon.Hej.z.IntIterator codePoints() {
            return new Inner_IntIterator_38(this);
        }
        public override int GetHashCode() {
            if ((cachedHash != 0)) {
                return cachedHash;
            }
            int h = 0;
            for (int i = start; (i<end); i++) {
                int b = chars[i];
                h = ((31*h)+b);
            }
            return cachedHash = h;
        }
        public override string ToString() {
            return new string(chars, start, (end-start));
        }
    
        private class Inner_IntIterator_38 : Saxon.Hej.z.IntIterator {
            private readonly Slice16 outerInstance;
            public Inner_IntIterator_38(Slice16 outerInstance) {
                this.outerInstance = outerInstance;
                i = outerInstance.start;
            }
            internal int i;
            public bool hasNext() {
                return (/*field*/i<outerInstance.end);
            }
            public int next() {
                return outerInstance.chars[/*field*/i++];
            }
        }

    }
}