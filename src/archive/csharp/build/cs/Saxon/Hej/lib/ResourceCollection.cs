using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface ResourceCollection {
        string getCollectionURI();
        System.Collections.Generic.IEnumerator<string> getResourceURIs(Saxon.Hej.expr.XPathContext context);
        System.Collections.Generic.IEnumerator<Saxon.Hej.lib.Resource> getResources(Saxon.Hej.expr.XPathContext context);
        bool isStable(Saxon.Hej.expr.XPathContext context);
    
    }
}