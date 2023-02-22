using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface JJ_ResourceResolver {
        Saxon.Ejavax.xml.transform.Source resolve(Saxon.Hej.lib.JJ_ResourceRequest request);
    
    }
}