using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public interface RegexIterator : Saxon.Hej.om.SequenceIterator {
        /* implicit =====
        Saxon.Hej.om.Item next();
        ===== implicit */
        bool isMatching();
        int getNumberOfGroups();
        Saxon.Hej.str.UnicodeString getRegexGroup(int number);
        void processMatchingSubstring(Saxon.Hej.regex.RegexMatchHandler action);
    
    }
}