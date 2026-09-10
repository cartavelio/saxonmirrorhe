using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public class CompressedWhitespace : Saxon.Hej.str.WhitespaceString {
        private static readonly char[] WHITE_CHARS = {'\t', '\n', '\r', ' '};
        private static readonly int[] CODES = {-1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 1, -1, -1, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 3};
        private readonly long value;
        public CompressedWhitespace(long compressedValue)  {
            value = compressedValue;
        }
        public static Saxon.Hej.str.UnicodeString compressWS(char[] @in, int start, int len) {
            int runlength = 1;
            int outlength = 0;
            for (int i = 0; (i<len); i++) {
                char c = @in[i];
                if (((c<=32) && (CODES[c]>=0))) {
                    if ((((i == (len-1)) || (c != @in[(i+1)])) || (runlength == 63))) {
                        runlength = 1;
                        outlength++;
                        if ((outlength>8)) {
                            return Saxon.Hej.str.StringTool.compress(@in, start, len, false);
                        }
                    } else {
                        runlength++;
                    }
                } else {
                    return Saxon.Hej.str.StringTool.compress(@in, start, len, false);
                }
            }
            int ix = 0;
            runlength = 1;
            int[] @out = new int[outlength];
            for (int i = 0; (i<len); i++) {
                char c = @in[i];
                if ((((i == (len-1)) || (c != @in[(i+1)])) || (runlength == 63))) {
                    @out[ix++] = (((CODES[c]<<6))|runlength);
                    runlength = 1;
                } else {
                    runlength++;
                }
            }
            long value = 0;
            for (int i = 0; (i<outlength); i++) {
                value = (((value<<8))|(long)(@out[i]));
            }
            value = (value<<((8*((8-outlength)))));
            return new Saxon.Hej.str.CompressedWhitespace(value);
        }
        public override Saxon.Hej.str.UnicodeString uncompress() {
            return uncompress(value);
        }
        public static Saxon.Hej.str.UnicodeString uncompress(long value) {
            byte[] bytes = new byte[1000];
            int offset = 0;
            for (int s = 56; (s>=0); s -= 8) {
                byte b = (byte)(((((value>>s))&unchecked((int)0xff))));
                if ((b == 0)) {
                    break ;
                }
                byte c = (byte)(((WHITE_CHARS[((b>>6)&0x3)]&unchecked((int)0xff))));
                int len = (b&0x3f);
                for (int j = 0; (j<len); j++) {
                    bytes[offset++] = c;
                }
            }
            return new Saxon.Hej.str.Twine8(Saxon.Ejava.util.Arrays.copyOf(bytes, offset));
        }
        public long getCompressedValue() {
            return value;
        }
        public override long length() {
            return length(value);
        }
        public override int length32() {
            return length(value);
        }
        public static int length(long value) {
            int count = 0;
            for (int s = 56; (s>=0); s -= 8) {
                int c = (int)(((((value>>s))&0x3f)));
                if ((c == 0)) {
                    break ;
                }
                count += c;
            }
            return count;
        }
        public override int codePointAt(long index) {
            int count = 0;
            for (int s = 56; (s>=0); s -= 8) {
                byte b = (byte)(((((value>>s))&unchecked((int)0xff))));
                if ((b == 0)) {
                    break ;
                }
                count += (b&0x3f);
                if ((count>index)) {
                    return WHITE_CHARS[((b>>6)&0x3)];
                }
            }
            throw new System.IndexOutOfRangeException((index+""));
        }
        public override Saxon.Hej.z.IntIterator codePoints() {
            return uncompress().codePoints();
        }
        public override bool Equals(System.Object obj) {
            if ((obj is Saxon.Hej.str.CompressedWhitespace)) {
                return (value == ((Saxon.Hej.str.CompressedWhitespace)(obj)).value);
            }
            return uncompress().Equals(obj);
        }
        public override int GetHashCode() {
            return base.GetHashCode();
        }
        public override void write(Saxon.Hej.str.UnicodeWriter writer) {
            for (int s = 56; (s>=0); s -= 8) {
                byte b = (byte)(((((value>>s))&unchecked((int)0xff))));
                if ((b == 0)) {
                    break ;
                }
                char c = WHITE_CHARS[((b>>6)&0x3)];
                int len = (b&0x3f);
                writer.writeRepeatedAscii((byte)(c), len);
            }
        }
        public override void writeEscape(bool[] specialChars, Saxon.Hej.str.UnicodeWriter writer) {
            for (int s = 56; (s>=0); s -= 8) {
                byte b = (byte)(((((value>>s))&unchecked((int)0xff))));
                if ((b == 0)) {
                    break ;
                }
                char c = WHITE_CHARS[((b>>6)&0x3)];
                int len = (b&0x3f);
                if (specialChars[c]) {
                    byte[] e = null;
                    if ((c == '\n')) {
                        e = Saxon.Hej.str.StringConstants.ESCAPE_NL;
                    } else {
                        if ((c == '\r')) {
                            e = Saxon.Hej.str.StringConstants.ESCAPE_CR;
                        } else {
                            if ((c == '\t')) {
                                e = Saxon.Hej.str.StringConstants.ESCAPE_TAB;
                            }
                        }
                    }
                    System.Diagnostics.Debug.Assert((e != null));
                    for (int j = 0; (j<len); j++) {
                        writer.writeAscii(e);
                    }
                } else {
                    writer.writeRepeatedAscii((byte)(c), len);
                }
            }
        }
    
    }
}