package net.sf.saxon.str;

import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Base64BinaryValue;
import net.sf.saxon.z.IntIterator;

import java.util.function.IntPredicate;

/**
 * A UnicodeString is a sequence of Unicode codepoints that supports codepoint addressing.
 * <p>The interface is future-proofed to support code points in the range 0 to 2^31, and string lengths of
 * up to 2^63 characters. Implementations may (and do) impose lower limits.</p>
 */
public abstract class UnicodeString extends CodePoints implements AtomicMatchKey {

    /**
     * Ensure that the implementation is capable of counting codepoints in the string. For an
     * implementation of {@code UnicodeString}, this is normally a null operation.
     * @return this {@code UnicodeString}
     */

    @Override
    public UnicodeString tidy() {
        return this;
    }

    /**
     * Get the length of the string
     * @return the number of code points in the string
     */

    public abstract long length();

    /**
     * Get the length of the string, provided it is less than 2^31 characters
     * @return the length of the string if it fits within a Java {@code int}
     * @throws UnsupportedOperationException if the string is longer than 2^31 characters
     */

    public int length32() {
        return requireInt(length());
    }

    /**
     * Get the estimated length of the string, suitable for space allocation.
     *
     * @return for a {@code UnicodeString}, the actual length of the string in codepoints
     */

    @Override
    public final long estimatedLength() {
        return length();
    }

    /**
     * Ask whether the string is empty
     * @return true if the length of the string is zero
     */

    @Override
    public boolean isEmpty() {
        return length() == 0;
    }

    /**
     * Get the number of bits needed to hold all the characters in this string
     * @return 7 for ascii characters, 8 for latin-1, 16 for BMP, 24 for general Unicode.
     */

    public abstract int getWidth();

    /**
     * Get the position of the first occurrence of the specified codepoint,
     * starting the search at the beginning
     *
     * @param codePoint the sought codePoint
     * @return the position (0-based) of the first occurrence found, or -1 if not found,
     * counting codePoints rather than UTF16 chars.
     * @throws UnsupportedOperationException if the {@code UniString} has not been prepared
     * for codePoint access
     */

    public long indexOf(int codePoint) {
        return indexOf(codePoint, 0);
    }

    /**
     * Get the position of the first occurrence of the specified codepoint,
     * starting the search at a given position in the string
     * @param codePoint the sought codePoint
     * @param from the position from which the search should start (0-based)
     * @return the position (0-based) of the first occurrence found, or -1 if not found
     * @throws UnsupportedOperationException if the {@code UniString} has not been prepared
     * for codePoint access
     */

    public abstract long indexOf(int codePoint, long from);

    /**
     * Get the position of the first occurrence of a codepoint that matches a supplied predicate,
     * starting the search at a given position in the string
     *
     * @param predicate condition that the codepoint must satisfy
     * @param from      the position from which the search should start (0-based)
     * @return the position (0-based) of the first codepoint to match the predicate, or -1 if not found
     * @throws UnsupportedOperationException if the {@code UniString} has not been prepared
     *                                       for codePoint access
     */

    public long indexWhere(IntPredicate predicate, long from) {
        IntIterator iter = codePoints();
        long i = 0;
        while (iter.hasNext()) {
            if (i >= from && predicate.test(iter.next())) {
                return i;
            }
            i++;
        }
        return -1;
    }

    /**
     * Get the first position, at or beyond start, where another string appears as a substring
     * of this string, comparing codepoints.
     *
     * @param other the other (sought) string
     * @param from  the position (0-based) where searching is to start (counting in codepoints)
     * @return the first position where the substring is found, or -1 if it is not found
     * @throws UnsupportedOperationException if the {@code UniString} has not been prepared
     * for codePoint access
     */

    public long indexOf(UnicodeString other, long from) {
        if (from < 0 || from >= length()) {
            return -1;
        }
        if (other.isEmpty()) {
            return from;
        }
        int initial = other.codePointAt(0);
        long len = other.length();
        long lastPossible = length() - len;
        while (from <= lastPossible) {
            long i = indexOf(initial, from);
            if (i < 0) {
                return -1;
            }
            if (hasSubstring(other, i)) {
                return i;
            }
            from = i + 1;
        }
        return -1;
    }

    /**
     * Ask whether this string has another string as its content starting at a given offset
     *
     * @param other  the other string
     * @param offset the starting position in this string (counting in codepoints)
     * @return true if the other string appears as a substring of this string starting at the
     * given position.
     * @throws UnsupportedOperationException if the {@code UniString} has not been prepared
     * for codePoint access
     */

    public boolean hasSubstring(UnicodeString other, long offset) {
        if (offset < 0 || offset > length()) {
            throw new IndexOutOfBoundsException();
        }
        long len = other.length();
        if (len + offset > length()) {
            return false;
        }
        for (long k = 0; k < len; k++) {
            if (codePointAt(offset + k) != other.codePointAt(k)) {
                return false;
            }
        }
        return true;
    }

    /**
     * Get the code point at a given position in the string
     * @param index the given position (0-based)
     * @throws UnsupportedOperationException if the {@code UniString} has not been prepared
     * for codePoint access
     */

    public abstract int codePointAt(long index);

    /**
     * Get a substring of this codepoint sequence, with a given start position,
     * finishing at the end of the string
     *
     * @param start the start position (0-based): that is, the position of the first
     *              code point to be included
     * @throws UnsupportedOperationException if the {@code UniString} has not been prepared
     * for codePoint access
     */

    public UnicodeString substring(long start) {
        return substring(start, length());
    }

    /**
     * Get a substring of this codepoint sequence, with a given start and end position
     * @param start the start position (0-based): that is, the position of the first
     *              code point to be included
     * @param end the end position (0-based): specifically, the position of the first
     *            code point not to be included
     * @throws UnsupportedOperationException if the {@code UniString} has not been prepared
     * for codePoint access
     */

    public abstract UnicodeString substring(long start, long end);

    /**
     * Concatenate another string
     * @param other the string to be appended to this one
     * @return the result of the concatenation (neither input string is altered)
     */

    public UnicodeString concat(UnicodeString other) {
        return Rope.makeRope(this, other);
    }

    @Override
    public boolean equals(Object obj) {
        if (obj instanceof UnicodeString) {
            UnicodeString other = (UnicodeString)obj;
            if (this.length() != other.length() || this.hashCode() != other.hashCode()) {
                return false;
            }
        }
        return super.equals(obj);
    }

    /**
     * Get the codepoints of the string as a byte array, allocating three
     * bytes per character. (Max unicode codepoint = x10FFFF)
     *
     * @return a byte array that can act as a collation key
     */

    private byte[] getCodepointCollationKey() {
        UnicodeString prep = tidy();
        int len = requireInt(prep.length());
        byte[] result = new byte[len * 3];
        IntIterator iter = prep.codePoints();
        int j=0;
        while (iter.hasNext()) {
            int c = iter.next();
            result[j++] = (byte) (c >> 16);
            result[j++] = (byte) (c >> 8);
            result[j++] = (byte) c;
        }
        return result;
    }

    /**
     * Get an atomic value that encapsulates this match key. Needed to support the collation-key() function.
     *
     * @return an atomic value that encapsulates this match key
     */
    @Override
    public AtomicValue asAtomic() {
        return new Base64BinaryValue(getCodepointCollationKey());
    }

    /**
     * Append the content, or part of the content, to a {@link LargeTextBuffer}
     * @param consumer the recipient of the content
     * @param start the start offset (in codepoints) to be sent to the consumer
     * @param length the number of codepoints to be sent
     * @throws IndexOutOfBoundsException if the start or end are out of range
     */

    public abstract void supplyContent(LargeTextBuffer consumer, long start, long length);

    /**
     * Utility method for use where strings longer than 2^31 characters cannot yet be handled.
     * @param value the actual value of a character position within a string, or the length of
     *              a string
     * @return the value as an integer if it is within range
     * @throws UnsupportedOperationException if the supplied value exceeds {@link Integer#MAX_VALUE}
     */

    public static int requireInt(long value) {
        if (value > Integer.MAX_VALUE) {
            throw new UnsupportedOperationException("String offset exceeds 2^31 characters");
        }
        return (int) value;
    }
}

