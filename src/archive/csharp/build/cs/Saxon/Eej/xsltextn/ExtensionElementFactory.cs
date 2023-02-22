using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn {
    public interface ExtensionElementFactory {
        System.Type getExtensionClass(string localname);
    
    }
}