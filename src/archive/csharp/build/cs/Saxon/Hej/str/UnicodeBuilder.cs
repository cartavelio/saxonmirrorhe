using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public sealed class UnicodeBuilder : Saxon.Hej.str.UniStringConsumer, Saxon.Hej.str.UnicodeWriter {
        internal Saxon.Hej.str.ZenoString content = Saxon.Hej.str.ZenoString.of(Saxon.Hej.str.EmptyUnicodeString.getInstance());
        public UnicodeBuilder()  {
        }
        public UnicodeBuilder(int allocate)  {
        }
        public Saxon.Hej.str.UnicodeBuilder append(char ch) {
            content = content.concat(new Saxon.Hej.str.UnicodeChar(ch));
            return this;
        }
        public Saxon.Hej.str.UnicodeBuilder append(int codePoint) {
            content = content.concat(new Saxon.Hej.str.UnicodeChar(codePoint));
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
            Saxon.Hej.om.Item item;
            while (((item = iter.next()) != null)) {
                append(item.getUnicodeStringValue());
            }
            return this;
        }
        public Saxon.Hej.str.UnicodeBuilder append(string str) {
            return append(Saxon.Hej.str.StringTool.codePoints(str));
        }
        public Saxon.Hej.str.UnicodeBuilder append(Saxon.Hej.str.UnicodeString str) {
            content = content.concat(str);
            return this;
        }
        public long length() {
            return content.length();
        }
        public bool isEmpty() {
            return content.isEmpty();
        }
        public Saxon.Hej.str.UnicodeString toUnicodeString() {
            return content.economize();
        }
        public Saxon.Hej.value.StringValue toStringItem(Saxon.Hej.type.AtomicType type) {
            return new Saxon.Hej.value.StringValue(toUnicodeString(), type);
        }
        public override string ToString() {
            return content.ToString();
        }
        public void clear() {
            content = Saxon.Hej.str.ZenoString.of(Saxon.Hej.str.EmptyUnicodeString.getInstance());
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