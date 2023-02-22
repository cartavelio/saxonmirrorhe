using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.sxpath {
    public interface XPathStaticContext : Saxon.Hej.expr.StaticContext {
        void setDefaultElementNamespace(Saxon.Hej.om.NamespaceUri uri);
        void setNamespaceResolver(Saxon.Hej.om.NamespaceResolver resolver);
        Saxon.Hej.sxpath.XPathVariable declareVariable(Saxon.Hej.value.QNameValue qname);
        Saxon.Hej.sxpath.XPathVariable declareVariable(Saxon.Hej.om.NamespaceUri namespaceURI, string localName);
        Saxon.Hej.expr.instruct.SlotManager getStackFrameMap();
        bool isContextItemParentless();
    
    }
}