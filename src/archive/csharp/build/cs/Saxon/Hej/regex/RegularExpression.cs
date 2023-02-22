using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public interface RegularExpression {
        bool matches(Saxon.Hej.str.UnicodeString input);
        bool containsMatch(Saxon.Hej.str.UnicodeString input);
        Saxon.Hej.tree.iter.AtomicIterator tokenize(Saxon.Hej.str.UnicodeString input);
        Saxon.Hej.regex.RegexIterator analyze(Saxon.Hej.str.UnicodeString input);
        Saxon.Hej.str.UnicodeString replace(Saxon.Hej.str.UnicodeString input, Saxon.Hej.str.UnicodeString replacement);
        Saxon.Hej.str.UnicodeString replaceWith(Saxon.Hej.str.UnicodeString input, System.Func<Saxon.Hej.str.UnicodeString,Saxon.Hej.str.UnicodeString> replacement);
        string getFlags();
        bool isPlatformNative();
    
    }
}