using System;
using System.Collections.Generic;
using System.Xml;
using HtmlAgilityPack;

namespace Saxon.Impl.Dom {
    /**
     * Wraps a .NET DOM tree. This is derived from the Java code in the Saxon .NET product,
     * but for Saxon-CS is maintained as native C# code.
     */
    public class DomDocumentTree : net.sf.saxon.om.GenericTreeInfo {
        public DomDocumentTree(XmlDocument doc, string baseURI, net.sf.saxon.Configuration config) : base (config) {
            setRootNode(wrap(doc));
            setSystemId(baseURI);
        }
        public DomNodeWrapper wrap(XmlNode node) {
            return DomNodeWrapper.makeWrapper(node, this);
        }
        public override net.sf.saxon.om.NodeInfo selectID(string id, bool getParent) {
            XmlNode node = (XmlNode)((DomNodeWrapper)getRootNode()).getUnderlyingNode();
            if ((node is XmlDocument document)) {
                XmlNode el = document.GetElementById(id);
                return el==null ? null : wrap(el);
            } else {
                return null;
            }
        }
    }
}