////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.str;

import net.sf.saxon.serialize.UTF8Writer;
import net.sf.saxon.z.ConcatenatingIntIterator;
import net.sf.saxon.z.IntIterator;
import net.sf.saxon.z.IntRepeatIterator;

import java.util.Arrays;

/**
 * This class provides a compressed representation of a string used to represent indentation: specifically,
 * an integer number of newlines followed by an integer number of spaces.  It's a little cheaper than
 * the {@link CompressedWhitespace} class, and is specifically used for constructing indentation and passing
 * it to a {@link UTF8Writer}, which recognizes it specially. Unlike {@link CompressedWhitespace}, it can't
 * handle arbitrary whitespace strings, only those consisting of newlines followed by spaces.
 */

public class IndentWhitespace extends WhitespaceString {

    private final Twine8 lineEnding;
    private final int charCount;
    private final char indentChar;

    private IndentWhitespace(Twine8 lineEnding, int charCount, char indentChar) {
        this.lineEnding = lineEnding;
        this.charCount = charCount;
        this.indentChar = indentChar;
    }

    /**
     * Create an IndentWhitespace object
     * @param lineEnding the end-of-line sequence
     * @param spaces the number of spaces following the newlines
     * @return the constructed (or potentially cached) IndentWhitespace object
     */

    public static IndentWhitespace of(Twine8 lineEnding, int spaces, char indentChar) {
        // Uses a factory method to permit pooling, not currently implemented
        return new IndentWhitespace(lineEnding, spaces, indentChar);
    }

    /**
     * Uncompress the whitespace to a (normal) UnicodeString
     * @return the uncompressed value
     */

    @Override
    public UnicodeString uncompress() {
        byte[] nl = lineEnding.getByteArray();
        byte[] bytes = Arrays.copyOf(nl, nl.length + charCount);
        Arrays.fill(bytes, nl.length, bytes.length, (byte)indentChar);
        return new Twine8(bytes);
    }

    @Override
    public long length() {
        return lineEnding.length32() + charCount;
    }

    @Override
    public int length32() {
        return lineEnding.length32() + charCount;
    }

    /**
     * Get the number of spaces or tabs following the newlines
     * @return the number of space or tab characters
     */

    public int getCharCount() {
        return charCount;
    }

    /**
     * Returns the codepoint value at the specified index.  An index ranges from zero
     * to <code>length() - 1</code>.  The first codepoint value of the sequence is at
     * index zero, the next at index one, and so on, as for array
     * indexing.
     *
     * @param index the index of the codepoint value to be returned
     * @return the specified codepoint value
     * @throws IndexOutOfBoundsException if the <code>index</code> argument is negative or not less than
     *                                   <code>length()</code>
     */
    @Override
    public int codePointAt(long index) {
        if (index >= 0 && index < lineEnding.length32()) {
            return 0x0A;
        } else if (index <= lineEnding.length32() + charCount) {
            return indentChar;
        } else {
            throw new IndexOutOfBoundsException();
        }
    }


    @Override
    public IntIterator codePoints() {
        return new ConcatenatingIntIterator(
                lineEnding.codePoints(),
                () -> new IntRepeatIterator(indentChar, charCount));
    }

    /**
     * Returns a string representation of the object.
     */
    public String toString() {
        return uncompress().toString();
    }

    /**
     * Write the value to a UnicodeWriter
     *
     * @param writer the writer to write to
     * @throws java.io.IOException if an error occurs downstream
     */

    @Override
    public void write(/*@NotNull*/ UnicodeWriter writer) throws java.io.IOException {
        writer.writeAscii(lineEnding.getByteArray());
        if (charCount > 0) {
            writer.writeRepeatedAscii((byte) indentChar, charCount);
        }
    }

    /**
     * Write the value to a Writer with escaping of special characters
     *
     * @param specialChars identifies which characters are considered special
     * @param writer       the writer to write to
     * @throws java.io.IOException if an error occurs downstream
     */

    @Override
    public void writeEscape(boolean[] specialChars, UnicodeWriter writer) throws java.io.IOException {
        writer.writeAscii(lineEnding.getByteArray());
        writer.writeRepeatedAscii((byte) indentChar, charCount);
    }
}

