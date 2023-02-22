using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class JavaRegularExpression : Saxon.Hej.regex.RegularExpression {
        internal Saxon.Ejava.util.regex.Pattern pattern;
        internal string javaRegex;
        internal int flagBits;
        public JavaRegularExpression(Saxon.Hej.str.UnicodeString javaRegex, string flags)  {
            this.flagBits = setFlags(flags);
            this.javaRegex = javaRegex.ToString();
            try {
                pattern = Saxon.Ejava.util.regex.Pattern.compile(this.javaRegex, flagBits);
            } catch (Saxon.Ejava.util.regex.PatternSyntaxException e) {
                throw new Saxon.Hej.trans.XPathException(("Incorrect syntax for native regular expression: "+e.Message), "FORX0002");
            }
        }
        public string getJavaRegularExpression() {
            return javaRegex;
        }
        public int getFlagBits() {
            return flagBits;
        }
        public Saxon.Hej.regex.RegexIterator analyze(Saxon.Hej.str.UnicodeString input) {
            return new Saxon.Hej.regex.JRegexIterator(input.ToString(), pattern);
        }
        public bool containsMatch(Saxon.Hej.str.UnicodeString input) {
            return pattern.matcher(input.ToString()).find();
        }
        public bool matches(Saxon.Hej.str.UnicodeString input) {
            return pattern.matcher(input.ToString()).matches();
        }
        public Saxon.Hej.str.UnicodeString replace(Saxon.Hej.str.UnicodeString input, Saxon.Hej.str.UnicodeString replacement) {
            Saxon.Ejava.util.regex.Matcher matcher = pattern.matcher(input.ToString());
            try {
                return Saxon.Hej.str.StringView.tidy(matcher.replaceAll(replacement.ToString()));
            } catch (System.IndexOutOfRangeException e) {
                throw new Saxon.Hej.trans.XPathException(e.Message, "FORX0004");
            }
        }
        public Saxon.Hej.str.UnicodeString replaceWith(Saxon.Hej.str.UnicodeString input, System.Func<Saxon.Hej.str.UnicodeString,Saxon.Hej.str.UnicodeString> replacement) {
            throw new Saxon.Hej.trans.XPathException("saxon:replace-with() is not supported with the Java regex engine");
        }
        public Saxon.Hej.tree.iter.AtomicIterator tokenize(Saxon.Hej.str.UnicodeString input) {
            if (input.isEmpty()) {
                return Saxon.Hej.tree.iter.EmptyIterator.ofAtomic();
            }
            return new Saxon.Hej.regex.JTokenIterator(input.ToString(), pattern);
        }
        public static int setFlags(string inFlags) {
            int flags = Saxon.Ejava.util.regex.Pattern.UNIX_LINES;
            for (int i = 0; (i<inFlags.Length); i++) {
                char c = inFlags[i];
                switch (c) {
                  case 'd':
                    flags |= Saxon.Ejava.util.regex.Pattern.UNIX_LINES;
                    break ;
                  case 'm':
                    flags |= Saxon.Ejava.util.regex.Pattern.MULTILINE;
                    break ;
                  case 'i':
                    flags |= Saxon.Ejava.util.regex.Pattern.CASE_INSENSITIVE;
                    break ;
                  case 's':
                    flags |= Saxon.Ejava.util.regex.Pattern.DOTALL;
                    break ;
                  case 'x':
                    flags |= Saxon.Ejava.util.regex.Pattern.COMMENTS;
                    break ;
                  case 'u':
                    flags |= Saxon.Ejava.util.regex.Pattern.UNICODE_CASE;
                    break ;
                  case 'q':
                    flags |= Saxon.Ejava.util.regex.Pattern.LITERAL;
                    break ;
                  case 'c':
                    flags |= Saxon.Ejava.util.regex.Pattern.CANON_EQ;
                    break ;
                  default:
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Invalid character '"+c)+"' in regular expression flags"));
                    err.setErrorCode("FORX0001");
                    throw err;
                }
            }
            return flags;
        }
        public string getFlags() {
            string flags = "";
            if ((((flagBits&Saxon.Ejava.util.regex.Pattern.UNIX_LINES)) != 0)) {
                flags += 'd';
            }
            if ((((flagBits&Saxon.Ejava.util.regex.Pattern.MULTILINE)) != 0)) {
                flags += 'm';
            }
            if ((((flagBits&Saxon.Ejava.util.regex.Pattern.CASE_INSENSITIVE)) != 0)) {
                flags += 'i';
            }
            if ((((flagBits&Saxon.Ejava.util.regex.Pattern.DOTALL)) != 0)) {
                flags += 's';
            }
            if ((((flagBits&Saxon.Ejava.util.regex.Pattern.COMMENTS)) != 0)) {
                flags += 'x';
            }
            if ((((flagBits&Saxon.Ejava.util.regex.Pattern.UNICODE_CASE)) != 0)) {
                flags += 'u';
            }
            if ((((flagBits&Saxon.Ejava.util.regex.Pattern.LITERAL)) != 0)) {
                flags += 'q';
            }
            if ((((flagBits&Saxon.Ejava.util.regex.Pattern.CANON_EQ)) != 0)) {
                flags += 'c';
            }
            return flags;
        }
        public bool isPlatformNative() {
            return true;
        }
    
    }
}