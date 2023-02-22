package net.sf.saxon.str;

import net.sf.saxon.serialize.UTF8Writer;
import net.sf.saxon.z.IntIterator;

import java.io.IOException;
import java.io.Writer;
import java.nio.charset.StandardCharsets;
import java.util.function.IntPredicate;

/**
 * A Unicode string consisting entirely of 8-bit characters, implemented as a range
 * of an underlying byte array
 */

class Slice8 extends UnicodeString {

    private byte[] bytes;
    private int start;
    private int end;
    private int cachedHash;

    /**
     * Create a slice of an underlying byte array
     * @param bytes the byte array, containing Unicode codepoints in the range 0-255
     * @param start the offset of the first character within the byte array
     * @param end the offset of the first excluded byte, so the length of the string
     *            is {@code end-start}
     */

    public Slice8(byte[] bytes, int start, int end) {
        this.bytes = bytes;
        this.start = start;
        this.end = end;
    }

    @Override
    public long length() {
        return end - start;
    }

    @Override
    public int getWidth() {
        return 8;
    }

    @Override
    public long indexOf(int codePoint, long from) {
        if (codePoint > 255) {
            return -1;
        }
        ;
        byte b = (byte) (codePoint & 0xff);
        byte[] array = bytes;
        int limit = end;
        for (int i = start + requireInt(from); i < limit; i++) {
            if (array[i] == b) {
                return i - start;
            }
        }
        return -1;
    }

    @Override
    public int codePointAt(long index) {
        return (bytes[start + requireInt(index)]) & 0xff;
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
            return new Slice8(bytes, requireInt(start) + this.start, requireInt(end) + this.start);
        }
    }

    @Override
    public void supplyContent(LargeTextBuffer consumer, long start, long len) {
        consumer.append8(bytes, requireInt(start) + this.start, requireInt(len));
    }

    @Override
    public void write(Writer writer) throws IOException {
        if (writer instanceof UTF8Writer) {
            ((UTF8Writer) writer).writeLatin1(bytes, start, end - start);
        } else {
            writer.write(toString());
        }
    }

    @Override
    public void write(Writer writer, long start, long len) throws IOException {
        if (writer instanceof UTF8Writer) {
            ((UTF8Writer) writer).writeLatin1(bytes, this.start + requireInt(start), requireInt(len));
        } else {
            writer.write(substring(requireInt(start), requireInt(start + len)).toString());
        }
    }

    @Override
    public long writeUntil(Writer writer, long start, long limit, IntPredicate stop) throws IOException {
        int end = this.start + requireInt(limit);
        for (int i = this.start + requireInt(start); i < end; i++) {
            if (stop.test((int) (bytes[i] & 0xff))) {
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
                return bytes[i++] & 0xff;
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
            byte b = bytes[i];
            h = 31 * h + (b & 0xff);
        }
        return cachedHash = h;
    }

    /**
     * Display as a string.
     */

    /*@NotNull*/
    public String toString() {
        return new String(bytes, start, end - start, StandardCharsets.ISO_8859_1);
    }

}

