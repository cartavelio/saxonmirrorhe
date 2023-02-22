package net.sf.saxon.str;

import net.sf.saxon.serialize.charcode.UTF16CharacterSet;
import net.sf.saxon.z.IntIterator;

import java.io.IOException;
import java.io.Writer;
import java.util.Objects;

/**
 * An implementation of UniString that wraps an ordinary Java string.
 */

public class StringView extends CodePoints {

    private String base;
    private boolean codePointSafe;

//    public static boolean canPeek = true;
//    private static Method coder;

    private StringView(String base) {
        Objects.requireNonNull(base);
        this.base = base;
    }

    public static CodePoints of(String base) {
        return new StringView(base);
    }

    public static UnicodeString wrap(String base) {
        return new StringView(base).tidy();
    }

    public static UnicodeString tidy(String base) {
        return new StringView(base).tidy();
    }

//    /**
//     * Ask whether a string is known to consist entirely of Latin-1 characters. From Java 9,
//     * the {@code java.lang.String} class uses one-byte-per-character encoding where possible.
//     * If this format is being used internally, then we know that the string cannot contain
//     * any surrogate pairs, which means we can mark it as being codePoint-safe without doing
//     * further checks.
//     * @implNote The implementation of this method relies on Java reflection to check the internal
//     * coding of the string. If this fails once (for example because we're using Java 8), then
//     * we assume it will always fail, and we don't try again.
//     * @param string the string to be tested
//     * @return true if the internal implementation of the string shows that it consists
//     * entirely of single-byte characters, which means there can be no surrogate pairs
//     */
//    private static boolean isLatin1Compression(String string) {
//        if (canPeek && !"false".equals(System.getProperty("saxon.string.inspection"))) {
//            if ("false".equals(System.getProperty("saxon.string.inspection"))) {
//                canPeek = false;
//            } else if (coder == null) {
//                    try {
//                        System.err.println("WARNING: Saxon is performing optimizations which may generate warning messages");
//                        System.err.println("WARNING: To suppress these optimizations, set system property saxon.string.inspection=false");
//                        coder = String.class.getDeclaredMethod("coder");
//                        coder.setAccessible(true);
//                    } catch (NoSuchMethodException e) {
//                        canPeek = false;
//                        return false;
//                    }
//                }
//            if (coder != null) {
//                try {
//                    // coder() == 0 implies one byte per character, which implies no surrogate pairs
//                    return (byte) coder.invoke(string) == 0;
//                } catch (IllegalAccessException | InvocationTargetException e) {
//                    coder = null;
//                    canPeek = false;
//                    return false;
//                }
//            }
//        }
//        return false;
//    }

    @Override
    public synchronized UnicodeString tidy() {
        if (codePointSafe) {
            return new BMPString(base);
        } else {
            for (int i = 0; i < base.length(); i++) {
                if (UTF16CharacterSet.isSurrogate(base.charAt(i))) {
                    UnicodeBuilder ub = new UnicodeBuilder();
                    ub.append(StringTool.codePoints(base));
                    return ub.toUnicodeString();
                }
            }
            codePointSafe = true;
            return new BMPString(base);
        }
    }


    @Override
    public boolean isEmpty() {
        return base.isEmpty();
    }

    @Override
    public long estimatedLength() {
        return base.length();
    }

    @Override
    public IntIterator codePoints() {
        return StringTool.codePoints(base);
    }

    @Override
    public CodePoints concat(CodePoints other) {
        if (other instanceof StringView) {
            StringView sv = new StringView(base + ((StringView)other).base);
            sv.codePointSafe = this.codePointSafe && ((StringView) other).codePointSafe;
            return sv;
        } else {
            UnicodeBuilder ub = new UnicodeBuilder();
            return ub.append(this).append(other).toUnicodeString();
        }
    }

    @Override
    public int compareTo(CodePoints other) {
        if (codePointSafe && other instanceof StringView && ((StringView)other).codePointSafe) {
            return base.compareTo(((StringView)other).base);
        } else {
            return super.compareTo(other);
        }
    }

    @Override
    public boolean equals(Object obj) {
        if (obj instanceof StringView) {
            return this.base.equals(((StringView) obj).base);
        } else if (obj instanceof BMPString) {
            return this.base.equals(((BMPString) obj).toString());
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
        return tidy();
    }

    @Override
    public String toString() {
        return base;
    }

    @Override
    public void write(Writer writer) throws IOException {
        writer.write(base);
    }
}

