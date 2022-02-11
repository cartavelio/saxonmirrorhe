using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface NamespaceResolver {
        string getURIForPrefix(string prefix, bool useDefault);
        System.Collections.Generic.IEnumerator<string> iteratePrefixes();
    
    }
}