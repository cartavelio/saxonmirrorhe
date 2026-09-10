
using System.Xml;
using Saxon.Api;
using Saxon.Hej.om;
using Saxon.Hej.pattern;
using Saxon.Hej.tree.iter;


namespace Saxon.Impl.Dom {
    /*
     * Wraps a .NET DOM tree. This is derived from the Java code in the Saxon .NET product,
     * but for SaxonCS is maintained as native C# code.
     */
    public class DomDocumentTree : Saxon.Hej.om.GenericTreeInfo {
        public DomDocumentTree(XmlDocument doc, string baseURI, Saxon.Hej.Configuration config) : base (config) {
            setRootNode(wrap(doc));
            setSystemId(baseURI);
        }
        public DomNodeWrapper wrap(XmlNode node) {
            return DomNodeWrapper.makeWrapper(node, this);
        }
        public override Saxon.Hej.om.NodeInfo selectID(string id, bool getParent) {
            XmlNode node = (XmlNode)((DomNodeWrapper)getRootNode()).getUnderlyingNode();
            if (node is XmlDocument document) {
                NodeInfo root = wrap(document);
                AxisIterator descendants = root.iterateAxis(AxisInfo.DESCENDANT, NodeKindTest.ELEMENT);
                for (NodeInfo n; (n = (NodeInfo)descendants.next()) != null;) {
                    if (id.Equals(n.getAttributeValue(NamespaceUri.XML, "id"))) {
                        return n;
                    }
                }
            }
            return null;
        }
    }
}

// Copyright (c) 2021 Saxonica Limited