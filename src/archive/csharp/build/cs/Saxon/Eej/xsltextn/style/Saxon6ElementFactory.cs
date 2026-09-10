using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.style {
    public abstract class Saxon6ElementFactory {
        public readonly static Saxon.Eej.xsltextn.ExtensionElementCreator FACTORY = (localName => {
            if (localName.Equals("output")) {
                return new Saxon.Hej.style.XSLResultDocument();
            }
            return null;
        } );
    
    }
}