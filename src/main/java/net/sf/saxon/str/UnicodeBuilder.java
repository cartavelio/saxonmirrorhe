package net.sf.saxon.str;

import net.sf.saxon.type.AtomicType;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.z.IntIterator;

import java.io.IOException;
import java.util.function.Consumer;

/**
 * Builder class to construct a UnicodeString by appending text incrementally
 */

public final class UnicodeBuilder implements UniStringConsumer, UnicodeWriter {

    ZenoString content = ZenoString.of(EmptyUnicodeString.getInstance());
    Consumer<UnicodeString> segmentConsumer = null;

    public UnicodeBuilder() {

    }

    public UnicodeBuilder(int allocate) {  // used by bytecode, for compatibility with StringBuilder

    }

    public UnicodeBuilder(Consumer<UnicodeString> consumer) {
        //segments = new ArrayList<>();
        segmentConsumer = consumer;
    }

    /**
     * Append a character (which must not be a surrogate. (Method needed for C#, because implicit
     * conversion of char to int isn't supported)
     * @param ch the character
     * @return the builder, with the new character added
     */
    public UnicodeBuilder append(char ch) {
        content = content.concat(new UnicodeChar(ch));
        return this;
    }

    /**
     * Append a single unicode character to the content
     * @param codePoint the unicode codepoint. The caller is responsible for ensuring that this
     *                  is not a surrogate
     * @return the builder, with the new character added
     */

    public UnicodeBuilder append(int codePoint) {
        content = content.concat(new UnicodeChar(codePoint));
//        if (content.length() % 100 == 0) {
//            System.err.println("Length = " + content.length());
//        }
        return this;
    }

    public UnicodeBuilder append(IntIterator codePoints) {
        while (codePoints.hasNext()) {
            append(codePoints.next());
        }
        return this;
    }

    /**
     * Append a Java string to the content. The caller is responsible for ensuring that this
     * consists entirely of characters in the Latin-1 character set
     *
     * @param str the string to be appended
     * @return the builder, with the new string added
     */

    public UnicodeBuilder appendLatin(String str) {
        return append(new BMPString(str));
    }

    /**
     * Append a Java CharSequence to the content. This may contain arbitrary characters including
     * well formed surrogate pairs
     *
     * @param str the string to be appended
     * @return the builder, with the new string added
     */

    public UnicodeBuilder append(CharSequence str) {
//        int uLength = StringTool.getStringLength(str);
//        if (uLength == str.length()) {
//            // No surrogate pairs
//        }
        return append(StringTool.codePoints(str));
    }

    /**
     * Append a UnicodeString object to the content.
     *
     * @param str the string to be appended. The length is currently restricted to 2^31.
     * @return the builder, with the new string added
     */

    public UnicodeBuilder append(UnicodeString str) {
        content = content.concat(str);
        return this;
    }

    /**
     * Get the number of codepoints currently in the builder
     * @return the size in codepoints
     */
    public long length() {
        return content.length();
    }

    /**
     * Ask whether the content of the builder is empty
     * @return true if the size is zero
     */
    public boolean isEmpty() {
        return content.isEmpty();
    }

    /**
     * Construct a UnicodeString whose value is formed from the contents of this builder
     * @return the constructed {@link UnicodeString}
     */

    public UnicodeString toUnicodeString() {
        return content.economize();
    }

    /**
     * Construct a StringItem whose value is formed from the contents of this builder
     * @param type the required type, for example BuiltInAtomicType.STRING or
     *             BuiltInAtomicType.UNTYPED_ATOMIC. The caller warrants that the value is
     *             a valid instance of this type. No validation or whitespace normalization
     *             is carried out
     * @return the constructed StringItem
     */

    public StringValue toStringItem(AtomicType type) {
        return new StringValue(toUnicodeString(), type);
    }

    /**
     * Return a string containing the character content of this builder
     * @return the character content of this builder as a Java String
     */

    public String toString() {
        return content.toString();
    }



    /**
     * Reset the contents of this builder to be empty
     */

    public void clear() {
        content = ZenoString.of(EmptyUnicodeString.getInstance());
    }

    /**
     * Expand a byte array from 1-byte-per-character to 2-bytes-per-character
     * @param in the input byte array
     * @param start the start offset in bytes
     * @param used the end offset in bytes
     * @param allocate the number of code points to allow for in the output byte array
     * @return the new byte array
     */

    public static byte[] expand1to2(byte[] in, int start, int used, int allocate) {
        byte[] result = new byte[allocate*2];
        for (int i=start, j=0; i<used;) {
            result[j++] = 0;
            result[j++] = in[i++];
        }
        return result;
    }

    public static char[] expandBytesToChars(byte[] in, int start, int end) {
        char[] result = new char[end - start];
        for (int i = start, j = 0; i < end; ) {
            result[j++] = (char)in[i++];
        }
        return result;
    }


    /**
     * Expand a byte array from 1-byte-per-character to 3-bytes-per-character
     *
     * @param in the input byte array
     * @param start the start offset in bytes
     * @param used the end offset in bytes
     * @param allocate the number of code points to allow for in the output byte array
     * @return the new byte array
     */

    public static byte[] expand1to3(byte[] in, int start, int used, int allocate) {
        byte[] result = new byte[allocate*3];
        for (int i = start, j = 0; i < used; ) {
            result[j++] = 0;
            result[j++] = 0;
            result[j++] = in[i++];
        }
        return result;
    }

    /**
     * Expand a byte array from 2-bytes-per-character to 3-bytes-per-character
     *
     * @param in the input byte array
     * @param start the start offset in bytes
     * @param used the end offset in bytes
     * @param allocate the number of code points to allow for in the output byte array
     * @return the new byte array
     */

    public static byte[] expand2to3(byte[] in, int start, int used, int allocate) {
        byte[] result = new byte[allocate*3];
        for (int i = start, j = 0; i < used; ) {
            result[j++] = 0;
            result[j++] = in[i++];
            result[j++] = in[i++];
        }
        return result;
    }

    /**
     * Expand the width of the characters in a byte array
     * @param in the input byte array
     * @param start the start offset in bytes
     * @param end the end offset in bytes
     * @param oldWidth the width of the characters (number of bytes per character) in the input array
     * @param newWidth the width of the characters (number of bytes per character) in the output array. If
     *                 newWidth LE oldWidth then the input array is copied; the width is never reduced
     * @param allocate the number of code points to allow for in the output byte array; if zero (or insufficient)
     *                 the output array will have no spare space for expansion
     * @return the new byte array
     */

    public static byte[] expand(byte[] in, int start, int end, int oldWidth, int newWidth, int allocate) {
        if (allocate <= (end - start) / oldWidth) {
            allocate = (end - start) / oldWidth;
        }
        if (newWidth <= oldWidth) {
            // leave the width unchanged; we don't narrow it
            byte[] out = new byte[allocate * newWidth];
            System.arraycopy(in, start, out, 0, end*oldWidth);
            return out;
        }
        if (oldWidth == 1 && newWidth == 2) {
            return expand1to2(in, start, end, allocate);
        }
        if (oldWidth == 1 && newWidth == 3) {
            return expand1to3(in, start, end, allocate);
        }
        if (oldWidth == 2 && newWidth == 3) {
            return expand2to3(in, start, end, allocate);
        }
        throw new IllegalArgumentException();
    }

    /**
     * Process a supplied string
     *
     * @param chars the characters to be processed
     */
    @Override
    public UnicodeBuilder accept(UnicodeString chars) {
        return append(chars);
    }

    @Override
    public void write(UnicodeString chars) {
        append(chars);
    }

    /**
     * Write a supplied string known to consist entirely of ASCII characters,
     * supplied as a byte array
     *
     * @param content byte array holding ASCII characters only
     */
    @Override
    public void writeAscii(byte[] content) throws IOException {
        accept(new Twine8(content));
    }

    /**
     * Process a supplied string
     *
     * @param chars the characters to be processed
     * @throws IOException if processing fails for any reason
     */
    @Override
    public void write(String chars) throws IOException {
        append(chars);
    }

    public void trimToSize() {
        // Used from bytecode: currently has no effect
    }

    /**
     * Complete the writing of characters to the result. The default implementation
     * does nothing.
     */
    @Override
    public void close() { }
}

