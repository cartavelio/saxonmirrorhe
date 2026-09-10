using System.Xml;
using HtmlAgilityPack;
using net.sf.saxon.om;
using net.sf.saxon.tree.iter;
using net.sf.saxon.tree.util;
using net.sf.saxon.tree.wrapper;
using SaxonCS3.Impl.Dom;

namespace Saxon.Impl.Dom {
    /**
     * Wraps an HTML DOM tree produced by HTMLAgilityPack. This object represents the document node.
     */
    public class HtmlDocumentWrapper : AbstractNodeWrapper, HtmlContainerNodeWrapper, SteppingNode, SiblingCountingNode {

        private readonly HtmlDocument htmlDocument;
        private readonly HtmlNodeWrapper outermostElement;
        public HtmlDocumentWrapper(HtmlDocument doc, string baseURI, net.sf.saxon.Configuration config) {
            htmlDocument = doc;
            setSystemId(baseURI);
            treeInfo = new GenericTreeInfo(config);
            outermostElement = HtmlNodeWrapper.makeWrapper(htmlDocument.DocumentNode, this, this, 0);
        }

        public override object getUnderlyingNode() {
            return htmlDocument;
        }

        public override int getNodeKind() {
            return net.sf.saxon.type.Type.DOCUMENT;
        }

        public override bool Equals(object other) {
            return ((other is HtmlDocumentWrapper wrapper) && (htmlDocument == wrapper.htmlDocument));
        }

        public override int GetHashCode() {
            java.lang.StringBuilder buffer = new java.lang.StringBuilder(64);
            generateId(buffer);
            return buffer.ToString().GetHashCode();
        }

        public override int compareOrder(NodeInfo other) {
            if ((other is SiblingCountingNode countingNode)) {
                return Navigator.compareOrder(this, countingNode);
            }
            else {
                return -(other.compareOrder(this));
            }
        }
        
        public override string getStringValue() {
            return outermostElement.getStringValue();
        }

        public override net.sf.saxon.str.UnicodeString getUnicodeStringValue() {
            return outermostElement.getUnicodeStringValue();
        }

        public override string getLocalPart() {
            return "";
        }

        public override string getURI() {
            return "";
        }

        public override string getPrefix() {
            return "";
        }

        public override string getDisplayName() {
            return "";
        }

        public override NodeInfo getParent() {
            return null;
        }

        public int getSiblingPosition() {
            return 0;
        }

        protected internal override AxisIterator iterateAttributes(net.sf.saxon.pattern.NodeTest nodeTest) {
            return EmptyIterator.ofNodes();
        }

        protected internal override AxisIterator iterateChildren(net.sf.saxon.pattern.NodeTest nodeTest) {
            return SingleNodeIterator.makeIterator(outermostElement);
        }

        protected internal override AxisIterator iterateSiblings(net.sf.saxon.pattern.NodeTest nodeTest, bool forwards) {
            return EmptyIterator.ofNodes();
        }

        public override string getAttributeValue(string uri, string local) {
            return null;
        }

        public override bool hasChildNodes() {
            return true;
        }

        public override void generateId(java.lang.StringBuilder buffer) {
            Navigator.appendSequentialKey(this, buffer, true);
        }

        public override void copy(net.sf.saxon.@event.Receiver @out, int copyOptions, net.sf.saxon.s9api.Location locationId) {
            net.sf.saxon.@event.Receiver r = new net.sf.saxon.@event.NamespaceReducer(@out);
            Navigator.copy(this, r, copyOptions, locationId);
        }

        public override NamespaceBinding[] getDeclaredNamespaces(NamespaceBinding[] buffer) {
            return new NamespaceBinding[0];
        }

        public SteppingNode getNextSibling() {
            return null;
        }

        public SteppingNode getFirstChild() {
            return outermostElement;
        }

        public SteppingNode getPreviousSibling() {
            return null;
        }

        public SteppingNode getSuccessorElement(SteppingNode anchor, string uri, string local) {
            return outermostElement;
        }


       
    }
}