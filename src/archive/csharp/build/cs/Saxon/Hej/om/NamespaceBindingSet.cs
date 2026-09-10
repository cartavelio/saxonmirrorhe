using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface NamespaceBindingSet : System.Collections.Generic.IEnumerable<Saxon.Hej.om.NamespaceBinding> {
        string getURI(string prefix);
    
    }
}