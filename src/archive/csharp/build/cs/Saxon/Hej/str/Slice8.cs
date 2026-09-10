using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public class Slice8 : Saxon.Hej.str.UnicodeString {
        private readonly byte[] bytes;
        private readonly int start;
        private readonly int end;
        private int cachedHash;
        public Slice8(byte[] bytes, int start, int end)  {
            this.bytes = bytes;
            this.start = start;
            this.end = end;
        }
        public override long length() {
            return (end-start);
        }
        public override int getWidth() {
            return 8;
        }
        public byte[] getByteArray() {
            return bytes;
        }
        public int getStart() {
            return start;
        }
        public int getEnd() {
            return end;
        }
        public override long indexOf(int codePoint, long from) {
            if ((codePoint>255)) {
                return -1;
            }
            byte b = (byte)(((codePoint&unchecked((int)0xff))));
            for (int i = (start+requireInt(from)); (i<end); i++) {
                if ((bytes[i] == b)) {
                    return (i-start);
                }
            }
            return -1;
        }
        public override int codePointAt(long index) {
            return ((bytes[(start+requireInt(index))])&unchecked((int)0xff));
        }
        public override Saxon.Hej.str.UnicodeString substring(long start, long end) {
            checkSubstringBounds(start, end);
            if ((end == start)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            } else {
                return new Saxon.Hej.str.Slice8(bytes, (requireInt(start)+this.start), (requireInt(end)+this.start));
            }
        }
        private void write(System.IO.TextWriter writer, long start, long len) {
            if ((writer is Saxon.Hej.serialize.UTF8Writer)) {
                ((Saxon.Hej.serialize.UTF8Writer)(writer)).writeLatin1(bytes, (this.start+requireInt(start)), requireInt(len));
            } else {
                writer.Write(substring(requireInt(start), requireInt((start+len))).ToString());
            }
        }
        public override long indexWhere(Saxon.Ejava.util.function.IntPredicate predicate, long from) {
            for (int i = (requireInt(from)+start); (i<end); i++) {
                if (predicate((bytes[i]&unchecked((int)0xff)))) {
                    return (i-start);
                }
            }
            return -1;
        }
        public override Saxon.Hej.z.IntIterator codePoints() {
            return new Inner_IntIterator_d1584e515(this);
        }
        internal override void copy8bit(byte[] target, int offset) {
            Saxon.Ejava.lang.System.arraycopy(bytes, start, target, offset, (end-start));
        }
        internal override void copy16bit(char[] target, int offset) {
            for (int i = start, j = offset; (i<end); ) {
                target[j++] = (char)(((bytes[i++]&unchecked((int)0xff))));
            }
        }
        internal override void copy24bit(byte[] target, int offset) {
            for (int i = start, j = offset; (i<end); ) {
                target[j++] = (byte)(0);
                target[j++] = (byte)(0);
                target[j++] = bytes[i++];
            }
        }
        public override int GetHashCode() {
            if ((cachedHash != 0)) {
                return cachedHash;
            }
            int h = 0;
            for (int i = start; (i<end); i++) {
                byte b = bytes[i];
                h = ((31*h)+((b&unchecked((int)0xff))));
            }
            return cachedHash = h;
        }
        public override string ToString() {
            return Saxon.Impl.Helpers.StringUtils.LATIN_1.GetString(bytes, start, end - start);
        }
    
        private class Inner_IntIterator_d1584e515 : Saxon.Hej.z.IntIterator {
            private readonly Slice8 outerInstance;
            public Inner_IntIterator_d1584e515(Slice8 outerInstance) {
                this.outerInstance = outerInstance;
                i = outerInstance.start;
            }
            internal int i;
            public bool hasNext() {
                return (/*field*/i<outerInstance.end);
            }
            public int next() {
                return (outerInstance.bytes[/*field*/i++]&unchecked((int)0xff));
            }
        }

    }
}