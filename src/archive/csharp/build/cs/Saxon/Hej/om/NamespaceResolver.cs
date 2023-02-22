using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface NamespaceResolver {
        Saxon.Hej.om.NamespaceUri getURIForPrefix(string prefix, bool useDefault);
        System.Collections.Generic.IEnumerator<string> iteratePrefixes();
    
    }
}