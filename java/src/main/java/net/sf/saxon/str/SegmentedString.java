/// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.str;

import net.sf.saxon.expr.sort.EmptyIntIterator;
import net.sf.saxon.z.IntIterator;

import java.io.IOException;
import java.util.*;
import java.util.function.IntPredicate;

/**
 * A SegmentedString is an implementation of UnicodeString that comprises an array
 * of segments representing substrings of the total string; all segments except the
 * last have the same length in codepoints, so indexing is straightforward.
 * Unlike the ZenoString, this structure is not designed for efficient modification.
 */

public class SegmentedString extends UnicodeString {

    private UnicodeString[] segments;

    private final static long LOCAL_BITS = 16;  // segments of fixed size 2^16 = 65536 codepoints
    public final static int SEGMENT_LENGTH = 1<<LOCAL_BITS;
    private final static long INNER_MASK = SEGMENT_LENGTH - 1;

    /**
     * Private constructor creating an empty SegmentedString (containing an empty list of segments)
     */

    public SegmentedString() {
        segments = new UnicodeString[0];
    }

    /**
     * Constructor creating a SegmentedString from a codepoint iterator
     */

    public SegmentedString(IntIterator codepoints) {
        List<UnicodeString> segmentList = new ArrayList<>();
        UnicodeBuilder currentSegment = new UnicodeBuilder();
        while (codepoints.hasNext()) {
            currentSegment.append(codepoints.next());
            if (currentSegment.length() == SEGMENT_LENGTH) {
                segmentList.add(currentSegment.toUnicodeString().economize());
                currentSegment = new UnicodeBuilder();
            }
        }
        if (!currentSegment.isEmpty()) {
            segmentList.add(currentSegment.toUnicodeString().economize());
        }
        this.segments = segmentList.toArray(new UnicodeString[0]);
    }

    /**
     * An empty SegmentedString
     */

    public static final SegmentedString EMPTY = new SegmentedString();

    /**
     * Internal constructor
     * @param segmentArray the array of segments
     */
    protected SegmentedString(UnicodeString[] segmentArray) {
        this.segments = segmentArray;
    }

    /**
     * Construct a SegmentedString from a supplied UnicodeString
     * @param content the supplied UnicodeString
     * @return the resulting ZenoString
     */

    public static SegmentedString of(UnicodeString content) {
        if (content instanceof SegmentedString) {
            return (SegmentedString) content;
        } else if (content.isEmpty()) {
            return new SegmentedString();
        } else {
            return new SegmentedString(content.codePoints());
        }
    }

    /**
     * Get an iterator over the code points present in the string.
     * @return an iterator that delivers the individual code points
     */
    @Override
    public IntIterator codePoints() {
        if (isEmpty()) {
            return EmptyIntIterator.getInstance();
        }
        return new IntIterator() {
            final long len = length();
            int offset = 0;

            @Override
            public boolean hasNext() {
                return offset < len;
            }

            @Override
            public int next() {
                return codePointAt(offset++);
            }
        };
    }

    /**
     * Get the length of the string
     *
     * @return the number of code points in the string
     */
    @Override
    public long length() {
        long temp = ((long) segments.length - 1) * SEGMENT_LENGTH + segments[segments.length - 1].length();
        return ((long) segments.length - 1) * SEGMENT_LENGTH + segments[segments.length - 1].length();
    }

    /**
     * Ask whether the string is empty
     *
     * @return true if the length of the string is zero
     */
    @Override
    public boolean isEmpty() {
        return segments.length == 0;
    }

    /**
     * Get the number of bits needed to hold all the characters in this string
     *
     * @return 7 for ascii characters, 8 for latin-1, 16 for BMP, 24 for general Unicode.
     */
    @Override
    public int getWidth() {
        int maxWidth = 7;
        for (UnicodeString entry : segments) {
            int width = entry.getWidth();
            if (width == 24) {
                return 24;
            } else {
                maxWidth = Math.max(maxWidth, width);
            }
        }
        return maxWidth;
    }

    /**
     * Get the position of the first occurrence of the specified codepoint,
     * starting the search at a given position in the string
     *
     * @param codePoint the sought codePoint
     * @param from      the position from which the search should start (0-based). A negative value is
     *                  treated as zero. A position beyond the end of the string results in a return
     *                  value of -1 (meaning not found).
     * @return the position (0-based) of the first occurrence found, or -1 if not found
     * @throws IndexOutOfBoundsException if the <code>from</code> value is out of range
     */
    @Override
    public long indexOf(int codePoint, long from) {
        from = Math.max(from, 0);
        if (from >= length()) {
            return -1L;
        }
        int first = 0;
        if (from != 0L) {
            first = (int) (from >> LOCAL_BITS);
            long pos = segments[first].indexOf(codePoint, from & INNER_MASK);
            if (pos >= 0) {
                return ((long) first * SEGMENT_LENGTH) + pos;
            }
            first++;
        }
        for (int i = first; i < segments.length; i++) {
            long pos = segments[i].indexOf(codePoint, 0);
            if (pos >= 0) {
                return ((long) i * SEGMENT_LENGTH) + pos;
            }
        }
        return -1;
    }

    @Override
    public long indexWhere(IntPredicate predicate, long from) {
        from = Math.max(from, 0);
        if (from >= length()) {
            return -1L;
        }
        int first = 0;
        if (from != 0L) {
            first = (int) (from >> LOCAL_BITS);
            long pos = segments[first].indexWhere(predicate, from & INNER_MASK);
            if (pos >= 0) {
                return ((long) first * SEGMENT_LENGTH) + pos;
            }
            first++;
        }
        for (int i = first; i < segments.length; i++) {
            long pos = segments[i].indexWhere(predicate, 0);
            if (pos >= 0) {
                return ((long) i * SEGMENT_LENGTH) + pos;
            }
        }
        return -1;
    }

    /**
     * Get the code point at a given position in the string
     * @param index the given position (0-based)
     * @throws IndexOutOfBoundsException if the index is out of range
     */
    @Override
    public int codePointAt(long index) {
        try {
            UnicodeString segment = segments[(int) (index >> LOCAL_BITS)];
            return segment.codePointAt(index & INNER_MASK);
        } catch (Exception e) {
            throw new RuntimeException(e);
        }
    }

    /**
     * Get a substring of this codepoint sequence, with a given start and end position
     *
     * @param start the start position (0-based): that is, the position of the first
     *              code point to be included
     * @param end   the end position (0-based): specifically, the position of the first
     *              code point not to be included
     */
    @Override
    public UnicodeString substring(long start, long end) {
        int firstSegment = (int) (start >> LOCAL_BITS);
        int lastSegment = (int) (end >> LOCAL_BITS);
        if (firstSegment == lastSegment) {
            // start and end in the same segment
            return segments[firstSegment].substring(start & INNER_MASK, end & INNER_MASK);
        }
        ZenoString zeno = ZenoString.EMPTY;
        zeno = zeno.concat(segments[firstSegment].substring(start & INNER_MASK, SEGMENT_LENGTH));
        for (int i = firstSegment + 1; i < lastSegment; i++) {
            zeno = zeno.concat(segments[i]);
        }
        zeno = zeno.concat(segments[lastSegment].substring(0, end & INNER_MASK));
        return zeno;
    }

    /**
     * Concatenate another string
     *
     * @param other the string to be appended to this one
     * @return the result of the concatenation (neither input string is altered)
     */
    @Override
    public UnicodeString concat(UnicodeString other) {
        if (isEmpty()) {
            return other;
        } else if (other.isEmpty()) {
            return this;
        }
        List<UnicodeString> segmentList =
                new ArrayList<>(Arrays.asList(segments).subList(0, segments.length - 1));

        UnicodeBuilder currentSegment = new UnicodeBuilder();
        currentSegment.append(segments[segments.length - 1]);
        if (currentSegment.length() == SEGMENT_LENGTH) {
            segmentList.add(currentSegment.toUnicodeString().economize());
            currentSegment = new UnicodeBuilder();
        }

        IntIterator codepoints = other.codePoints();
        while (codepoints.hasNext()) {
            currentSegment.append(codepoints.next());
            if (currentSegment.length() == SEGMENT_LENGTH) {
                segmentList.add(currentSegment.toUnicodeString().economize());
                currentSegment = new UnicodeBuilder();
            }
        }
        if (!currentSegment.isEmpty()) {
            segmentList.add(currentSegment.toUnicodeString().economize());
        }

        UnicodeString[] segmentArray = segmentList.toArray(new UnicodeString[0]);
        return new SegmentedString(segmentArray);
    }

    @Override
    void copy8bit(byte[] target, int offset) {
        for (UnicodeString us : segments) {
            us.copy8bit(target, offset);
            offset += us.length32();
        }
    }

    @Override
    void copy16bit(char[] target, int offset) {
        for (UnicodeString us : segments) {
            us.copy16bit(target, offset);
            offset += us.length32();
        }
    }

    @Override
    void copy24bit(byte[] target, int offset) {
        for (UnicodeString us : segments) {
            us.copy24bit(target, offset);
            offset += (us.length32() * 3);
        }
    }

    @Override
    void copy32bit(int[] target, int offset) {
        for (UnicodeString us : segments) {
            us.copy32bit(target, offset);
            offset += us.length32();
        }
    }

    /**
     * Write each of the segments in turn to a UnicodeWriter
     * @param writer the writer to which the string is to be written
     */

    public void writeSegments(UnicodeWriter writer) throws IOException {
        for (UnicodeString str : segments) {
            writer.write(str);
        }
    }

    public static UnicodeString concatSegments(UnicodeString left, UnicodeString right) {
        if (left.getWidth() <= 8 && right.getWidth() <= 8) {
            byte[] newByteArray = new byte[left.length32() + right.length32()];
            left.copy8bit(newByteArray, 0);
            right.copy8bit(newByteArray, left.length32());
            return new Twine8(newByteArray);
        } else if (left.getWidth() <= 16 && right.getWidth() <= 16) {
            char[] newCharArray = new char[left.length32() + right.length32()];
            left.copy16bit(newCharArray, 0);
            right.copy16bit(newCharArray, left.length32());
            return new Twine16(newCharArray);
        } else {
            byte[] newByteArray = new byte[(left.length32() + right.length32()) * 3];
            left.copy24bit(newByteArray, 0);
            right.copy24bit(newByteArray, left.length32() * 3);
            return new Twine24(newByteArray);
        }
    }


    /**
     * Get an equivalent UnicodeString that uses the most economical representation available
     *
     * @return an equivalent UnicodeString
     */
    @Override
    public UnicodeString economize() {
        int segs = segments.length;
        if (segs == 0) {
            return EmptyUnicodeString.getInstance();
        } else if (segs == 1) {
            return segments[0];
        } else if (segs < 32 && length() < 256 && getWidth() <= 16) {
            // Return a single wrapped Java String, for economy of any subsequent toString() operations.
            return new BMPString(toString());
        } else {
            return this;
        }
    }

    public String toString() {
        StringBuilder sb = new StringBuilder();
        for (UnicodeString str : segments) {
            sb.append(str.toString());
        }
        return sb.toString();
    }

}

