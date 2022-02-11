package net.sf.saxon.str;

import net.sf.saxon.type.AtomicType;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.z.IntIterator;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.function.Consumer;

/**
 * Builder class to construct a UniString by appending text incrementally
 */

public final class UnicodeBuilder implements UniStringConsumer, UnicodeWriter {

    // We maintain the data as a list of segments, followed by a list of individual loose
    // codepoints. When short strings are added, we append to the list of loose codepoints.
    // When longer strings are added, we flush the list of loose codepoints, and add the string
    // to the list of segments
    private List<UnicodeString> segments;
    private List<Integer> looseCodepoints;
    long _length = 0;
    int mask = 0;
    Consumer<UnicodeString> segmentConsumer = null;

    public UnicodeBuilder() {
        segments = new ArrayList<>();
    }

    public UnicodeBuilder(Consumer<UnicodeString> consumer) {
        segments = new ArrayList<>();
        segmentConsumer = consumer;
    }

    /**
     * Append a character (which must not be a surrogate. (Method needed for C#, because implicit
     * conversion of char to int isn't supported)
     * @param ch the character
     * @return the builder, with the new character added
     */
    public UnicodeBuilder append(char ch) {
        append((int)ch);
        return this;
    }

    /**
     * Append a single unicode character to the content
     * @param codePoint the unicode codepoint. The caller is responsible for ensuring that this
     *                  is not a surrogate
     * @return the builder, with the new character added
     */

    public UnicodeBuilder append(int codePoint) {
        if (looseCodepoints == null) {
            looseCodepoints = new ArrayList<>();
        }
        looseCodepoints.add(codePoint);
        _length++;
        mask |= codePoint;
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
        return appendCodePoints(StringTool.codePoints(str));
    }

    /**
     * Append a Java CharSequence to the content. This may contain arbitrary characters including
     * well formed surrogate pairs
     *
     * @param str the string to be appended
     * @return the builder, with the new string added
     */

    public UnicodeBuilder append(CharSequence str) {
        return appendCodePoints(StringTool.codePoints(str));
    }

    /**
     * Append a UniString to the content.
     *
     * @param str the string to be appended. The length is currently restricted to 2^31.
     * @return the builder, with the new string added
     */

    public UnicodeBuilder append(CodePoints str) {
        if (!str.isEmpty()) {
            flushLooseCodepoints();
            UnicodeString t = str.tidy();
            segments.add(t);
            _length += t.length();
            switch (t.getWidth()) {
                case 7:
                case 8:
                    mask |= 0xff;
                    break;
                case 16:
                    mask |= 0xffff;
                    break;
                default:
                    mask |= 0xffffff;
                    break;
            }
        }
        return this;
    }

    private void flushLooseCodepoints() {
        if (looseCodepoints != null) {
            List<Integer> loose = looseCodepoints;
            looseCodepoints = null;
            _length -= loose.size();
            int max = 0;
            for (int i : loose) {
                max = Math.max(max, i);
            }
            if (max < 255) {
                byte[] content = new byte[loose.size()];
                int k = 0;
                for (int i : loose) {
                     content[k++] = (byte)(i & 0xff);
                }
                accept(new Twine8(content));
            } else if (max < 65535) {
                char[] content = new char[loose.size()];
                int k = 0;
                for (int i : loose) {
                    content[k++] = (char) (i & 0xffff);
                }
                accept(new Twine16(content));
            } else {
                byte[] content = new byte[loose.size() * 3];
                int k = 0;
                for (int cp : loose) {
                    content[k++] = (byte) ((cp >> 16) & 0xff);
                    content[k++] = (byte) ((cp >> 8) & 0xff);
                    content[k++] = (byte) (cp & 0xff);
                }
                accept(new Twine24(content));
            }
        }
    }

    /**
     * Append a sequence of Unicode characters to the content.
     *
     * @param codePoints An iterator over the sequence of Unicode codePoints to be added.
     *                   The method consumes this iterator.
     * @return the builder, with the new string added
     */

    public UnicodeBuilder appendCodePoints (IntIterator codePoints) {
        while (codePoints.hasNext()) {
            append(codePoints.next());
        }
        return this;
    }

    /**
     * Get the number of codepoints currently in the builder
     * @return the size in codepoints
     */
    public long length() {
        return _length;
    }

    /**
     * Ask whether the content of the builder is empty
     * @return true if the size is zero
     */
    public boolean isEmpty() {
        return _length == 0;
    }

    /**
     * Construct a UniString whose value is formed from the contents of this builder
     * @return the constructed {@link UnicodeString}
     */

    public UnicodeString toUnicodeString() {
        if (_length == 0) {
            return EmptyUniString.getInstance();
        } else if (mask <= 0xff) {
            byte[] target = new byte[(int)length()];
            int k = 0;
            for (UnicodeString t : segments) {
                if (t instanceof Twine8) {
                    byte[] content = ((Twine8)t).getByteArray();
                    System.arraycopy(content, 0, target, k, content.length);
                    k += content.length;
                } else {
                    IntIterator iter = t.codePoints();
                    while (iter.hasNext()) {
                        target[k++] = (byte)iter.next();
                    }
                }
            }
            if (looseCodepoints != null) {
                for (Integer cp : looseCodepoints) {
                    target[k++] = (byte) (cp & 0xff);
                }
            }
            return new Twine8(target);
        } else if (mask <= 0xffff) {
            char[] target = new char[(int) length()];
            int k = 0;
            for (UnicodeString t : segments) {
                if (t instanceof Twine8) {
                    byte[] content = ((Twine8)t).getByteArray();
                    for (byte b : content) {
                        target[k++] = (char)(b & 0xff);
                    }
                } else if (t instanceof Twine16) {
                    char[] content = ((Twine16) t).getCharArray();
                    System.arraycopy(content, 0, target, k, content.length);
                    k += content.length;
                } else {
                    IntIterator iter = t.codePoints();
                    while (iter.hasNext()) {
                        target[k++] = (char) iter.next();
                    }
                }
            }
            if (looseCodepoints != null) {
                for (Integer cp : looseCodepoints) {
                    target[k++] = (char) (int) cp;
                }
            }
            return new Twine16(target);
        } else {
            byte[] target = new byte[(int) length()*3];
            int k = 0;
            for (UnicodeString t : segments) {
                if (t instanceof Twine8) {
                    byte[] content = ((Twine8) t).getByteArray();
                    for (byte b : content) {
                        target[k++] = 0;
                        target[k++] = 0;
                        target[k++] = (byte) (b & 0xff);
                    }
                } else if (t instanceof Twine16) {
                    char[] content = ((Twine16) t).getCharArray();
                    for (char c : content) {
                        target[k++] = 0;
                        target[k++] = (byte) ((c >> 8) & 0xff);
                        target[k++] = (byte) (c & 0xff);
                    }
                } else if (t instanceof Twine24) {
                    byte[] content = ((Twine24) t).getByteArray();
                    System.arraycopy(content, 0, target, k, content.length);
                    k += content.length;
                } else {
                    IntIterator iter = t.codePoints();
                    while (iter.hasNext()) {
                        int cp = iter.next();
                        target[k++] = (byte) ((cp >> 16) & 0xff);
                        target[k++] = (byte) ((cp >> 8) & 0xff);
                        target[k++] = (byte) (cp & 0xff);
                    }
                }
            }
            if (looseCodepoints != null) {
                for (Integer cp : looseCodepoints) {
                    target[k++] = (byte) ((cp >> 16) & 0xff);
                    target[k++] = (byte) ((cp >> 8) & 0xff);
                    target[k++] = (byte) (cp & 0xff);
                }
            }
            return new Twine24(target);
        }
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
        StringBuilder sb = new StringBuilder();
        for (CodePoints s : segments) {
            sb.append(s.toString());
        }
        if (looseCodepoints != null) {
            for (int cp : looseCodepoints) {
                sb.appendCodePoint(cp);
            }
        }
        return sb.toString();
    }



    /**
     * Reset the contents of this builder to be empty
     */

    public void clear() {
        segments = new ArrayList<>();
        looseCodepoints = null;
        _length = 0;
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
     *                 newWidth <= oldWidth then the input array is copied; the width is never reduced
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
    public UnicodeBuilder accept(CodePoints chars) {
        return append(chars);
    }

    @Override
    public void write(CodePoints chars) {
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

    /**
     * Complete the writing of characters to the result. The default implementation
     * does nothing.
     */
    @Override
    public void close() { }
}

