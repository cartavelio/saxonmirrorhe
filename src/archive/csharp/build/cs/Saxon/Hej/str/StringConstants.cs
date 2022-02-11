using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public class StringConstants {
        public static byte[] bytes(string s) {
            return System.Text.Encoding.ASCII.GetBytes(s);
        }
        public static readonly Saxon.Hej.str.UnicodeString SINGLE_SPACE = new Saxon.Hej.str.Twine8(bytes(" "));
        public static readonly Saxon.Hej.str.UnicodeString TRUE = new Saxon.Hej.str.Twine8(bytes("true"));
        public static readonly Saxon.Hej.str.UnicodeString FALSE = new Saxon.Hej.str.Twine8(bytes("false"));
        public static readonly Saxon.Hej.str.UnicodeString ONE = new Saxon.Hej.str.Twine8(bytes("1"));
        public static readonly Saxon.Hej.str.UnicodeString ZERO = new Saxon.Hej.str.Twine8(bytes("0"));
        public static readonly Saxon.Hej.str.UnicodeString ZERO_TO_NINE = new Saxon.Hej.str.Twine8(bytes("0123456789"));
        public static readonly Saxon.Hej.str.UnicodeString MIN_LONG = new Saxon.Hej.str.Twine8(bytes("-9223372036854775808"));
        public static readonly Saxon.Hej.str.UnicodeString POINT_ZERO = new Saxon.Hej.str.Twine8(bytes(".0"));
        public static readonly Saxon.Hej.str.UnicodeString ASTERISK = new Saxon.Hej.str.Twine8(bytes("*"));
        public static readonly byte[] COMMENT_START = bytes("<!--");
        public static readonly byte[] COMMENT_END = bytes("-->");
        public static readonly byte[] TWO_HYPHENS = bytes("--");
        public static readonly byte[] PI_START = bytes("<?");
        public static readonly byte[] PI_END = bytes("?>");
        public static readonly byte[] EMPTY_TAG_MIDDLE = bytes("></");
        public static readonly byte[] EMPTY_TAG_END = bytes("/>");
        public static readonly byte[] EMPTY_TAG_END_XHTML = bytes(" />");
        public static readonly byte[] END_TAG_START = bytes("</");
        public static readonly byte[] ESCAPE_LT = bytes("&lt;");
        public static readonly byte[] ESCAPE_GT = bytes("&gt;");
        public static readonly byte[] ESCAPE_AMP = bytes("&amp;");
        public static readonly byte[] ESCAPE_NL = bytes("&#xA;");
        public static readonly byte[] ESCAPE_CR = bytes("&#xD;");
        public static readonly byte[] ESCAPE_TAB = bytes("&#x9;");
        public static readonly byte[] ESCAPE_QUOT = bytes("&#34;");
        public static readonly byte[] ESCAPE_APOS = bytes("&#39;");
        public static readonly byte[] ESCAPE_NBSP = bytes("&nbsp;");
    
    }
}