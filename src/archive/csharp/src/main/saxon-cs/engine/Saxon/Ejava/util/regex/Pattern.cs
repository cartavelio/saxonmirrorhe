using System;
using System.Text.RegularExpressions;

namespace Saxon.Ejava.util.regex {
    public class Pattern {
        
        public const int UNIX_LINES = 1;
        public const int MULTILINE = 2;
        public const int CASE_INSENSITIVE = 4;
        public const int DOTALL = 8;
        public const int COMMENTS = 16;
        public const int UNICODE_CASE = 32;
        public const int LITERAL = 64;
        public const int CANON_EQ = 128;

        private Regex regex;

        private Pattern(Regex regex) {
            this.regex = regex;
        }
        public static Pattern compile(string pattern) {
            return new Pattern(new Regex(pattern));
        }
        public static Pattern compile(string pattern, int flags) {
            RegexOptions options = RegexOptions.None;
            if ((flags & Saxon.Ejava.util.regex.Pattern.UNIX_LINES) != 0) {
                options |= RegexOptions.Multiline;
            }
            if ((flags & Saxon.Ejava.util.regex.Pattern.MULTILINE) != 0) {
                options |= RegexOptions.Multiline;
            }
            if ((flags & Saxon.Ejava.util.regex.Pattern.CASE_INSENSITIVE) != 0) {
                options |= RegexOptions.IgnoreCase;
            }
            if ((flags & Saxon.Ejava.util.regex.Pattern.DOTALL) != 0) {
                options |= RegexOptions.Singleline;
            }
            if ((flags & Saxon.Ejava.util.regex.Pattern.COMMENTS) != 0) {
                options |= RegexOptions.IgnorePatternWhitespace;
            }
            return new Pattern(new Regex(pattern, options)); 
        }
 

        public string pattern() {
            return regex.ToString();
        }

        public Matcher matcher(string input) {
            return new Matcher(regex, input);
        }
    }
}