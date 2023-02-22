using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface NotationSet {
        bool isDeclaredNotation(Saxon.Hej.om.NamespaceUri uri, string local);
    
    }
}