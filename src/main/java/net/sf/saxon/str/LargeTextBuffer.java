////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.str;

import net.sf.saxon.z.IntIterator;

import java.util.Arrays;


/**
 * The segments (other than the last) have a fixed size of 65536 codepoints,
 * which may use one byte per codepoint, two bytes per codepoint, or three bytes per
 * codepoint, depending on the largest codepoint present in the segment.
 * <p>This is more efficient than a buffer backed by a contiguous array of characters
 * in cases where the size is likely to grow very large, and where substring operations
 * are rare. As used within the TinyTree, extraction of the string value of a node
 * requires character copying only in the case where the value crosses segment
 * boundaries.</p>
 */

public final class LargeTextBuffer {

    private final static int BITS = 16;
    private final static int SEGLEN = 1 << BITS;
    private final static int MASK = SEGLEN - 1;


    private Segment[] completeSegments;
    private int segmentsUsed;
    private Segment lastSegment;
    private int lastSegmentLength;


    private interface Segment {
        /**
         * Get the number of bits-per-character in this segment (8, 16, or 24)
         * @return the number of bits per character
         */
        int getWidth();

        /**
         * Return a Segment that contains the existing content of this segment, but
         * stretched if necessary to accommodate a given length and width
         * @param oldLength the number of characters (codepoints) currently used
         *                  in the segment
         * @param newLength the number of characters (codepoints) that the new segment
         *               must have room for
         * @param newWidth the number of bits-per-character that the new segment must
         *              have room for
         * @return either this Segment, or a replacement
         */
        Segment stretch(int oldLength, int newLength, int newWidth);

        /**
         * Get the content of this segment as a {@code UnicodeString}
         */

        UnicodeString asUnicodeString();

        /**
         * Get a substring of this segment
         * @param start the start offset (in codepoints)
         * @param end the end offset (in codepoints)
         * @return the substring
         */
        UnicodeString substring(int start, int end);

    }

    /**
     * A Segment comprising 8-bit characters (codepoints in the range 0-255)
     */
    private static class Segment8 implements Segment {
        public byte[] bytes;

        public Segment8(byte[] bytes) {
            this.bytes = bytes;
        }

        @Override
        public int getWidth() {
            return 8;
        }

        @Override
        public Segment stretch(int oldLength, int newLength, int newWidth) {
            if (newWidth <= 8) {
                if (newLength > bytes.length) {
                    bytes = Arrays.copyOf(bytes, Math.max(newLength, oldLength*2));
                }
                return this;
            } else if (newWidth == 16) {
                char[] array16 = new char[newLength];
                StringTool.copy8to16(bytes, 0, array16, 0, oldLength);
                return new Segment16(array16);
            } else {
                byte[] array24 = new byte[newLength*3];
                StringTool.copy8to24(bytes, 0, array24, 0, oldLength);
                return new Segment24(array24);
            }
        }

        @Override
        public UnicodeString asUnicodeString() {
            return new Twine8(bytes);
        }

        @Override
        public UnicodeString substring(int start, int end) {
            return new Slice8(bytes, start, end);
        }

    }

    /**
     * A Segment comprising 16-bit characters (codepoints in the range 0-65535)
     */

    private static class Segment16 implements Segment {
        public char[] chars;

        /**
         * Construct the segment
         *
         * @param chars an array of chars holding the codepoints, arranged
         *              as two bytes per codepoint; the caller warrants that the
         *              char array contains no surrogates.
         */

        public Segment16(char[] chars) {
            this.chars = chars;
        }

        @Override
        public int getWidth() {
            return 16;
        }

        @Override
        public Segment stretch(int oldLength, int newLength, int newWidth) {
            if (newWidth <= 16) {
                if (newLength > chars.length) {
                    chars = Arrays.copyOf(chars, Math.max(newLength, oldLength * 2));
                }
                return this;
            } else {
                byte[] array24 = new byte[newLength * 3];
                StringTool.copy16to24(chars, 0, array24, 0, oldLength);
                return new Segment24(array24);
            }
        }

        @Override
        public UnicodeString asUnicodeString() {
            return new Twine16(chars);
        }

        @Override
        public UnicodeString substring(int start, int end) {
            return new Slice16(chars, start, end);
        }
    }

    /**
     * A Segment comprising 24-bit characters (any Unicode codepoints)
     */

    private static class Segment24 implements Segment {
        public byte[] bytes;

        /**
         * Construct the segment
         * @param bytes an array of bytes holding the codepoints, arranged
         *              as three bytes per codepoint.
         */

        public Segment24(byte[] bytes) {
            this.bytes = bytes;
        }

        @Override
        public int getWidth() {
            return 24;
        }

        @Override
        public Segment stretch(int oldLength, int newLength, int newWidth) {
            if (newLength * 3 > bytes.length ) {
                bytes = Arrays.copyOf(bytes, Math.max(newLength * 3, oldLength * 6));
            }
            return this;
        }

        @Override
        public UnicodeString substring(int start, int length) {
            return new Slice24(bytes, start, length);
        }

        @Override
        public UnicodeString asUnicodeString() {
            return new Twine24(bytes);
        }

    }

    /**
     * Create an empty LargeTextBuffer with default space allocation
     */

    public LargeTextBuffer() {
        completeSegments = new Segment[2];
        segmentsUsed = 0;
        lastSegment = new Segment8(new byte[4096]);
        lastSegmentLength = 0;
    }

    private void addSegment(Segment segment) {
        if (segmentsUsed + 1 >= completeSegments.length) {
            completeSegments = Arrays.copyOf(completeSegments, completeSegments.length * 2);
        }
        completeSegments[segmentsUsed++] = segment;
    }

    private Segment getSegment(int n) {
        if (n == segmentsUsed) {
            return lastSegment;
        } else {
            return completeSegments[n];
        }
    }

    /**
     * Append an array (or part of an array) of 8-bit characters to the content
     * of the LargeTextBuffer
     * @param bytes the array of characters to be added
     * @param start the start offset of the first character to be copied
     * @param len the number of characters (codepoints, one byte per character) to be copied
     */

    public void append8(byte[] bytes, int start, int len) {
        if (lastSegment == null) {
            byte[] newBytes = new byte[Math.min(len, 4096)];
            lastSegment = new Segment8(newBytes);
            lastSegmentLength = 0;
        }
        if (len > (SEGLEN - lastSegmentLength)) {
            int firstChunkSize = SEGLEN - lastSegmentLength;
            append8(bytes, start, firstChunkSize);
            addSegment(lastSegment);
            lastSegment = null;
            lastSegmentLength = 0;
            append8(bytes, start + firstChunkSize, len - firstChunkSize);
        } else {
            lastSegment = lastSegment.stretch(lastSegmentLength, lastSegmentLength + len, 8);
            switch (lastSegment.getWidth()) {
                case 8: {
                    byte[] existing = ((Segment8) lastSegment).bytes;
                    System.arraycopy(bytes, start, existing, lastSegmentLength, len);
                    break;
                }
                case 16: {
                    char[] existing = ((Segment16) lastSegment).chars;
                    StringTool.copy8to16(bytes, start, existing, lastSegmentLength, len);
                    break;
                }
                case 24: {
                    byte[] existing = ((Segment24) lastSegment).bytes;
                    StringTool.copy8to24(bytes, start, existing, lastSegmentLength, len);
                    break;
                }
                default:
                    throw new IllegalStateException();
            }
            lastSegmentLength += len;
        }
    }

    /**
     * Append an array (or part of an array) of 16-bit characters to the content
     * of the LargeTextBuffer
     *
     * @param chars the array of characters to be added. This must not contain any surrogates
     * @param start the start offset of the first character to be copied
     * @param len   the number of characters (codepoints, two bytes per character) to be copied
     */

    public void append16(char[] chars, int start, int len) {
        if (lastSegment == null) {
            byte[] newBytes = new byte[Math.min(len, 4096)];
            lastSegment = new Segment8(newBytes);
            lastSegmentLength = 0;
        }
        if (len > (SEGLEN - lastSegmentLength)) {
            int firstChunkSize = SEGLEN - lastSegmentLength;
            append16(chars, start, firstChunkSize);
            addSegment(lastSegment);
            lastSegment = null;
            lastSegmentLength = 0;
            append16(chars, start + firstChunkSize, len - firstChunkSize);
        } else {
            lastSegment = lastSegment.stretch(lastSegmentLength, lastSegmentLength + len, 16);
            switch (lastSegment.getWidth()) {
                case 16: {
                    char[] existing = ((Segment16) lastSegment).chars;
                    System.arraycopy(chars, start, existing, lastSegmentLength, len);
                    break;
                }
                case 24: {
                    byte[] existing = ((Segment24) lastSegment).bytes;
                    StringTool.copy16to24(chars, start, existing, lastSegmentLength, len);
                    break;
                }
                default:
                    throw new IllegalStateException();
            }
            lastSegmentLength += len;
        }
    }

    /**
     * Append an array (or part of an array) of 24-bit characters to the content
     * of the LargeTextBuffer
     *
     * @param bytes the array holding the characters to be added, three bytes per character
     * @param start the start offset of the first character to be copied (codepoint offset, not byte offset)
     * @param len   the number of characters (codepoints, three bytes per character) to be copied
     */

    public void append24(byte[] bytes, int start, int len) {
        if (lastSegment == null) {
            byte[] newBytes = new byte[Math.min(len, 4096)];
            lastSegment = new Segment8(newBytes);
            lastSegmentLength = 0;
        }
        if (len > (SEGLEN - lastSegmentLength)) {
            // Allocate a new segment
            int firstChunkSize = SEGLEN - lastSegmentLength;
            append24(bytes, start, firstChunkSize);
            addSegment(lastSegment);
            lastSegment = null;
            lastSegmentLength = 0;
            append24(bytes, start + firstChunkSize, len - firstChunkSize);
        } else {
            lastSegment = lastSegment.stretch(lastSegmentLength, lastSegmentLength + len, 24);
            byte[] existing = ((Segment24) lastSegment).bytes;
            System.arraycopy(bytes, start * 3, existing, lastSegmentLength * 3, len * 3);
            lastSegmentLength += len;
        }
    }

    public void append(IntIterator codePoints) {
        UnicodeBuilder builder = new UnicodeBuilder();
        builder.append(codePoints);
        UnicodeString str = builder.toUnicodeString();
        str.supplyContent(this, 0, str.length());
    }

    public void append(int codePoint) {
        if (codePoint < 256) {
            append8(new byte[]{(byte)(codePoint & 0xff)}, 0, 1);
        } else if (codePoint < 65536) {
            append16(new char[]{(char)(codePoint & 0xffff)}, 0, 1);
        } else {
            Twine24 t = new Twine24(new int[]{codePoint});
            t.supplyContent(this, 0, 1);
        }
    }

    public void append(UnicodeString str, long start, long length) {
        str.supplyContent(this, start, length);
    }

    /**
     * Returns a new character sequence that is a subsequence of this sequence.
     * The subsequence starts with the character at the specified index and
     * ends with the character at index <tt>end - 1</tt>.  The length of the
     * returned sequence is <tt>end - start</tt>, so if <tt>start == end</tt>
     * then an empty sequence is returned.
     *
     * @param start the start index, inclusive (codepoints, not bytes)
     * @param end   the end index, exclusive (codepoints, not bytes)
     * @return the specified subsequence
     * @throws IndexOutOfBoundsException if <tt>start</tt> or <tt>end</tt> are negative,
     *                                   if <tt>end</tt> is greater than <tt>length()</tt>,
     *                                   or if <tt>start</tt> is greater than <tt>end</tt>
     */
    /*@NotNull*/
    public UnicodeString substring(int start, int end) {
        int firstSeg = start >> BITS;
        int lastSeg = (end - 1) >> BITS;
        int lastCP = end & MASK;
        if (lastCP == 0) {
            lastCP = SEGLEN;
        }
        if (firstSeg == lastSeg) {
            // String falls entirely within one segment
            try {
                Segment seg = getSegment(firstSeg);
                return seg.substring(start & MASK, lastCP);
            } catch (ArrayIndexOutOfBoundsException e) {
                e.printStackTrace();
                throw e;
            }
        } else {
            // Concatenate strings from two or more segments
            //System.err.println("Cross-segment s=" + start + " e=" + end);
            UnicodeBuilder ub = new UnicodeBuilder();
            int segNr = firstSeg;
            ub.accept(getSegment(segNr++).substring(start & MASK, SEGLEN));
            while (segNr < lastSeg) {
                ub.accept(getSegment(segNr++).asUnicodeString());
            }
            ub.accept(getSegment(lastSeg).substring(0, lastCP));
            return ub.toUnicodeString();

        }
    }

    public void close() {
        if (lastSegment != null && lastSegmentLength > 0) {
            addSegment(lastSegment);
        }
        lastSegment = null;
    }

    public int length() {
        return segmentsUsed*SEGLEN + (lastSegment == null ? 0 : lastSegmentLength);
    }

    /**
     * Set the length. If this exceeds the current length, this method is a no-op.
     * If this is less than the current length, characters beyond the specified point
     * are deleted.
     *
     * @param newLength the new length
     */

    public void setLength(int newLength) {
        // used to remove a text node if it's found to be a duplicate
        if (newLength < length()) {
            if (newLength <= segmentsUsed * SEGLEN) {
                // drop the current "last segment", and make the last segment in the completed list
                // the new "last segment"
                lastSegment = completeSegments[--segmentsUsed];
            }
            lastSegmentLength = newLength & MASK;
        }
    }

}

