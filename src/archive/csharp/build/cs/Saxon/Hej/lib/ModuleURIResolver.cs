using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface ModuleURIResolver {
        Saxon.Ejavax.xml.transform.stream.StreamSource[] resolve(string moduleURI, string baseURI, string[] locations);
    
    }
}