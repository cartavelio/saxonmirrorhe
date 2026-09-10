using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface Resource {
        string getResourceURI();
        Saxon.Hej.om.Item getItem(Saxon.Hej.expr.XPathContext context);
        string getContentType();
    
    }
}