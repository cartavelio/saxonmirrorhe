package net.sf.saxon.str;

import net.sf.saxon.serialize.charcode.UTF16CharacterSet;
import net.sf.saxon.z.IntIterator;

import java.io.IOException;
import java.io.Writer;

/**
 * This class is analogous to the Java CharSequence interface, but it provides access to Unicode codepoints
 * rather than UCS-2 characters. Except where otherwise stated, "character" means a Unicode codepoint,
 * not a Java char.
 * <p>The interface is future-proofed to support code points in the range 0 to 2^31, and string lengths of
 * up to 2^63 characters. Implementations may (and do) impose lower limits.</p>
 */
public abstract class CodePoints implements Comparable<CodePoints> {

    /**
     * Ensure that the implementation is capable of counting codepoints in the string.
     * <p>The method changes the implementation if necessary, returning a new {@link UnicodeString} that
     * supports operations such as {@link UnicodeString#codePointAt(long)} and {@link UnicodeString#substring(long, long)}.</p>
     * @implNote Some implementations intrinsically support codepoint access; in those cases, this method
     * is a no-op. With other implementations, this method may scan the string to see if it contains
     * any surrogate pairs. If there are none, this fact is noted, so the scan need never be done more
     * than once. If there are surrogate pairs, then (and only then) will the data be copied to a new
     * implementation that uses fixed width characters (3 bytes per codePoint).
     * @return if necessary, a new {@code UniString}, with the same content as this one, but which supports
     * codepoint sensitive operations. If the implementation already supports such operations,
     * then this {@code UniString} object is returned unchanged.
     */

    public abstract UnicodeString tidy();

    /**
     * Get an equivalent UniString that uses the most economical representation available
     * @return an equivalent UniString
     */

    public UnicodeString economize() {
        return tidy();
    }

    /**
     * Get the estimated length of the string, suitable for space allocation.
     *
     * @return an estimate of the number of code points in the string. If the {@code UniString}
     * has been prepared for codepoint access then this will return the number of codepoints;
     * if not, it will return some other value, such as the number of 2-byte char units.
     */

    public abstract long estimatedLength();

    /**
     * Ask whether the string is empty
     * @return true if the length of the string is zero
     */

    public abstract boolean isEmpty();

    /**
     * Get the effective boolean value of a string
     *
     * @return true if the string has length greater than zero
     */

    public boolean effectiveBooleanValue() {
        return !isEmpty();
    }

    /**
     * Make an immutable copy of this UniString. Most implementations of UniString are
     * immutable, so this is a no-op; but it is possible to create implementations
     * that are unsafe because they are views of a mutable object.
     * In such an implementation, this method should return a truly immutable copy of the
     * string.
     * @return an immutable copy of this UniString, or this string unchanged if it is
     * already immutable.
     */

    public CodePoints save() {
        return this;
    }


    /**
     * Get an iterator over the code points present in the string. Note that this method
     * is always available, whether or not the {@code UniString} has been prepared for codePoint access.
     * @return an iterator that delivers the individual code points
     */

    public abstract IntIterator codePoints();

    /**
     * Concatenate another string
     * @param other the string to be appended to this one
     * @return the result of the concatenation (neither input string is altered)
     */

    public CodePoints concat(CodePoints other) {
        UnicodeBuilder ub = new UnicodeBuilder();
        return ub.append(this).append(other).toUnicodeString();
    }

    @Override
    public boolean equals(Object obj) {
        if (obj instanceof CodePoints) {
            CodePoints other = (CodePoints)obj;
            IntIterator iter1 = codePoints();
            IntIterator iter2 = other.codePoints();
            while (true) {
                boolean more1 = iter1.hasNext();
                boolean more2 = iter2.hasNext();
                if (more1 && more2) {
                    int ch1 = iter1.next();
                    int ch2 = iter2.next();
                    if (ch1 != ch2) {
                        return false;
                    }
                } else {
                    return !(more1 || more2);
                }
            }
        } else {
            return false;
        }
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
        int h = 0;
        IntIterator iter = codePoints();
        while (iter.hasNext()) {
            int cp = iter.next();
            if ((cp & 0xff0000) != 0) {
                h = 31 * h + UTF16CharacterSet.highSurrogate(cp);
                h = 31 * h + UTF16CharacterSet.lowSurrogate(cp);
            } else {
                h = 31 * h + cp;
            }
        }
        return h;
    }

    @Override
    public int compareTo(CodePoints other) {
        IntIterator iter1 = codePoints();
        IntIterator iter2 = other.codePoints();
        while (true)  {
            boolean more1 = iter1.hasNext();
            boolean more2 = iter2.hasNext();
            if (more1 && more2) {
                int ch1 = iter1.next();
                int ch2 = iter2.next();
                int diff = ch1 - ch2;
                if (diff != 0) {
                    return diff;
                }
            } else if (!more1 && !more2) {
                return 0;
            } else {
                return more1 ? 1 : -1;
            }
        }
    }

    /**
     * Write the content of this string to a Writer
     * @param writer the writer to which the string should be written
     */

    public abstract void write(Writer writer) throws IOException;

}

