using System.Xml;
using System.Xml.Linq;
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
            // TODO: recognise xml:id attributes
            return null;
        }
    }
}