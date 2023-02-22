////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.str;

import net.sf.saxon.z.ConcatenatingIntIterator;
import net.sf.saxon.z.IntIterator;

import java.util.ArrayList;
import java.util.List;
import java.util.function.IntPredicate;

/**
 * Implementation of a Unicode string as the concatenation of two Unicode strings,
 * forming a binary tree.
 *
 * <p>The term <b>Rope</b> comes from the paper: <i>Ropes: an Alternative to Strings</i> by
 * Hans-J. Boehm, Russ Atkinson and Michael Plass, in SOFTWARE—PRACTICE AND EXPERIENCE, VOL. 25(12),
 * 1315–1330 (DECEMBER 1995). The structure used here is similar to the structure described
 * in that paper, but the mechanisms for tree balancing differ somewhat.</p>
 */
public class Rope extends UnicodeString {

    private final UnicodeString left;
    private final UnicodeString right;
    private final long _length; // length of the string in characters
    private final int _depth;

    public static UnicodeString makeRope(UnicodeString left, UnicodeString right) {
        if (left instanceof Rope && ((Rope)left).right.length() + right.length() < 1024) {
            return new Rope(((Rope) left).left, ((Rope) left).right.concat(right)).rebalance();
        }
        return new Rope(left, right).rebalance();
    }

    private Rope(UnicodeString left, UnicodeString right) {
        this.left = left;
        this.right = right;
        this._length = left.length() + right.length();
        this._depth = Math.max(depth(left), depth(right)) + 1;
    }

    private static int depth(UnicodeString string) {
        return string instanceof Rope ? ((Rope)string)._depth : 0;
    }

//    private int depth() {
//        int d1 = left instanceof Rope ? ((Rope)left).depth() : 0;
//        int d2 = right instanceof Rope ? ((Rope)right).depth() : 0;
//        return Math.max(d1, d2) + 1;
//    }



    @Override
    public int codePointAt(long index) {
        if (index < 0) {
            throw outOfBounds(index, _length);
        } else if (index < left.length()) {
            return left.codePointAt(index);
        } else if (index < _length) {
            return right.codePointAt(index - left.length());
        } else {
            throw outOfBounds(index, _length);
        }
    }

    @Override
    public long length() {
        return _length;
    }

    @Override
    public boolean isEmpty() {
        return _length == 0;
    }

    /**
     * Get the number of bits needed to hold all the characters in this string
     * @return 7 for ascii characters, 8 for latin-1, 16 for BMP, 24 for general Unicode.
     */
    @Override
    public int getWidth() {
        return Math.max(left.getWidth(), right.getWidth());
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
        long index;
        if (from < left.length()) {
            index = left.indexOf(codePoint, from);
            if (index >= 0) {
                return index;
            }
        }
        index = right.indexOf(codePoint, Math.min(from - left.length(), 0));
        if (index >= 0) {
            return index + left.length();
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
     *                                       for codePoint access
     */
    @Override
    public long indexOf(UnicodeString other, long from) {
        if (other.isEmpty()) {
            return 0;
        }
        if (other.length() > _length - from) {
            return -1;
        }
        if (from + other.length() < left.length()) {
            // Look in the left substring
            long lefty = left.indexOf(other, from);
            if (lefty >= 0) {
                return lefty;
            }
        }
        if (from < left.length()) {
            // Look for a "spanning" substring
            long probe = left.length() - other.length() + 1;
            int initial = other.codePointAt(0);
            while (probe < left.length()) {
                if (left.codePointAt(probe) == initial && substring(probe, probe + other.length()).equals(other)) {
                    return probe;
                }
                probe++;
            }
        }
        // Look in the right substring
        long loc = right.indexOf(other, Math.max(from - left.length(), 0));
        return loc < 0 ? -1 : loc + left.length();
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
        if (start < 0 || start > _length) {
            throw outOfBounds(start, _length);
        } else if (end < start || end > _length) {
            throw outOfBounds(end, _length);
        }
        if (start < left.length()) {
            if (end <= left.length()) {
                return left.substring(start, end);
            } else {
                // substring spans the two branches
                UnicodeString part1 = left.substring(start);
                UnicodeString part2 = right.substring(0, end - left.length());
                return part1.concat(part2);
            }
        } else {
            return right.substring(start - left.length(), end - left.length());
        }
    }

    /**
     * Append the content, or part of the content, to a {@link LargeTextBuffer}
     *
     * @param consumer the recipient of the content
     * @param start    the start offset (in codepoints) to be sent to the consumer
     * @param len   the number of codepoints to be sent
     * @throws IndexOutOfBoundsException if the start or end are out of range
     */
    @Override
    public void supplyContent(LargeTextBuffer consumer, long start, long len) {
        long end = start + len;
        if (start < 0 || start >= _length) {
            throw outOfBounds(start, _length);
        } else if (end < start || end > _length) {
            throw outOfBounds(end, _length);
        }
        if (start < left.length()) {
            if (end <= left.length()) {
                left.supplyContent(consumer, start, len);
            } else {
                // substring spans the two branches
                //UnicodeString part1 = left.substring(start);
                left.supplyContent(consumer, start, left.length() - start);
                right.supplyContent(consumer, 0, end - left.length());
            }
        } else {
            right.supplyContent(consumer, start - left.length(), len);
        }
    }

    /**
     * Get the position of the first occurrence of the specified codepoint,
     * starting the search at a given position in the string
     *
     * @param predicate condition that the codepoint must satisfy
     * @param from      the position from which the search should start (0-based)
     * @return the position (0-based) of the first codepoint to match the predicate, or -1 if not found
     * @throws UnsupportedOperationException if the {@code UniString} has not been prepared
     *                                       for codePoint access
     */
    @Override
    public long indexWhere(IntPredicate predicate, long from) {
        if (from < left.length()) {
            long found = left.indexWhere(predicate, from);
            if (found >= 0) {
                return found;
            }
        }
        long r = right.indexWhere(predicate, Math.max(0, from - left.length()));
        return r < 0 ? r : r + left.length();
    }

    /**
     * Get an equivalent UniString that uses the most economical representation available
     *
     * @return an equivalent UniString
     */
    @Override
    public UnicodeString economize() {
        if (_length < 256 || (_length < 8192 && left.getWidth() == right.getWidth())) {
            UnicodeBuilder builder = new UnicodeBuilder();
            writeToBuilder(this, builder);
            return builder.toUnicodeString();
        } else {
            UnicodeString l2 = left.economize();
            UnicodeString r2 = right.economize();
            if (l2 == left && r2 == right) {
                return this;
            } else {
                return makeRope(l2, r2);
            }
        }
    }

    private static void writeToBuilder(UnicodeString string, UnicodeBuilder builder) {
        if (string instanceof Rope) {
            writeToBuilder(((Rope) string).left, builder);
            writeToBuilder(((Rope) string).right, builder);
        } else {
            builder.append(string);
        }
    }

    @Override
    public String toString() {
        StringBuilder builder = new StringBuilder();
        writeToStringBuilder(this, builder);
        return builder.toString();
    }

    private static void writeToStringBuilder(UnicodeString string, StringBuilder builder) {
        if (string instanceof Rope) {
            writeToStringBuilder(((Rope) string).left, builder);
            writeToStringBuilder(((Rope) string).right, builder);
        } else {
            builder.append(string);
        }
    }

    @Override
    public IntIterator codePoints() {
        return new ConcatenatingIntIterator(left.codePoints(), right::codePoints);
    }

    private static final int THRESHOLD = 10;

    protected UnicodeString rebalance() {
        if (left.isEmpty()) {
            return right;
        }
        if (right.isEmpty()) {
            return left;
        }
        if (_depth > 64) {
            List<UnicodeString> leaves = new ArrayList<>();
            gatherLeaves(this, leaves);
            //System.err.println("Before balance depth=" + depth + " length=" + length + " segments=" + leaves.size());

            // Average leaf length < 50, try combining adjacent leaves
            List<UnicodeString> merged = new ArrayList<>();
            UnicodeString prev = null;
            int targetLength = 1024;
            int minLength = 16;
            if (leaves.size() > 10000) {
                // in this case try a bit harder to merge adjacent leaves, in order to reduce the tree depth
                targetLength = 4096;
                minLength = 64;
            }
            for (UnicodeString leaf : leaves) {
                if (prev == null) {
                    prev = leaf;
                } else {
                    // Some pragmatic decisions here. Merge two adjacent strings if they're the same width
                    // and the total length is less than 1024 (because this will generally be an arraycopy),
                    // or if the narrower string is very short, so widening it doesn't cost too much
                    if (prev.length() + leaf.length() < targetLength && (
                            prev.getWidth() == leaf.getWidth() ||
                                    (prev.getWidth() < leaf.getWidth() && prev.length() < minLength) ||
                                    (prev.getWidth() > leaf.getWidth() && leaf.length() < minLength)
                    )) {
                        UnicodeBuilder ub = new UnicodeBuilder();
                        ub.append(prev);
                        ub.append(leaf);
                        prev = ub.toUnicodeString();
                    } else {
                        merged.add(prev);
                        prev = leaf;
                    }
                }
            }
            merged.add(prev);
            leaves = merged;

            UnicodeString balanced = balance(leaves, 0, leaves.size());
            //System.err.println("After balance " + (balanced instanceof Rope ? (" depth = " + ((Rope)balanced).depth) : balanced.getClass().toString()) + " segments=" + leaves.size());
            return balanced;
        }
//        //System.err.println("Balance l=" + left.size() + " r=" + right.size());
//        if (length() > THRESHOLD) {
//            if (left instanceof Rope) {
//                long rlen = ((Rope) left).right.length() + right.length();
//                if (rlen <= 1024) {
//                    return new Rope(((Rope) left).left, ((Rope) left).right.concat(right));
//                } else if (rlen <= ((Rope) left).left.length() * 2) {
//                    return new Rope(((Rope) left).left, new Rope(((Rope) left).right, right));
//                }
//            }
//            if (right instanceof Rope
//                    && ((Rope) right).left.length() + left.length() < ((Rope) right).right.length() * 2) {
//                return new Rope(new Rope(left, ((Rope) right).left), ((Rope) right).right);
//            }
//        }
        return this;
    }

    private static void gatherLeaves(Rope node, List<UnicodeString> leaves) {
        if (node.left instanceof Rope) {
            gatherLeaves((Rope)node.left, leaves);
        } else {
            leaves.add(node.left);
        }
        if (node.right instanceof Rope) {
            gatherLeaves((Rope) node.right, leaves);
        } else {
            leaves.add(node.right);
        }
    }

    private static UnicodeString balance(List<UnicodeString> leaves, int start, int end) {
        int len = end - start;
        if (len == 1) {
            return leaves.get(start);
        } else {
            int mid = start + len/2;
            UnicodeString left = balance(leaves, start, mid);
            UnicodeString right = balance(leaves, mid, end);
            return new Rope(left, right);
        }
    }

    /**
     * Convenience method to throw an IndexOutOfBounds exception when needed
     *
     * @param requested the index value that was requested by the caller
     * @param actual    the actual size of the list
     * @return an {@code IndexOutOfBoundsException} with suitable message text
     */

    protected StringIndexOutOfBoundsException outOfBounds(long requested, long actual) {
        return new StringIndexOutOfBoundsException("Requested " + requested + ", actual size " + actual);
    }
}

