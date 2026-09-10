using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface SchemaURIResolver {
        void setConfiguration(Saxon.Hej.Configuration config);
        Saxon.Ejavax.xml.transform.Source[] resolve(string targetNamespace, string baseURI, string[] locations);
    
    }
}