using System.Xml;
using HtmlAgilityPack;
using Saxon.Api;
using Saxon.Hej.om;
using Saxon.Hej.tree.iter;
using Saxon.Hej.tree.util;
using Saxon.Hej.tree.wrapper;

namespace Saxon.Impl.Dom {
    /**
     * Wraps an HTML DOM tree produced by HTMLAgilityPack. This object represents the document node.
     */
    public class HtmlDocumentWrapper : AbstractNodeWrapper, HtmlContainerNodeWrapper, SteppingNode, SiblingCountingNode {
        
        

        private readonly HtmlDocument htmlDocument;
        private readonly HtmlNodeWrapper outermostElement;
        

        public HtmlDocumentWrapper(HtmlDocument doc, string baseURI, Saxon.Hej.Configuration config) {
            htmlDocument = doc;
            treeInfo = new GenericTreeInfo(config);
            ((GenericTreeInfo) treeInfo).setSystemId(baseURI);
            ((GenericTreeInfo) treeInfo).setRootNode(this);
            foreach (HtmlNode node in doc.DocumentNode.ChildNodes) {
                if (node.NodeType == HtmlNodeType.Element) {
                    outermostElement = HtmlNodeWrapper.makeWrapper(node, this, this, 0);
                    break;
                }
            }
        }

        public override object getUnderlyingNode() {
            return htmlDocument;
        }

        public override int getNodeKind() {
            return Saxon.Hej.type.Type.DOCUMENT;
        }

        public override bool Equals(object other) {
            return ((other is HtmlDocumentWrapper wrapper) && (htmlDocument == wrapper.htmlDocument));
        }

        public override int GetHashCode() {
            Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(64);
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

        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return outermostElement.getUnicodeStringValue();
        }

        public override string getLocalPart() {
            return "";
        }

        public override string getURI() {
            return "";
        }

        public override NamespaceUri getNamespaceUri() {
            return NamespaceUri.NULL;
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

        public override string getSystemId() {
            return ((GenericTreeInfo)getTreeInfo()).getSystemId();
        }

        public override string getBaseURI() {
            return ((GenericTreeInfo)getTreeInfo()).getSystemId();
        }

        protected internal override AxisIterator iterateAttributes(Saxon.Hej.pattern.NodeTest nodeTest) {
            return EmptyIterator.ofNodes();
        }

        protected internal override AxisIterator iterateChildren(Saxon.Hej.pattern.NodeTest nodeTest) {
            return SingleNodeIterator.makeIterator(outermostElement);
        }

        protected internal override AxisIterator iterateSiblings(Saxon.Hej.pattern.NodeTest nodeTest, bool forwards) {
            return EmptyIterator.ofNodes();
        }

        public override string getAttributeValue(string uri, string local) {
            return null;
        }

        public override bool hasChildNodes() {
            return true;
        }

        public override void generateId(Saxon.Ejava.lang.StringBuilder buffer) {
            Navigator.appendSequentialKey(this, buffer, true);
        }

        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            Saxon.Hej.@event.Receiver r = new Saxon.Hej.@event.NamespaceReducer(@out);
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

        public SteppingNode getSuccessorElement(SteppingNode anchor, NamespaceUri uri, string local) {
            return outermostElement;
        }


       
    }
}

// Copyright (c) 2021 Saxonica Limited