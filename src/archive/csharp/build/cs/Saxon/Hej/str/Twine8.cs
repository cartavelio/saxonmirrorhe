using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public class Twine8 : Saxon.Hej.str.UnicodeString {
        protected internal byte[] bytes;
        protected internal int cachedHash = 0;
        public Twine8(byte[] bytes)  {
            this.bytes = bytes;
        }
        public Twine8(char[] chars, int start, int len)  {
            bytes = new byte[len];
            for (int i = start; (i<len); i++) {
                int c = chars[i];
                if (Saxon.Hej.Configuration.isAssertionsEnabled()) {
                    if ((c>255)) {
                        throw new Saxon.Ejava.lang.IllegalArgumentException();
                    }
                }
                bytes[i] = (byte)(((chars[i]&unchecked((int)0xff))));
            }
        }
        public Twine8(string str)  {
            bytes = fromString(str);
        }
        private static byte[] fromString(string str) {
            return System.Text.Encoding.Latin1.GetBytes(str);
        }
        public byte[] getByteArray() {
            return bytes;
        }
        public override long length() {
            return bytes.Length;
        }
        public override int length32() {
            return bytes.Length;
        }
        internal override void copy8bit(byte[] target, int offset) {
            Saxon.Ejava.lang.System.arraycopy(bytes, 0, target, offset, bytes.Length);
        }
        internal override void copy16bit(char[] target, int offset) {
            for (int i = 0, j = offset; (i<bytes.Length); ) {
                target[j++] = (char)(((bytes[i++]&unchecked((int)0xff))));
            }
        }
        internal override void copy24bit(byte[] target, int offset) {
            for (int i = 0, j = offset; (i<bytes.Length); ) {
                target[j++] = (byte)(0);
                target[j++] = (byte)(0);
                target[j++] = bytes[i++];
            }
        }
        public override Saxon.Hej.str.UnicodeString substring(long start, long end) {
            long len = length();
            if ((((start<0) || (end<start)) || (end>len))) {
                throw new System.IndexOutOfRangeException();
            }
            if ((end == start)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            } else {
                if (((start == 0) && (end == len))) {
                    return this;
                } else {
                    return new Saxon.Hej.str.Slice8(bytes, requireInt(start), requireInt(end));
                }
            }
        }
        public override int codePointAt(long index) {
            int index32 = Saxon.Hej.str.StringTool.requireInt(index);
            if (((index32<0) || (index32>=length32()))) {
                throw new System.IndexOutOfRangeException();
            }
            return ((int)(bytes[index32])&unchecked((int)0xff));
        }
        public override long indexOf(int codePoint, long from) {
            int from32 = Saxon.Hej.str.StringTool.requireInt(from);
            if ((from32<0)) {
                from32 = 0;
            } else {
                if ((from32>=length32())) {
                    return -1;
                }
            }
            int last = bytes.Length;
            if (((codePoint<0) || (codePoint>255))) {
                return -1;
            }
            for (int i = from32; (i<last); i++) {
                if ((((bytes[i]&unchecked((int)0xff))) == codePoint)) {
                    return i;
                }
            }
            return -1;
        }
        public override long indexOf(Saxon.Hej.str.UnicodeString other, long from) {
            int from32 = requireInt(from);
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
            return 8;
        }
        public override Saxon.Hej.z.IntIterator codePoints() {
            return new Inner_IntIterator_d1657e905(bytes);
        }
        public override int GetHashCode() {
            if ((cachedHash != 0)) {
                return cachedHash;
            }
            return cachedHash = base.GetHashCode();
        }
        public override bool Equals(System.Object o) {
            if ((o is Saxon.Hej.str.Twine8)) {
                Saxon.Hej.str.Twine8 other = (Saxon.Hej.str.Twine8)(o);
                if ((this.length32() != other.length32())) {
                    return false;
                }
                if ((this.GetHashCode() != other.GetHashCode())) {
                    return false;
                }
                return Saxon.Ejava.util.Arrays.Equals(bytes, other.bytes);
            }
            return base.Equals(o);
        }
        public override int compareTo(Saxon.Hej.str.UnicodeString other) {
            if ((other is Saxon.Hej.str.Twine8)) {
                Saxon.Hej.str.Twine8 o = (Saxon.Hej.str.Twine8)(other);
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
            return Saxon.Impl.Helpers.StringUtils.LATIN_1.GetString(bytes);
        }
        private void write(System.IO.TextWriter writer, long start, long len) {
            if ((writer is Saxon.Hej.serialize.UTF8Writer)) {
                ((Saxon.Hej.serialize.UTF8Writer)(writer)).writeLatin1(bytes, requireInt(start), requireInt(len));
            } else {
                writer.Write(ToString().Substring(requireInt(start), requireInt((start+len)) - requireInt(start)));
            }
        }
        public override long indexWhere(Saxon.Ejava.util.function.IntPredicate predicate, long from) {
            for (int i = requireInt(from); (i<length()); i++) {
                if (predicate((bytes[i]&unchecked((int)0xff)))) {
                    return i;
                }
            }
            return -1;
        }
        public string details() {
            return ("Twine8 bytes.length = "+bytes.Length);
        }
    
        private class Inner_IntIterator_d1657e905 : Saxon.Hej.z.IntIterator {
            public Inner_IntIterator_d1657e905(byte[] bytes) {
                this.bytes = bytes;
                i = 0;
            }
            private byte[] bytes;
            internal int i;
            public bool hasNext() {
                return (/*field*/i</*extraParam*/bytes.Length);
            }
            public int next() {
                return (/*extraParam*/bytes[/*field*/i++]&unchecked((int)0xff));
            }
        }

    }
}