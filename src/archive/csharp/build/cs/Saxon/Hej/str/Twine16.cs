using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public class Twine16 : Saxon.Hej.str.UnicodeString {
        protected internal char[] chars;
        protected internal int cachedHash = 0;
        protected internal Twine16(char[] chars)  {
            this.chars = chars;
        }
        public Twine16(char[] chars, int start, int len)  {
            if ((start == 0)) {
                this.chars = Saxon.Ejava.util.Arrays.copyOf(chars, len);
            } else {
                this.chars = new char[len];
                Saxon.Ejava.lang.System.arraycopy(chars, start, this.chars, 0, len);
            }
        }
        public char[] getCharArray() {
            return chars;
        }
        public override long length() {
            return chars.Length;
        }
        public override int length32() {
            return chars.Length;
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
                    return new Saxon.Hej.str.Slice16(chars, start32, end32);
                }
            }
        }
        public override int codePointAt(long index) {
            int index32 = Saxon.Hej.str.StringTool.requireInt(index);
            if (((index32<0) || (index32>=length32()))) {
                throw new System.IndexOutOfRangeException();
            }
            return chars[index32];
        }
        public override long indexOf(int codePoint, long from) {
            if (((codePoint<0) || (codePoint>65535))) {
                return -1;
            }
            int from32 = System.Math.Max(requireInt(from), 0);
            int last = chars.Length;
            for (int i = from32; (i<last); i++) {
                if ((chars[i] == codePoint)) {
                    return i;
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
            return (chars.Length == 0);
        }
        internal override void copy16bit(char[] target, int offset) {
            Saxon.Ejava.lang.System.arraycopy(chars, 0, target, offset, chars.Length);
        }
        internal override void copy24bit(byte[] target, int offset) {
            for (int i = 0, j = offset; (i<chars.Length); ) {
                char c = chars[i++];
                target[j++] = 0;
                target[j++] = (byte)(((c>>8)));
                target[j++] = (byte)(((c&unchecked((int)0xff))));
            }
        }
        public override int getWidth() {
            return 16;
        }
        public override Saxon.Hej.z.IntIterator codePoints() {
            return new Inner_IntIterator_54(this);
        }
        public override int GetHashCode() {
            if ((cachedHash != 0)) {
                return cachedHash;
            }
            int h = 0;
            foreach (int cp in chars) {
                h = ((31*h)+cp);
            }
            return cachedHash = h;
        }
        public override bool Equals(System.Object o) {
            if ((o is Saxon.Hej.str.Twine16)) {
                return Saxon.Ejava.util.Arrays.Equals(chars, ((Saxon.Hej.str.Twine16)(o)).chars);
            }
            return base.Equals(o);
        }
        public override int compareTo(Saxon.Hej.str.UnicodeString other) {
            if ((other is Saxon.Hej.str.Twine16)) {
                Saxon.Hej.str.Twine16 o = (Saxon.Hej.str.Twine16)(other);
                char[] a = chars;
                char[] b = o.chars;
                int len = System.Math.Min(a.Length, b.Length);
                for (int i = 0; (i<len); i++) {
                    int diff = (a[i]-b[i]);
                    if ((diff != 0)) {
                        return diff;
                    }
                }
                return length().CompareTo(o.length());
            } else {
                return base.compareTo(other);
            }
        }
        public override string ToString() {
            return new string(chars);
        }
        public override long indexWhere(Saxon.Ejava.util.function.IntPredicate predicate, long from) {
            for (int i = requireInt(from); (i<length()); i++) {
                if (predicate(chars[i])) {
                    return i;
                }
            }
            return -1;
        }
        public string details() {
            return ("Twine16 length = "+chars.Length);
        }
    
        private class Inner_IntIterator_54 : Saxon.Hej.z.IntIterator {
            private readonly Twine16 outerInstance;
            public Inner_IntIterator_54(Twine16 outerInstance) {
                this.outerInstance = outerInstance;
                i = 0;
            }
            internal int i;
            public bool hasNext() {
                return (/*field*/i<outerInstance.chars.Length);
            }
            public int next() {
                return outerInstance.chars[/*field*/i++];
            }
        }

    }
}