using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public interface RegexMatchHandler {
        void characters(Saxon.Hej.str.UnicodeString s);
        void onGroupStart(int groupNumber);
        void onGroupEnd(int groupNumber);
    
    }
}