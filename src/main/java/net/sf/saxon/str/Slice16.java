package net.sf.saxon.str;

import net.sf.saxon.z.IntIterator;

import java.io.IOException;
import java.io.Writer;
import java.util.function.IntPredicate;

/**
 * A Unicode string consisting entirely of 16-bit BMP characters, implemented as a range
 * of an underlying byte array
 */

class Slice16 extends UnicodeString {

    private char[] chars;
    private int start;
    private int end;
    private int cachedHash;

    /**
     * Create a slice of an underlying char array
     * @param chars the char array, containing Unicode codepoints in the range 0-65535;
     *              the caller warrants that there are no surrogate characters present
     * @param start the offset of the first character within the character array
     * @param end the offset of the first excluded character, so the length of the string
     *            is {@code end-start}
     */

    public Slice16(char[] chars, int start, int end) {
        this.chars = chars;
        this.start = start;
        this.end = end;
    }

    @Override
    public long length() {
        return end - start;
    }

    @Override
    public int getWidth() {
        return 16;
    }

    @Override
    public long indexOf(int codePoint, long from) {
        if (codePoint > 65535) {
            return -1;
        }
        char b = (char)(codePoint & 0xffff);
        int limit = end;
        for (int i = start + requireInt(from); i < limit; i++) {
            if (chars[i] == b) {
                return i - start;
            }
        }
        return -1;
    }

    @Override
    public int codePointAt(long index) {
        return (chars[start + requireInt(index)]);
    }

    @Override
    public UnicodeString substring(long start, long end) {
        long len = length();
        if (start < 0 || end < start || end > len) {
            throw new IndexOutOfBoundsException();
        }
        if (end == start) {
            return EmptyUniString.getInstance();
        } else {
            return new Slice16(chars, requireInt(start) + this.start, requireInt(end) + this.start);
        }
    }

    @Override
    public void supplyContent(LargeTextBuffer consumer, long start, long len) {
        consumer.append16(chars, requireInt(start) + this.start, requireInt(len));
    }

    @Override
    public void write(Writer writer) throws IOException {
        writer.write(chars, start, end-start);
    }

    @Override
    public void write(Writer writer, long start, long len) throws IOException {
        writer.write(chars, this.start + requireInt(start), requireInt(len));
    }

    @Override
    public long writeUntil(Writer writer, long start, long limit, IntPredicate stop) throws IOException {
        int end = this.start + requireInt(limit);
        for (int i = this.start + requireInt(start); i < end; i++) {
            if (stop.test(chars[i])) {
                write(writer, start, i - this.start - start);
                return i - this.start;
            }
        }
        write(writer, start, limit - start);
        return -1;
    }

    @Override
    public IntIterator codePoints() {
        return new IntIterator() {
            int i = start;

            @Override
            public boolean hasNext() {
                return i < end;
            }

            @Override
            public int next() {
                return chars[i++];
            }
        };
    }

    /**
     * Compute a hashCode. All implementations of {@code UniString use compatible hash codes and the
     * hashing algorithm is therefore identical to that for {@code java.lang.String}. This means
     * that for strings containing Astral characters, the hash code needs to be computed by decomposing
     * an Astral character into a surrogate pair.
     *
     * @return the hash code
     */

    public int hashCode() {
        if (cachedHash != 0) {
            return cachedHash;
        }
        int h = 0;
        for (int i = start; i < end; i++) {
            int b = chars[i];
            h = 31 * h + b;
        }
        return cachedHash = h;
    }

    /**
     * Convert to a string.
     */

    /*@NotNull*/
    public String toString() {
        return new String(chars, start, end - start);
    }

}

