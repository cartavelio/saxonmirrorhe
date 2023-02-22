using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface SubstringMatcher : Saxon.Hej.lib.StringCollator {
        bool contains(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2);
        bool startsWith(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2);
        bool endsWith(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2);
        Saxon.Hej.str.UnicodeString substringBefore(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2);
        Saxon.Hej.str.UnicodeString substringAfter(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2);
    
    }
}