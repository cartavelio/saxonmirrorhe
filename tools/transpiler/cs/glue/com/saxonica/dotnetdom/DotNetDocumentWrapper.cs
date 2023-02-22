using System;
using System.Collections.Generic;
using System.Xml;

namespace com.saxonica.dotnetdom {
    /**
     * Wraps a .NET DOM tree. This is derived from the Java code in the Saxon .NET product,
     * but for Saxon-CS is maintained as native C# code.
     */
    public class DotNetDocumentWrapper : net.sf.saxon.om.GenericTreeInfo {
        public DotNetDocumentWrapper(XmlNode doc, string baseURI, net.sf.saxon.Configuration config) : base (config) {
            if ((doc.NodeType!=XmlNodeType.Document)) {
                throw new java.lang.IllegalArgumentException("Node must be a DOM Document");
            }
            setRootNode(wrap(doc));
            setSystemId(baseURI);
        }
        public DotNetNodeWrapper wrap(XmlNode node) {
            return DotNetNodeWrapper.makeWrapper(node, this);
        }
        public override net.sf.saxon.om.NodeInfo selectID(string id, bool getParent) {
            XmlNode node = (XmlNode)((DotNetNodeWrapper)getRootNode()).getUnderlyingNode();
            if ((node is XmlDocument)) {
                XmlNode el = ((XmlDocument)node).GetElementById(id);
                if ((el==null)) {
                    return null;
                }
                return wrap(el);
            } else {
                return null;
            }
        }
    }
}