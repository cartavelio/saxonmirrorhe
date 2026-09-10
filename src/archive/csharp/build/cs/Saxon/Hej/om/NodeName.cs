using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface NodeName : Saxon.Hej.om.IdentityComparable {
        string getPrefix();
        string getURI();
        string getLocalPart();
        string getDisplayName();
        Saxon.Hej.om.StructuredQName getStructuredQName();
        bool hasURI(string ns);
        Saxon.Hej.om.NamespaceBinding getNamespaceBinding();
        bool hasFingerprint();
        int getFingerprint();
        int obtainFingerprint(Saxon.Hej.om.NamePool namePool);
    
    }
}