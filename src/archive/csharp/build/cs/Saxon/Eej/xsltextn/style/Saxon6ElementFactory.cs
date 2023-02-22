using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.style {
    public class Saxon6ElementFactory : Saxon.Eej.xsltextn.ExtensionElementFactory {
        public System.Type getExtensionClass(string localname) {
            if (localname.Equals("output")) {
                return typeof(Saxon.Hej.style.XSLResultDocument);
            }
            return null;
        }
    
    }
}