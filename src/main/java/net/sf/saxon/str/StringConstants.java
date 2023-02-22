package net.sf.saxon.str;

import net.sf.saxon.transpile.CSharpReplaceBody;

import java.nio.charset.StandardCharsets;

public class StringConstants {

    @CSharpReplaceBody(code="return System.Text.Encoding.ASCII.GetBytes(s);")
    public static byte[] bytes(String s) {
        return s.getBytes(StandardCharsets.US_ASCII);
    }

    public static final UnicodeString SINGLE_SPACE = new Twine8(bytes(" "));
    public static final UnicodeString TRUE = new Twine8(bytes("true"));
    public static final UnicodeString FALSE = new Twine8(bytes("false"));
    public static final UnicodeString ONE = new Twine8(bytes("1"));
    public static final UnicodeString ZERO = new Twine8(bytes("0"));

    public static final UnicodeString MIN_LONG = new Twine8(bytes("-9223372036854775808"));

    public static final byte[] COMMENT_START = bytes("<!--");
    public static final byte[] COMMENT_END = bytes("-->");
    public static final byte[] TWO_HYPHENS = bytes("--");
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

