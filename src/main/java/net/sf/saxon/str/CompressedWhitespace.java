////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.str;

import net.sf.saxon.serialize.UTF8Writer;
import net.sf.saxon.z.IntIterator;

import java.io.IOException;
import java.io.Writer;
import java.util.function.IntPredicate;

/**
 * This class provides a compressed representation of a sequence of whitespace characters. The representation
 * is a sequence of bytes: in each byte the top two bits indicate which whitespace character is used
 * (x9, xA, xD, or x20) and the bottom six bits indicate the number of such characters. A zero byte is a filler.
 * We don't compress the sequence if it would occupy more than 8 bytes, because that's the space we've got available
 * in the TinyTree arrays.
 */

public class CompressedWhitespace extends UnicodeString {

    /*@NotNull*/ private static final char[] WHITE_CHARS = {0x09, 0x0A, 0x0D, 0x20};
    /*@NotNull*/ private static final int[] CODES =
            {-1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 1, -1, -1, 2, -1, -1,
                    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                    3};

    private final long value;

    public CompressedWhitespace(long compressedValue) {
        value = compressedValue;
    }

    public static UnicodeString compressWS(char[] in, int start, int len) {
        int runlength = 1;
        int outlength = 0;
        for (int i = 0; i < len; i++) {
            final char c = in[i];
            if (c <= 32 && CODES[c] >= 0) {
                if (i == len - 1 || c != in[i + 1] || runlength == 63) {
                    runlength = 1;
                    outlength++;
                    if (outlength > 8) {
                        return StringTool.compress(in, start, len, false);
                    }
                } else {
                    runlength++;
                }
            } else {
                return StringTool.compress(in, start, len, false);
            }
        }
        int ix = 0;
        runlength = 1;
        int[] out = new int[outlength];
        for (int i = 0; i < len; i++) {
            final char c = in[i];
            if (i == len - 1 || c != in[i + 1] || runlength == 63) {
                out[ix++] = (CODES[c] << 6) | runlength;
                runlength = 1;
            } else {
                runlength++;
            }
        }
        long value = 0;
        for (int i = 0; i < outlength; i++) {
            value = (value << 8) | out[i];
        }
        value <<= 8 * (8 - outlength);
        return new CompressedWhitespace(value);
    }

    /**
     * Make a compressed whitespace text node suitable for inserting indentation into
     * serialized output. (When the output is UTF8, the output writer has a method for
     * outputting repeated characters that uses {@code Arrays.fill()} to directly
     * populate the output buffer.)
     * @param newlines the number of newlines to include at the start (generally zero or one)
     * @param spaces the number of spaces to include
     * @return a {@link UnicodeString} representing the whitespace
     */
    public static CompressedWhitespace makeIndent(int newlines, int spaces) {
        newlines = Math.min(newlines, 63);
        spaces = Math.min(spaces, 63*7);
        int outlength = 0;
        long result = 0L;

        if (newlines > 0) {
            result = CODES[10]<<6 | newlines;
            outlength++;
        }
        while (spaces > 0) {
            int run = spaces % 63;
            result = (result << 8) | (CODES[32] << 6) | run;
            spaces -= run;
            outlength++;
        }
        result <<= 8 * (8 - outlength);
        return new CompressedWhitespace(result);
    }

    /**
     * Uncompress the whitespace to a (normal) UniString
     * @return the uncompressed value
     */

    public UnicodeString uncompress() {
        return uncompress(value);
    }

    public static UnicodeString uncompress(long value) {
        UnicodeBuilder buffer = new UnicodeBuilder();
        uncompress(value, buffer);
        return buffer.toUnicodeString();
    }


    public static void uncompress(long value, UnicodeBuilder buffer) {
        for (int s = 56; s >= 0; s -= 8) {
            byte b = (byte) ((value >>> s) & 0xff);
            if (b == 0) {
                break;
            }
            char c = WHITE_CHARS[b >>> 6 & 0x3];
            int len = b & 0x3f;
            for (int j = 0; j < len; j++) {
                buffer.append(c);
            }
        }
    }

    public long getCompressedValue() {
        return value;
    }

    @Override
    public long length() {
        return length(value);
    }

    @Override
    public int length32() {
        return length(value);
    }

    public static int length(long value) {
        int count = 0;
        for (int s = 56; s >= 0; s -= 8) {
            int c = (int) ((value >>> s) & 0x3f);
            if (c == 0) {
                break;
            }
            count += c;
        }
        return count;
    }

    @Override
    public int getWidth() {
        return 7;
    }

    /**
     * Returns the <code>char</code> value at the specified index.  An index ranges from zero
     * to <tt>length() - 1</tt>.  The first <code>char</code> value of the sequence is at
     * index zero, the next at index one, and so on, as for array
     * indexing.
     * <p>If the <code>char</code> value specified by the index is a
     * <a href="Character.html#unicode">surrogate</a>, the surrogate
     * value is returned.</p>
     *
     * @param index the index of the <code>char</code> value to be returned
     * @return the specified <code>char</code> value
     * @throws IndexOutOfBoundsException if the <tt>index</tt> argument is negative or not less than
     *                                   <tt>length()</tt>
     */
    @Override
    public int codePointAt(long index) {
        int count = 0;
        for (int s = 56; s >= 0; s -= 8) {
            byte b = (byte) ((value >>> s) & 0xff);
            if (b == 0) {
                break;
            }
            count += b & 0x3f;
            if (count > index) {
                return WHITE_CHARS[b >>> 6 & 0x3];
            }
        }
        throw new IndexOutOfBoundsException(index + "");
    }

    /**
     * Returns a new <code>CharSequence</code> that is a subsequence of this sequence.
     * The subsequence starts with the <code>char</code> value at the specified index and
     * ends with the <code>char</code> value at index <tt>end - 1</tt>.  The length
     * (in <code>char</code>s) of the
     * returned sequence is <tt>end - start</tt>, so if <tt>start == end</tt>
     * then an empty sequence is returned.
     *
     * @param start the start index, inclusive
     * @param end   the end index, exclusive
     * @return the specified subsequence
     * @throws IndexOutOfBoundsException if <tt>start</tt> or <tt>end</tt> are negative,
     *                                   if <tt>end</tt> is greater than <tt>length()</tt>,
     *                                   or if <tt>start</tt> is greater than <tt>end</tt>
     */
    @Override
    public UnicodeString substring(long start, long end) {
        return uncompress().substring(start, end);
    }

    @Override
    public UnicodeString tidy() {
        return uncompress();
    }

    @Override
    public IntIterator codePoints() {
        return uncompress().codePoints();
    }

    @Override
    public long indexOf(int codePoint, long from) {
        return uncompress().indexOf(codePoint, from);
    }

    /**
     * Indicates whether some other object is "equal to" this one.
     */
    public boolean equals(/*@NotNull*/ Object obj) {
        if (obj instanceof CompressedWhitespace) {
            return value == ((CompressedWhitespace) obj).value;
        }
        return uncompress().equals(obj);
    }

    /**
     * Returns a hash code value for the object.
     */
    public int hashCode() {
        return uncompress().hashCode();
    }

    /**
     * Returns a string representation of the object.
     */
    public String toString() {
        return uncompress().toString();
    }

    /**
     * Write the value to a Writer
     *
     * @param writer the writer to write to
     * @throws java.io.IOException if an error occurs downstream
     */

    @Override
    public void write(/*@NotNull*/ Writer writer) throws java.io.IOException {
        if (writer instanceof UTF8Writer) {
            UTF8Writer uw = (UTF8Writer)writer;
            for (int s = 56; s >= 0; s -= 8) {
                final byte b = (byte) ((value >>> s) & 0xff);
                if (b == 0) {
                    break;
                }
                final char c = WHITE_CHARS[b >>> 6 & 0x3];
                final int len = b & 0x3f;
                uw.writeRepeatedAscii((byte) c, len);
            }
        } else {
            for (int s = 56; s >= 0; s -= 8) {
                final byte b = (byte) ((value >>> s) & 0xff);
                if (b == 0) {
                    break;
                }
                final char c = WHITE_CHARS[b >>> 6 & 0x3];
                final int len = b & 0x3f;
                for (int j = 0; j < len; j++) {
                    writer.write(c);
                }
            }
        }
    }

    @Override
    public void supplyContent(LargeTextBuffer consumer, long start, long length) {
        uncompress().supplyContent(consumer, start, length);
    }

    @Override
    public void write(Writer writer, long start, long len) throws IOException {
        uncompress().write(writer, start, len);
    }

    @Override
    public long writeUntil(Writer writer, long start, long limit, IntPredicate stop) throws IOException {
        return uncompress().writeUntil(writer, start, limit, stop);
    }

    /**
     * Write the value to a Writer with escaping of special characters
     *
     * @param specialChars identifies which characters are considered special
     * @param writer       the writer to write to
     * @throws java.io.IOException if an error occurs downstream
     */

    public void writeEscape(boolean[] specialChars, Writer writer) throws java.io.IOException {
        for (int s = 56; s >= 0; s -= 8) {
            final byte b = (byte) ((value >>> s) & 0xff);
            if (b == 0) {
                break;
            }
            final char c = WHITE_CHARS[b >>> 6 & 0x3];
            final int len = b & 0x3f;
            if (specialChars[c]) {
                String e = "";
                if (c == '\n') {
                    e = "&#xA;";
                } else if (c == '\r') {
                    e = "&#xD;";
                } else if (c == '\t') {
                    e = "&#x9;";
                }
                for (int j = 0; j < len; j++) {
                    writer.write(e);
                }
            } else {
                for (int j = 0; j < len; j++) {
                    writer.write(c);
                }
            }
        }
    }
}

