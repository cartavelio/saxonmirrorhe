using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class Whitespace {
        private readonly static Saxon.Hej.regex.ARegularExpression anyWhitespace = Saxon.Hej.regex.ARegularExpression.compile(Saxon.Hej.str.StringTool.fromLatin1("[ \\n\\r\\t]+"), "");
        private readonly static Saxon.Ejava.util.regex.Pattern J_oneWhitespace = Saxon.Ejava.util.regex.Pattern.compile("[ \\n\\r\\t]");
        private readonly static Saxon.Ejava.util.regex.Pattern J_anyWhitespace = Saxon.Ejava.util.regex.Pattern.compile("[ \\n\\r\\t]+");
        private Whitespace()  {
        }
        public const int PRESERVE = 0;
        public const int REPLACE = 1;
        public const int COLLAPSE = 2;
        public const int TRIM = 3;
        public const int NONE = 0;
        public const int IGNORABLE = 1;
        public const int ALL = 2;
        public const int UNSPECIFIED = 3;
        public const int XSLT = 4;
        public static Saxon.Hej.str.UnicodeString applyWhitespaceNormalization(int action, Saxon.Hej.str.UnicodeString value) {
            switch (action) {
              case PRESERVE:
                return value;
              case REPLACE:
                Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder();
                Saxon.Hej.z.IntIterator iter = value.codePoints();
                while (iter.hasNext()) {
                    int c = iter.next();
                    switch (c) {
                      case '\n':
                      case '\r':
                      case '\t':
                        sb.append(' ');
                        break ;
                      default:
                        sb.append(c);
                        break ;
                    }
                }
                return sb.toUnicodeString();
              case COLLAPSE:
                return collapseWhitespace(value);
              case TRIM:
                return trim(value);
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException("Unknown whitespace facet value");
            }
        }
        public static string removeAllWhitespace(string value) {
            return J_oneWhitespace.matcher(value).replaceAll("");
        }
        public static Saxon.Hej.str.UnicodeString removeLeadingWhitespace(Saxon.Hej.str.UnicodeString value) {
            long start = trimmedStart(value);
            return ((start<0) ? Saxon.Hej.str.EmptyUnicodeString.getInstance() : value.substring(start));
        }
        public static bool containsWhitespace(Saxon.Hej.z.IntIterator codePoints) {
            while (codePoints.hasNext()) {
                int c = codePoints.next();
                if (((c<=32) && C0WHITE[c])) {
                    return true;
                }
            }
            return false;
        }
        public static bool isAllWhite(Saxon.Hej.str.UnicodeString content) {
            if ((content is Saxon.Hej.str.CompressedWhitespace)) {
                return true;
            }
            Saxon.Hej.z.IntIterator iter = content.codePoints();
            while (iter.hasNext()) {
                if (!(isWhite(iter.next()))) {
                    return false;
                }
            }
            return true;
        }
        private static readonly bool[] C0WHITE = {false, false, false, false, false, false, false, false, false, true, true, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true};
        private enum TokenCategory {
            INITIAL_WHITESPACE,
            SEPARATOR_WHITESPACE,
            FINAL_WHITESPACE,
            CONTENT,
        }
        private delegate void TokenHandler (int start, int end, Saxon.Hej.value.Whitespace.TokenCategory category);
        private static void tokenize(Saxon.Hej.z.IntIterator input, Saxon.Hej.value.Whitespace.TokenHandler handler) {
            int position = 0;
            int tokenStart = 0;
            Saxon.Hej.value.Whitespace.TokenCategory currentCategory = Saxon.Hej.value.Whitespace.TokenCategory.INITIAL_WHITESPACE;
            while (input.hasNext()) {
                int ch = input.next();
                if (isWhite(ch)) {
                    if ((currentCategory == Saxon.Hej.value.Whitespace.TokenCategory.CONTENT)) {
                        /*Delegate.handleToken()*/handler(tokenStart, position, currentCategory);
                        tokenStart = position;
                        currentCategory = Saxon.Hej.value.Whitespace.TokenCategory.SEPARATOR_WHITESPACE;
                    }
                } else {
                    if ((currentCategory != Saxon.Hej.value.Whitespace.TokenCategory.CONTENT)) {
                        if ((position>0)) {
                            /*Delegate.handleToken()*/handler(tokenStart, position, currentCategory);
                        }
                        tokenStart = position;
                        currentCategory = Saxon.Hej.value.Whitespace.TokenCategory.CONTENT;
                    }
                }
                position++;
            }
            if ((position>tokenStart)) {
                if ((currentCategory == Saxon.Hej.value.Whitespace.TokenCategory.SEPARATOR_WHITESPACE)) {
                    /*Delegate.handleToken()*/handler(tokenStart, position, Saxon.Hej.value.Whitespace.TokenCategory.FINAL_WHITESPACE);
                } else {
                    /*Delegate.handleToken()*/handler(tokenStart, position, currentCategory);
                }
            }
        }
        public static bool isWhite(int c) {
            return ((c<=32) && C0WHITE[c]);
        }
        public static Saxon.Hej.str.UnicodeString normalizeWhitespace(Saxon.Hej.str.UnicodeString input) {
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder();
            Saxon.Hej.z.IntIterator iter = input.codePoints();
            while (iter.hasNext()) {
                int c = iter.next();
                switch (c) {
                  case '\n':
                  case '\r':
                  case '\t':
                    sb.append(' ');
                    break ;
                  default:
                    sb.append(c);
                    break ;
                }
            }
            return sb.toUnicodeString();
        }
        public static Saxon.Hej.str.UnicodeString collapseWhitespace(Saxon.Hej.str.UnicodeString @in) {
            if (!(containsWhitespace(@in.codePoints()))) {
                return @in;
            }
            long len = trimmedEnd(@in);
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder();
            bool inWhitespace = true;
            for (long i = 0; (i<len); i++) {
                int c = @in.codePointAt(i);
                switch (c) {
                  case '\n':
                  case '\r':
                  case '\t':
                  case ' ':
                    if (!(inWhitespace)) {
                        sb.append(0x20);
                        inWhitespace = true;
                    }
                    break ;
                  default:
                    sb.append(c);
                    inWhitespace = false;
                    break ;
                }
            }
            return sb.toUnicodeString();
        }
        public static string collapseWhitespace(string @in) {
            if (!(containsWhitespace(Saxon.Hej.str.StringTool.codePoints(@in)))) {
                return @in;
            }
            return trim(J_anyWhitespace.matcher(@in).replaceAll(" "));
        }
        public static long trimmedStart(Saxon.Hej.str.UnicodeString @in) {
            long len = @in.length();
            for (int i = 0; (i<len); i++) {
                if (!(isWhite(@in.codePointAt(i)))) {
                    return i;
                }
            }
            return -(1L);
        }
        public static long trimmedEnd(Saxon.Hej.str.UnicodeString @in) {
            long len = @in.length();
            for (long i = (len-1); (i>=0); i--) {
                if (!(isWhite(@in.codePointAt(i)))) {
                    return (i+1);
                }
            }
            return 0;
        }
        public static Saxon.Hej.str.UnicodeString trim(Saxon.Hej.str.UnicodeString @in) {
            long start = trimmedStart(@in);
            if ((start == -1)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
            long end = trimmedEnd(@in);
            if (((start == 0) && (end == @in.length()))) {
                return @in;
            }
            return @in.substring(start, end);
        }
        public static string trim(string @in) {
            if ((@in == null)) {
                return null;
            }
            int firstNonWhite = -1;
            int lastNonWhite = -1;
            int len = @in.Length;
            for (int i = 0; (i<len); i++) {
                if (!(isWhite(@in[i]))) {
                    firstNonWhite = i;
                    break ;
                }
            }
            if ((firstNonWhite == -1)) {
                return "";
            }
            for (int i = (len-1); (i>=firstNonWhite); i--) {
                if (!(isWhite(@in[i]))) {
                    lastNonWhite = i;
                    break ;
                }
            }
            if (((firstNonWhite == 0) && (lastNonWhite == @in.Length))) {
                return @in;
            }
            return @in.Substring(firstNonWhite, (lastNonWhite+1) - firstNonWhite);
        }
        public static Saxon.Hej.str.UnicodeString collapse(Saxon.Hej.str.UnicodeString @in) {
            Saxon.Hej.str.UnicodeBuilder builder = new Saxon.Hej.str.UnicodeBuilder();
            tokenize(@in.codePoints(), (s, e, cat) => {
                Saxon.Hej.value.Whitespace.TokenCategory category = cat;
                switch (category) {
                  case Saxon.Hej.value.Whitespace.TokenCategory.CONTENT:
                    builder.accept(@in.substring(s, e));
                    break ;
                  case Saxon.Hej.value.Whitespace.TokenCategory.SEPARATOR_WHITESPACE:
                    builder.append(' ');
                    break ;
                  default:
                    break ;
                }
            } );
            return builder.toUnicodeString();
        }
        public static string collapse(string @in) {
            Saxon.Ejava.lang.StringBuilder builder = new Saxon.Ejava.lang.StringBuilder(@in.Length);
            tokenize(Saxon.Hej.str.StringTool.codePoints(@in), (s, e, cat) => {
                Saxon.Hej.value.Whitespace.TokenCategory category = cat;
                switch (category) {
                  case Saxon.Hej.value.Whitespace.TokenCategory.CONTENT:
                    builder.append(@in.Substring(s, e - s));
                    break ;
                  case Saxon.Hej.value.Whitespace.TokenCategory.SEPARATOR_WHITESPACE:
                    builder.append(' ');
                    break ;
                  default:
                    break ;
                }
            } );
            return builder.ToString();
        }
        public static Saxon.Hej.str.UnicodeString normalize(Saxon.Hej.str.UnicodeString @in) {
            Saxon.Hej.str.UnicodeBuilder builder = new Saxon.Hej.str.UnicodeBuilder();
            tokenize(@in.codePoints(), (s, e, cat) => {
                Saxon.Hej.value.Whitespace.TokenCategory category = cat;
                switch (category) {
                  case Saxon.Hej.value.Whitespace.TokenCategory.CONTENT:
                    builder.accept(@in.substring(s, e));
                    break ;
                  case Saxon.Hej.value.Whitespace.TokenCategory.SEPARATOR_WHITESPACE:
                    for (int i = s; (i<e); i++) {
                        builder.append(' ');
                    }
                    break ;
                  default:
                    break ;
                }
            } );
            return builder.toUnicodeString();
        }
        public static string normalize(string @in) {
            Saxon.Ejava.lang.StringBuilder builder = new Saxon.Ejava.lang.StringBuilder(@in.Length);
            tokenize(Saxon.Hej.str.StringTool.codePoints(@in), (s, e, cat) => {
                Saxon.Hej.value.Whitespace.TokenCategory category = cat;
                switch (category) {
                  case Saxon.Hej.value.Whitespace.TokenCategory.CONTENT:
                    builder.append(@in.Substring(s, e - s));
                    break ;
                  case Saxon.Hej.value.Whitespace.TokenCategory.SEPARATOR_WHITESPACE:
                    for (int i = s; (i<e); i++) {
                        builder.append(' ');
                    }
                    break ;
                  default:
                    break;
                }
            } );
            return builder.ToString();
        }
        public class Tokenizer : Saxon.Hej.tree.iter.AtomicIterator {
            private readonly Saxon.Hej.str.UnicodeString input;
            private long position;
            public Tokenizer(string input)  {
                this.input = Saxon.Hej.str.StringView.tidy(input);
                this.position = 0;
            }
            public Tokenizer(Saxon.Hej.str.UnicodeString input)  {
                this.input = input.tidy();
                this.position = 0;
            }
            public Saxon.Hej.om.Item next() {
                long start = position;
                long eol = input.length();
                while (((start<eol) && isWhite(input.codePointAt(start)))) {
                    start++;
                }
                if ((start>=eol)) {
                    return null;
                }
                long end = start;
                while (((end<eol) && !(isWhite(input.codePointAt(end))))) {
                    end++;
                }
                position = end;
                return new Saxon.Hej.value.StringValue(input.substring(start, end));
            }
            public void close() {
            }
        
        }
    
    }
}