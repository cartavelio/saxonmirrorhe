using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public abstract class UnicodeString : Saxon.Hej.expr.sort.AtomicMatchKey, Saxon.Ejava.lang.Comparable<Saxon.Hej.str.UnicodeString> {
        public virtual Saxon.Hej.str.UnicodeString tidy() {
            return this;
        }
        public virtual Saxon.Hej.str.UnicodeString economize() {
            return this;
        }
        public abstract long length();
        public virtual int length32() {
            return requireInt(length());
        }
        public virtual long estimatedLength() {
            return length();
        }
        public virtual bool isEmpty() {
            return (length() == 0);
        }
        public abstract int getWidth();
        public virtual long indexOf(int codePoint) {
            return indexOf(codePoint, 0);
        }
        public abstract long indexOf(int codePoint, long from);
        public virtual long indexWhere(Saxon.Ejava.util.function.IntPredicate predicate, long from) {
            Saxon.Hej.z.IntIterator iter = codePoints();
            long i = 0;
            while (iter.hasNext()) {
                int ch = iter.next();
                if (((i>=from) && predicate(ch))) {
                    return i;
                }
                i++;
            }
            return -1;
        }
        public virtual long indexOf(Saxon.Hej.str.UnicodeString other, long from) {
            if (((from<0) || (from>=length()))) {
                return -1;
            }
            if (other.isEmpty()) {
                return from;
            }
            int initial = other.codePointAt(0);
            long len = other.length();
            long lastPossible = (length()-len);
            while ((from<=lastPossible)) {
                long i = indexOf(initial, from);
                if ((i<0)) {
                    return -1;
                }
                if (hasSubstring(other, i)) {
                    return i;
                }
                from = (i+1);
            }
            return -1;
        }
        public bool hasSubstring(Saxon.Hej.str.UnicodeString other, long offset) {
            if (((offset<0) || (offset>length()))) {
                throw new System.IndexOutOfRangeException();
            }
            long len = other.length();
            if (((len+offset)>length())) {
                return false;
            }
            for (long k = 0; (k<len); k++) {
                if ((codePointAt((offset+k)) != other.codePointAt(k))) {
                    return false;
                }
            }
            return true;
        }
        public abstract Saxon.Hej.z.IntIterator codePoints();
        public abstract int codePointAt(long index);
        public Saxon.Hej.str.UnicodeString substring(long start) {
            return substring(start, length());
        }
        public abstract Saxon.Hej.str.UnicodeString substring(long start, long end);
        public Saxon.Hej.str.UnicodeString prefix(long end) {
            return substring(0, end);
        }
        public virtual Saxon.Hej.str.UnicodeString concat(Saxon.Hej.str.UnicodeString other) {
            return Saxon.Hej.str.ZenoString.of(this).concat(other);
        }
        protected internal void checkSubstringBounds(long start, long end) {
            if ((start<0)) {
                throw new System.IndexOutOfRangeException("UnicodeString.substring(): start < 0");
            }
            if ((end<start)) {
                throw new System.IndexOutOfRangeException("UnicodeString.substring(): end < start");
            }
            if ((end>length())) {
                throw new System.IndexOutOfRangeException("UnicodeString.substring(): end > length()");
            }
        }
        public void verifyCharacters() {
            Saxon.Hej.z.IntIterator iter = codePoints();
            int p = 0;
            while (iter.hasNext()) {
                int x = iter.next();
                if (!(Saxon.Hej.serialize.charcode.XMLCharacterData.isValid11(x))) {
                    throw new Saxon.Ejava.lang.IllegalStateException(((((("Invalid char "+x)+" in ")+this.GetType())+" at offset ")+p));
                }
                p++;
            }
        }
        public override bool Equals(System.Object obj) {
            if ((obj is Saxon.Hej.str.UnicodeString)) {
                Saxon.Hej.str.UnicodeString other = (Saxon.Hej.str.UnicodeString)(obj);
                Saxon.Hej.z.IntIterator iter1 = codePoints();
                Saxon.Hej.z.IntIterator iter2 = other.codePoints();
                while (true) {
                    bool more1 = iter1.hasNext();
                    bool more2 = iter2.hasNext();
                    if ((more1 && more2)) {
                        int ch1 = iter1.next();
                        int ch2 = iter2.next();
                        if ((ch1 != ch2)) {
                            return false;
                        }
                    } else {
                        return !(((more1 || more2)));
                    }
                }
            } else {
                return false;
            }
        }
        public override int GetHashCode() {
            int h = 0;
            Saxon.Hej.z.IntIterator iter = codePoints();
            while (iter.hasNext()) {
                int cp = iter.next();
                if ((((cp&unchecked((int)0xff0000))) != 0)) {
                    h = ((31*h)+Saxon.Hej.serialize.charcode.UTF16CharacterSet.highSurrogate(cp));
                    h = ((31*h)+Saxon.Hej.serialize.charcode.UTF16CharacterSet.lowSurrogate(cp));
                } else {
                    h = ((31*h)+cp);
                }
            }
            return h;
        }
        public virtual int compareTo(Saxon.Hej.str.UnicodeString other) {
            Saxon.Hej.z.IntIterator iter1 = codePoints();
            Saxon.Hej.z.IntIterator iter2 = other.codePoints();
            while (true) {
                bool more1 = iter1.hasNext();
                bool more2 = iter2.hasNext();
                if ((more1 && more2)) {
                    int ch1 = iter1.next();
                    int ch2 = iter2.next();
                    int diff = (ch1-ch2);
                    if ((diff != 0)) {
                        return diff;
                    }
                } else {
                    if ((!(more1) && !(more2))) {
                        return 0;
                    } else {
                        return (more1 ? 1 : -1);
                    }
                }
            }
        }
        private byte[] getCodepointCollationKey() {
            Saxon.Hej.str.UnicodeString prep = tidy();
            int len = requireInt(prep.length());
            byte[] result = new byte[(len*3)];
            Saxon.Hej.z.IntIterator iter = prep.codePoints();
            int j = 0;
            while (iter.hasNext()) {
                int c = iter.next();
                result[j++] = (byte)(((c>>16)));
                result[j++] = (byte)(((c>>8)));
                result[j++] = (byte)(c);
            }
            return result;
        }
        public Saxon.Hej.value.AtomicValue asAtomic() {
            return new Saxon.Hej.value.Base64BinaryValue(getCodepointCollationKey());
        }
        public static int requireInt(long value) {
            if ((value>System.Int32.MaxValue)) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("String offset exceeds 2^31 characters");
            }
            return (int)(value);
        }
        internal virtual void copy8bit(byte[] target, int offset) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        internal virtual void copy16bit(char[] target, int offset) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        internal virtual void copy24bit(byte[] target, int offset) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
    
    }
}