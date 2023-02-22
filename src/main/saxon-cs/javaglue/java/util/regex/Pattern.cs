using System;

namespace java.util.regex {
    public class Pattern {
        
        public const int UNIX_LINES = 1;
        public const int MULTILINE = 2;
        public const int CASE_INSENSITIVE = 4;
        public const int DOTALL = 8;
        public const int COMMENTS = 16;
        public const int UNICODE_CASE = 32;
        public const int LITERAL = 64;
        public const int CANON_EQ = 128;
        public static Pattern compile(string regex) {
            
        }
        public static Pattern compile(string regex, int flags) {
            
        }

        public string pattern() {
            throw new NotImplementedException();
        }

        public Matcher matcher(string input) {
            return new Matcher(input);
        }
    }
}