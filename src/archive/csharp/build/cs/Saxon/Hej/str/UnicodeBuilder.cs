using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public sealed class UnicodeBuilder : Saxon.Hej.str.UniStringConsumer, Saxon.Hej.str.UnicodeWriter {
        private int[] codepoints;
        private int used;
        private int bits;
        private Saxon.Hej.str.ZenoString archive = Saxon.Hej.str.ZenoString.EMPTY;
        public UnicodeBuilder() : this (256) {
        }
        public UnicodeBuilder(int allocate)  {
            codepoints = new int[allocate];
        }
        public Saxon.Hej.str.UnicodeBuilder append(char ch) {
            append((int)(ch));
            return this;
        }
        public Saxon.Hej.str.UnicodeBuilder append(int codePoint) {
            ensureCapacity(1);
            codepoints[used++] = codePoint;
            bits |= codePoint;
            return this;
        }
        public Saxon.Hej.str.UnicodeBuilder append(Saxon.Hej.z.IntIterator codePoints) {
            while (codePoints.hasNext()) {
                append(codePoints.next());
            }
            return this;
        }
        public Saxon.Hej.str.UnicodeBuilder appendLatin(string str) {
            return append(new Saxon.Hej.str.BMPString(str));
        }
        public Saxon.Hej.str.UnicodeBuilder appendAll(Saxon.Hej.om.SequenceIterator iter) {
            for (Saxon.Hej.om.Item item; ((item = iter.next()) != null); ) {
                append(item.getUnicodeStringValue());
            }
            return this;
        }
        public Saxon.Hej.str.UnicodeBuilder append(string str) {
            return append(Saxon.Hej.str.StringTool.fromCharSequence(str));
        }
        public Saxon.Hej.str.UnicodeBuilder append(Saxon.Hej.str.UnicodeString str) {
            int len = str.length32();
            if ((len == 0)) {
                return this;
            }
            ensureCapacity(len);
            str.copy32bit(codepoints, used);
            used += len;
            int width = str.getWidth();
            if ((width>8)) {
                if ((width>16)) {
                    bits |= unchecked((int)0xffffff);
                } else {
                    bits |= unchecked((int)0xffff);
                }
            }
            return this;
        }
        public long length() {
            return (archive.length()+used);
        }
        public bool isEmpty() {
            return (archive.isEmpty() && (used == 0));
        }
        private void ensureCapacity(int required) {
            if ((used>65535)) {
                archive = archive.concat(getActivePart());
                used = 0;
                bits = unchecked((int)0xff);
            }
            while (((used+required)>codepoints.Length)) {
                codepoints = Saxon.Ejava.util.Arrays.copyOf(codepoints, (codepoints.Length*2));
            }
        }
        public Saxon.Hej.str.UnicodeString toUnicodeString() {
            if (archive.isEmpty()) {
                return getActivePart();
            } else {
                return archive.concat(getActivePart());
            }
        }
        private Saxon.Hej.str.UnicodeString getActivePart() {
            if ((((bits&unchecked((int)0xff0000))) != 0)) {
                return new Saxon.Hej.str.Twine24(codepoints, used);
            } else {
                if ((((bits&unchecked((int)0xff00))) != 0)) {
                    char[] chars = new char[used];
                    for (int i = 0; (i<used); i++) {
                        chars[i] = (char)(((codepoints[i]&unchecked((int)0xffff))));
                    }
                    return new Saxon.Hej.str.Twine16(chars);
                } else {
                    byte[] bytes = new byte[used];
                    for (int i = 0; (i<used); i++) {
                        bytes[i] = (byte)(((codepoints[i]&unchecked((int)0xff))));
                    }
                    return new Saxon.Hej.str.Twine8(bytes);
                }
            }
        }
        public Saxon.Hej.value.StringValue toStringItem(Saxon.Hej.type.AtomicType type) {
            return new Saxon.Hej.value.StringValue(toUnicodeString(), type);
        }
        public override string ToString() {
            return toUnicodeString().ToString();
        }
        public void clear() {
            archive = Saxon.Hej.str.ZenoString.EMPTY;
            used = 0;
            bits = 0;
        }
        public static byte[] expand1to2(byte[] @in, int start, int used, int allocate) {
            byte[] result = new byte[(allocate*2)];
            for (int i = start, j = 0; (i<used); ) {
                result[j++] = 0;
                result[j++] = @in[i++];
            }
            return result;
        }
        public static char[] expandBytesToChars(byte[] @in, int start, int end) {
            char[] result = new char[(end-start)];
            for (int i = start, j = 0; (i<end); ) {
                result[j++] = (char)(@in[i++]);
            }
            return result;
        }
        public static byte[] expand1to3(byte[] @in, int start, int used, int allocate) {
            byte[] result = new byte[(allocate*3)];
            for (int i = start, j = 0; (i<used); ) {
                result[j++] = 0;
                result[j++] = 0;
                result[j++] = @in[i++];
            }
            return result;
        }
        public static byte[] expand2to3(byte[] @in, int start, int used, int allocate) {
            byte[] result = new byte[(allocate*3)];
            for (int i = start, j = 0; (i<used); ) {
                result[j++] = 0;
                result[j++] = @in[i++];
                result[j++] = @in[i++];
            }
            return result;
        }
        public static byte[] expand(byte[] @in, int start, int end, int oldWidth, int newWidth, int allocate) {
            if ((allocate<=(((end-start))/oldWidth))) {
                allocate = (((end-start))/oldWidth);
            }
            if ((newWidth<=oldWidth)) {
                byte[] @out = new byte[(allocate*newWidth)];
                Saxon.Ejava.lang.System.arraycopy(@in, start, @out, 0, (end*oldWidth));
                return @out;
            }
            if (((oldWidth == 1) && (newWidth == 2))) {
                return expand1to2(@in, start, end, allocate);
            }
            if (((oldWidth == 1) && (newWidth == 3))) {
                return expand1to3(@in, start, end, allocate);
            }
            if (((oldWidth == 2) && (newWidth == 3))) {
                return expand2to3(@in, start, end, allocate);
            }
            throw new Saxon.Ejava.lang.IllegalArgumentException();
        }
        public Saxon.Hej.str.UniStringConsumer accept(Saxon.Hej.str.UnicodeString chars) {
            return append(chars);
        }
        public void write(Saxon.Hej.str.UnicodeString chars) {
            append(chars);
        }
        public void writeAscii(byte[] content) {
            accept(new Saxon.Hej.str.Twine8(content));
        }
        public void write(string chars) {
            append(chars);
        }
        public void trimToSize() {
            codepoints = Saxon.Ejava.util.Arrays.copyOf(codepoints, used);
        }
        public void close() {
        }
    
        /* Expanding default method writeCodePoint from interface UnicodeWriter */
        public void writeCodePoint(int codepoint) {
            write(new Saxon.Hej.str.UnicodeChar(codepoint));
        }

        /* Expanding default method writeRepeatedAscii from interface UnicodeWriter */
        public void writeRepeatedAscii(byte asciiChar, int count) {
            System.Diagnostics.Debug.Assert((asciiChar>=0));
            byte[] array = new byte[count];
            Saxon.Ejava.util.Arrays.fill(array, asciiChar);
            writeAscii(array);
        }

        /* Expanding default method flush from interface UnicodeWriter */
        public void flush() {
        }

        /* Expanding default method open from interface UniStringConsumer */
        public void open() {
        }

    }
}