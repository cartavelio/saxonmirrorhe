using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize.charcode {
    public interface CharacterSet {
        bool inCharset(int ch);
        string getCanonicalName();
    
    }
}