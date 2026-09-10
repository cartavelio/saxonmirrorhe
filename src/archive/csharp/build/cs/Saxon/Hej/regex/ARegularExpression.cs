using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class ARegularExpression : Saxon.Hej.regex.RegularExpression {
        internal Saxon.Hej.str.UnicodeString rawPattern;
        internal string rawFlags;
        internal Saxon.Hej.regex.REProgram regex;
        public ARegularExpression(Saxon.Hej.str.UnicodeString pattern, string flags, string hostLanguage, System.Collections.Generic.IList<string> warnings, Saxon.Hej.Configuration config)  {
            rawFlags = flags;
            Saxon.Hej.regex.REFlags reFlags;
            try {
                reFlags = new Saxon.Hej.regex.REFlags(flags, hostLanguage);
            } catch (Saxon.Hej.regex.RESyntaxException err) {
                throw new Saxon.Hej.trans.XPathException(err.Message, "FORX0001");
            }
            try {
                rawPattern = pattern;
                Saxon.Hej.regex.RECompiler comp2 = new Saxon.Hej.regex.RECompiler();
                comp2.setFlags(reFlags);
                regex = comp2.compile(rawPattern);
                if ((warnings != null)) {
                    Saxon.Impl.Helpers.ListUtils.AddAll(warnings, comp2.getWarnings());
                }
                if ((config != null)) {
                    regex.setBacktrackingLimit(config.getConfigurationProperty(Saxon.Hej.lib.Feature<int>.REGEX_BACKTRACKING_LIMIT));
                }
            } catch (Saxon.Hej.regex.RESyntaxException err) {
                throw new Saxon.Hej.trans.XPathException(err.Message, "FORX0002");
            }
        }
        public static Saxon.Hej.regex.ARegularExpression compile(string pattern, string flags) {
            try {
                return new Saxon.Hej.regex.ARegularExpression(Saxon.Hej.str.BMPString.of(pattern), flags, "XP31", null, null);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Ejava.lang.IllegalArgumentException(e);
            }
        }
        public static Saxon.Hej.regex.ARegularExpression compile(Saxon.Hej.str.UnicodeString pattern, string flags) {
            try {
                return new Saxon.Hej.regex.ARegularExpression(pattern, flags, "XP31", null, null);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Ejava.lang.IllegalArgumentException(e);
            }
        }
        public bool matches(Saxon.Hej.str.UnicodeString input) {
            if ((input.isEmpty() && regex.isNullable())) {
                return true;
            }
            Saxon.Hej.regex.REMatcher matcher = new Saxon.Hej.regex.REMatcher(regex);
            return matcher.isAnchoredMatch(input.tidy());
        }
        public bool containsMatch(Saxon.Hej.str.UnicodeString input) {
            Saxon.Hej.regex.REMatcher matcher = new Saxon.Hej.regex.REMatcher(regex);
            return matcher.match(input.tidy(), 0);
        }
        public Saxon.Hej.tree.iter.AtomicIterator tokenize(Saxon.Hej.str.UnicodeString input) {
            return new Saxon.Hej.regex.ATokenIterator(input.tidy(), new Saxon.Hej.regex.REMatcher(regex));
        }
        public Saxon.Hej.regex.RegexIterator analyze(Saxon.Hej.str.UnicodeString input) {
            return new Saxon.Hej.regex.ARegexIterator(input.tidy(), rawPattern, new Saxon.Hej.regex.REMatcher(regex));
        }
        public Saxon.Hej.str.UnicodeString replace(Saxon.Hej.str.UnicodeString input, Saxon.Hej.str.UnicodeString replacement) {
            Saxon.Hej.regex.REMatcher matcher = new Saxon.Hej.regex.REMatcher(regex);
            try {
                return matcher.replace(input.tidy(), replacement);
            } catch (Saxon.Hej.regex.RESyntaxException err) {
                throw new Saxon.Hej.trans.XPathException(err.Message, "FORX0004");
            }
        }
        public Saxon.Hej.str.UnicodeString replaceWith(Saxon.Hej.str.UnicodeString input, System.Func<Saxon.Hej.str.UnicodeString,Saxon.Hej.str.UnicodeString> replacer) {
            Saxon.Hej.regex.REMatcher matcher = new Saxon.Hej.regex.REMatcher(regex);
            try {
                return matcher.replaceWith(input.tidy(), replacer);
            } catch (Saxon.Hej.regex.RESyntaxException err) {
                throw new Saxon.Hej.trans.XPathException(err.Message, "FORX0004");
            }
        }
        public string getFlags() {
            return rawFlags;
        }
    
    }
}