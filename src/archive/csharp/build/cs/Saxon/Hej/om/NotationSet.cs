using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface NotationSet {
        bool isDeclaredNotation(string uri, string local);
    
    }
}