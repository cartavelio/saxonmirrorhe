package net.sf.saxon.str;

import net.sf.saxon.z.IntIterator;
import net.sf.saxon.z.IntSingletonIterator;

import java.io.IOException;
import java.io.Writer;
import java.util.function.IntPredicate;

/**
 * A UnicodeString containing a single codepoint
 */

public class UnicodeChar extends UnicodeString {

    int codepoint;

    public UnicodeChar(int codepoint) {
        this.codepoint = codepoint;
    }

    /**
     * Get an iterator over the code points present in the string. Note that this method
     * is always available, whether or not the {@code UniString} has been prepared for codePoint access.
     *
     * @return an iterator that delivers the individual code points
     */
    @Override
    public IntIterator codePoints() {
        return new IntSingletonIterator(codepoint);
    }

    /**
     * Get the length of the string
     *
     * @return the number of code points in the string
     * @throws UnsupportedOperationException if the {@code UniString} has not been prepared
     *                                       for codePoint access
     */
    @Override
    public long length() {
        return 1;
    }

    /**
     * Get the number of bits needed to hold all the characters in this string
     *
     * @return 7 for ascii characters, 8 for latin-1, 16 for BMP, 24 for general Unicode.
     */
    @Override
    public int getWidth() {
        if (codepoint < 128) {
            return 7;
        } else if (codepoint < 256) {
            return 8;
        } else if (codepoint < 65536) {
            return 16;
        } else {
            return 24;
        }
    }

    /**
     * Get the position of the first occurrence of the specified codepoint,
     * starting the search at a given position in the string
     *
     * @param codePoint the sought codePoint
     * @param from      the position from which the search should start (0-based)
     * @return the position (0-based) of the first occurrence found, or -1 if not found
     * @throws UnsupportedOperationException if the {@code UniString} has not been prepared
     *                                       for codePoint access
     */
    @Override
    public long indexOf(int codePoint, long from) {
        return (this.codepoint == codePoint && from == 0) ? 0 : -1;
    }

    /**
     * Get the code point at a given position in the string
     *
     * @param index the given position (0-based)
     * @throws UnsupportedOperationException if the {@code UniString} has not been prepared
     *                                       for codePoint access
     */
    @Override
    public int codePointAt(long index) {
        return index == 0 ? codepoint : -1;
    }

    /**
     * Get a substring of this codepoint sequence, with a given start and end position
     *
     * @param start the start position (0-based): that is, the position of the first
     *              code point to be included
     * @param end   the end position (0-based): specifically, the position of the first
     *              code point not to be included
     * @throws UnsupportedOperationException if the {@code UniString} has not been prepared
     *                                       for codePoint access
     */
    @Override
    public UnicodeString substring(long start, long end) {
        return start == 0 && end == 1 ? this : EmptyUniString.getInstance();
    }

    /**
     * Append the content, or part of the content, to a {@link LargeTextBuffer}
     *
     * @param consumer the recipient of the content
     * @param start    the start offset (in codepoints) to be sent to the consumer
     * @param length   the number of codepoints to be sent
     * @throws IndexOutOfBoundsException if the start or end are out of range
     */
    @Override
    public void supplyContent(LargeTextBuffer consumer, long start, long length) {
        if (start == 0 && length == 1) {
            consumer.append(codepoint);
        }
    }

    /**
     * Write the content of this string to a Writer
     *
     * @param writer the writer to which the string should be written
     */
    @Override
    public void write(Writer writer) throws IOException {
        writer.write(toString());
    }

    /**
     * Write part of this string to a Writer
     *
     * @param writer the writer to which the string should be written
     * @param start  the offset of the first codepoint to be written
     * @param len    the number of codepoints to be written
     */
    @Override
    public void write(Writer writer, long start, long len) throws IOException {
        if (start == 0 && len == 1) {
            writer.write(toString());
        }
    }

    /**
     * Write a substring of this string to a supplied Writer, ending the substring
     * where a specified ccondition is encountered
     *
     * @param writer the destination to which the substring is written
     * @param start  the offset of the first codepoint to be considered
     * @param limit  the offset of the first codepoint not to be considered.
     * @param stop   Predicate to match codepoints. The substring that is written includes
     *               all characters up to, but excluding, the first character to match
     *               this predicate
     * @return the offset of the first character that that matches the predicate, or -1 if not found
     */
    @Override
    public long writeUntil(Writer writer, long start, long limit, IntPredicate stop) throws IOException {
        if (start == 0 && limit == 1) {
            if (stop.test(codepoint)) {
                return 0;
            } else {
                writer.write(toString());
                return -1;
            }
        }
        return -1;
    }


    @Override
    public String toString() {
        if (codepoint < 65536) {
            return "" + (char)codepoint;
        } else {
            StringBuilder buff = new StringBuilder(2);
            buff.appendCodePoint(codepoint);
            return buff.toString();
        }
    }
}

