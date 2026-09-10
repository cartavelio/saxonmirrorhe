package net.sf.saxon.str;

import net.sf.saxon.Configuration;
import net.sf.saxon.serialize.charcode.UTF16CharacterSet;
import net.sf.saxon.z.IntIterator;

import java.util.function.IntPredicate;

/**
 * An implementation of {@code UnicodeString} that wraps a Java string which is known to contain
 * no surrogates. That is, all the characters in the string are in the Basic Multilingual
 * Plane (their codepoints are in the range 0-65535).
 */

public class BMPString extends UnicodeString {

    private String base;

    /**
     * Protected constructor
     * @param base the string to be wrapped: the caller is responsible for ensuring this
     *             contains no surrogates
     */

    protected BMPString(String base) {
        this.base = base;
    }

    /**
     * Wrap a String, which must contain no surrogates
     * @param base the string. The caller warrants that this string contains no surrogates;
     *             this condition is checked only if Java assertions are enabled.
     * @return the wrapped string.
     */

    public static UnicodeString of(String base) {
        if (Configuration.isAssertionsEnabled()) {
            for (int i=0; i<base.length(); i++) {
                assert !UTF16CharacterSet.isSurrogate(base.charAt(i));
            }
        }
        return new BMPString(base);
    }


    @Override
    public long length() {
        return base.length();
    }

    @Override
    public boolean isEmpty() {
        return base.isEmpty();
    }

    @Override
    public int getWidth() {
        return 16;
    }

    @Override
    public IntIterator codePoints() {
        return StringTool.codePoints(base);
    }

    @Override
    public long indexOf(int codePoint) {
        if (codePoint > 65535) {
            return -1;
        }
        return base.indexOf((char)codePoint);
    }

    @Override
    public long indexOf(int codePoint, long from) {
        int from32 = requireInt(from);
        if (codePoint > 65535) {
            return -1;
        }
        return base.indexOf((char)codePoint, from32);
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
        for (int i=requireInt(from); i<base.length(); i++) {
            if (predicate.test(base.charAt(i))) {
                return i;
            }
        }
        return -1;
    }

    @Override
    public int codePointAt(long index) {
        return base.charAt(StringTool.requireInt(index));
    }

    @Override
    public UnicodeString substring(long start, long end) {
        int start32 = StringTool.requireInt(start);
        int end32 = StringTool.requireInt(end);
        return new BMPString(base.substring(start32, end32));
    }

    @Override
    public CodePoints concat(CodePoints other) {
        if (other instanceof BMPString) {
            return new BMPString(base + ((BMPString)other).base);
        } else {
            UnicodeBuilder ub = new UnicodeBuilder();
            return ub.accept(this).accept(other).toUnicodeString();
        }
    }

    @Override
    public int compareTo(CodePoints other) {
        if (other instanceof BMPString) {
            return base.compareTo(((BMPString)other).base);
        } else {
            return super.compareTo(other);
        }
    }

    @Override
    public boolean equals(Object obj) {
        if (obj instanceof BMPString) {
            return this.base.equals(((BMPString)obj).base);
        } else {
            return super.equals(obj);
        }
    }

    @Override
    public int hashCode() {
        return base.hashCode();
    }

    @Override
    public UnicodeString economize() {
        return this;
    }

    @Override
    public String toString() {
        return base;
    }

    @Override
    public void supplyContent(LargeTextBuffer consumer, long start, long length) {
        consumer.append16(base.toCharArray(), requireInt(start), requireInt(length));
    }


}

