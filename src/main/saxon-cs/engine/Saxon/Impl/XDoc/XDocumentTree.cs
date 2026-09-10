using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Saxon.Api.Support;
using Saxon.Impl.Dom;

namespace Saxon.Impl.XDoc {
    public class XDocumentTree : Saxon.Hej.om.GenericTreeInfo {
        public XDocumentTree(XDocument doc, string baseURI, Saxon.Hej.Configuration config) : base (config) {
            setRootNode(wrap(doc));
            setSystemId(baseURI);
        }
        public XNodeWrapper wrap(XNode node) {
            return XNodeWrapper.makeWrapper(node, this);
        }
        public override Saxon.Hej.om.NodeInfo selectID(string id, bool getParent) {
            XNode node = (XNode)((XNodeWrapper)getRootNode()).getUnderlyingNode();
            if ((node is XDocument document)) {
                XName xmlId = XNamespace.Xml + "id";
                IEnumerator<XElement> allElements = document.Descendants().GetEnumerator();
                FilterEnumerator<XElement> selected =
                    new(allElements, e => id.Equals(e.Attribute(xmlId)?.Value));
                if (selected.MoveNext()) {
                    return wrap(selected.Current);
                }
            }
            return null;
        }
    }
}