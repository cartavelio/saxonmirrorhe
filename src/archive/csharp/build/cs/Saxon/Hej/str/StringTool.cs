using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public class StringTool {
        public static int getStringLength(string s) {
            int n = 0;
            for (int i = 0; (i<s.Length); i++) {
                int c = s[i];
                if (((c<55296) || (c>56319))) {
                    n++;
                }
            }
            return n;
        }
        public static int[] expand(Saxon.Hej.str.UnicodeString s) {
            int[] array = new int[s.length32()];
            Saxon.Hej.z.IntIterator iter = s.codePoints();
            int i = 0;
            while (iter.hasNext()) {
                array[i++] = iter.next();
            }
            return array;
        }
        public static Saxon.Hej.str.UnicodeString fromCodePoints(int[] codes, int used) {
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder();
            for (int i = 0; (i<used); i++) {
                sb.append(codes[i]);
            }
            return sb.toUnicodeString();
        }
        public static Saxon.Hej.str.UnicodeString fromCharSequence(string chars) {
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder();
            Saxon.Hej.z.IntIterator iter = codePoints(chars);
            while (iter.hasNext()) {
                sb.append(iter.next());
            }
            return sb.toUnicodeString();
        }
        public static Saxon.Hej.str.UnicodeString fromLatin1(string str) {
            byte[] bytes = new byte[str.Length];
            for (int i = 0; (i<str.Length); i++) {
                bytes[i] = (byte)(((str[i]&unchecked((int)0xff))));
            }
            return new Saxon.Hej.str.Twine8(bytes);
        }
        public static Saxon.Hej.z.IntIterator codePoints(string value) {
            return new Inner_IntIterator_d1587e383(value);
        }
        public static string diagnosticDisplay(string s) {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(s.Length);
            for (int i = 0, len = s.Length; (i<len); i++) {
                char c = s[i];
                if (((c>=0x20) && (c<=0x7e))) {
                    fsb.append(c);
                } else {
                    fsb.append("\\u");
                    for (int shift = 12; (shift>=0); shift -= 4) {
                        fsb.append("0123456789ABCDEF"[(((c>>shift))&0xF)]);
                    }
                }
            }
            return fsb.ToString();
        }
        public static void prependWideChar(Saxon.Ejava.lang.StringBuilder builder, int ch) {
            if ((ch>unchecked((int)0xffff))) {
                char[] pair = new char[]{Saxon.Hej.serialize.charcode.UTF16CharacterSet.highSurrogate(ch), Saxon.Hej.serialize.charcode.UTF16CharacterSet.lowSurrogate(ch)};
                builder.insert(0, pair);
            } else {
                builder.insert(0, (char)(ch));
            }
        }
        public static void prependRepeated(Saxon.Ejava.lang.StringBuilder builder, char ch, int count) {
            char[] array = new char[count];
            Saxon.Ejava.util.Arrays.fill(array, ch);
            builder.insert(0, array);
        }
        public static void appendRepeated(Saxon.Ejava.lang.StringBuilder builder, char ch, int count) {
            for (int i = 0; (i<count); i++) {
                builder.append(ch);
            }
        }
        public static int lastCodePoint(Saxon.Hej.str.UnicodeString str) {
            return str.codePointAt((str.length()-1));
        }
        public static long lastIndexOf(Saxon.Hej.str.UnicodeString str, int codePoint) {
            for (long i = (str.length()-1); (i>=0); i--) {
                if ((str.codePointAt(i) == codePoint)) {
                    return i;
                }
            }
            return -(1L);
        }
        public static int requireInt(long value) {
            if ((value>System.Int32.MaxValue)) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("String exceeds 2^31 characters");
            }
            return (int)(value);
        }
        public static Saxon.Hej.str.UnicodeString compress(char[] @in, int offset, int len, bool compressWS) {
            if ((len == 0)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
            int max = 255;
            int end = (offset+len);
            bool allWhite = compressWS;
            int surrogates = 0;
            for (int i = offset; (i<end); i++) {
                int c = @in[i];
                max |= c;
                if ((allWhite && !(Saxon.Hej.value.Whitespace.isWhite(c)))) {
                    allWhite = false;
                }
                if (Saxon.Hej.serialize.charcode.UTF16CharacterSet.isSurrogate(c)) {
                    surrogates++;
                }
            }
            if (allWhite) {
                return Saxon.Hej.str.CompressedWhitespace.compressWS(@in, offset, end);
            }
            if ((max<256)) {
                byte[] array = new byte[len];
                for (int i = offset, j = 0; (i<end); ) {
                    array[j++] = (byte)(@in[i++]);
                }
                return new Saxon.Hej.str.Twine8(array);
            }
            if ((surrogates == 0)) {
                char[] array = Saxon.Ejava.util.Arrays.copyOfRange(@in, offset, (offset+len));
                return new Saxon.Hej.str.Twine16(array);
            } else {
                byte[] array = new byte[(3*((len-(surrogates/2))))];
                for (int i = offset, j = 0; (i<end); ) {
                    char c = @in[i++];
                    if (Saxon.Hej.serialize.charcode.UTF16CharacterSet.isSurrogate(c)) {
                        int cp = Saxon.Hej.serialize.charcode.UTF16CharacterSet.combinePair(c, @in[i++]);
                        array[j++] = (byte)(((((cp&unchecked((int)0xffffff)))>>16)));
                        array[j++] = (byte)(((((cp&unchecked((int)0xffff)))>>8)));
                        array[j++] = (byte)(((cp&unchecked((int)0xff))));
                    } else {
                        array[j++] = (byte)(0);
                        array[j++] = (byte)(((((c&unchecked((int)0xffff)))>>8)));
                        array[j++] = (byte)(((c&unchecked((int)0xff))));
                    }
                }
                return new Saxon.Hej.str.Twine24(array);
            }
        }
        public static void copy8to16(byte[] source, int sourcePos, char[] dest, int destPos, int count) {
            int last = (sourcePos+count);
            for (int i = sourcePos, j = destPos; (i<last); ) {
                dest[j++] = (char)(((source[i++]&unchecked((int)0xff))));
            }
        }
        public static void copy8to24(byte[] source, int sourcePos, byte[] dest, int destPos, int count) {
            int last = (sourcePos+count);
            for (int i = sourcePos, j = (destPos*3); (i<last); ) {
                dest[j++] = 0;
                dest[j++] = 0;
                dest[j++] = source[i++];
            }
        }
        public static void copy16to24(char[] source, int sourcePos, byte[] dest, int destPos, int count) {
            int last = (sourcePos+count);
            for (int i = sourcePos, j = (destPos*3); (i<last); ) {
                char c = source[i++];
                dest[j++] = 0;
                dest[j++] = (byte)(((((c>>8))&unchecked((int)0xff))));
                dest[j++] = (byte)(((c&unchecked((int)0xff))));
            }
        }
    
        private class Inner_IntIterator_d1587e383 : Saxon.Hej.z.IntIterator {
            public Inner_IntIterator_d1587e383(string value) {
                this.value = value;
                i = 0;
            }
            private string value;
            internal int i;
            internal bool expectingLowSurrogate;
            public bool hasNext() {
                return (/*field*/i</*extraParam*/value.Length);
            }
            public int next() {
                int c = /*extraParam*/value[/*field*/i++];
                if (Saxon.Hej.serialize.charcode.UTF16CharacterSet.isHighSurrogate(/*localvar*/c)) {
                    try {
                        int d = (/*method*/hasNext() ? /*extraParam*/value[/*field*/i++] : -1);
                        if (!(Saxon.Hej.serialize.charcode.UTF16CharacterSet.isLowSurrogate(/*localvar*/d))) {
                            throw new Saxon.Ejava.lang.IllegalStateException(((("Unmatched surrogate code value "+/*localvar*/c)+" at position ")+/*field*/i));
                        }
                        return Saxon.Hej.serialize.charcode.UTF16CharacterSet.combinePair((char)(/*localvar*/c), (char)(/*localvar*/d));
                    } catch (System.IndexOutOfRangeException e) {
                        System.Console.Error.WriteLine("Invalid surrogate at end of string");
                        System.Console.Error.WriteLine(/*exception*/e);
                        throw;
                    }
                } else {
                    return /*localvar*/c;
                }
            }
        }

    }
}