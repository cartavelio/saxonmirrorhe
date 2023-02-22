using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface StringCollator {
        string getCollationURI();
        int compareStrings(Saxon.Hej.str.UnicodeString o1, Saxon.Hej.str.UnicodeString o2);
        bool comparesEqual(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2);
        bool isEqualToEmpty(Saxon.Hej.str.UnicodeString s1);
        Saxon.Hej.expr.sort.AtomicMatchKey getCollationKey(Saxon.Hej.str.UnicodeString s);
    
    }
}