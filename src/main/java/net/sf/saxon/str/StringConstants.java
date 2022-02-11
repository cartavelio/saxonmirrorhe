package net.sf.saxon.str;

import java.nio.charset.StandardCharsets;

public class StringConstants {

    public static byte[] bytes(String s) {
        return s.getBytes(StandardCharsets.US_ASCII);
    }

    public static final UnicodeString SINGLE_SPACE = new Twine8(new byte[]{0x20});
    public static final UnicodeString TRUE = new Twine8(new byte[]{'t','r','u','e'});
    public static final UnicodeString FALSE = new Twine8(new byte[]{'f','a','l','s','e'});
    public static final UnicodeString ONE = new Twine8(new byte[]{'1'});
    public static final UnicodeString ZERO = new Twine8(new byte[]{'0'});

    public static final byte[] COMMENT_START = bytes("<!--");
    public static final byte[] COMMENT_END = bytes("-->");
    public static final byte[] PI_START = bytes("<?");
    public static final byte[] PI_END = bytes("?>");
    public static final byte[] EMPTY_TAG_MIDDLE = bytes("></");
    public static final byte[] EMPTY_TAG_END = bytes("/>");
    public static final byte[] EMPTY_TAG_END_XHTML = bytes(" />");
    public static final byte[] END_TAG_START = bytes("</");

    public static final byte[] ESCAPE_LT = bytes("&lt;");
    public static final byte[] ESCAPE_GT = bytes("&gt;");
    public static final byte[] ESCAPE_AMP = bytes("&amp;");
    public static final byte[] ESCAPE_NL = bytes("&#xA;");
    public static final byte[] ESCAPE_CR = bytes("&#xD;");
    public static final byte[] ESCAPE_TAB = bytes("&#x9;");
    public static final byte[] ESCAPE_QUOT = bytes("&#34;");
    public static final byte[] ESCAPE_APOS = bytes("&#39;");
    public static final byte[] ESCAPE_NBSP = bytes("&nbsp;");
}

