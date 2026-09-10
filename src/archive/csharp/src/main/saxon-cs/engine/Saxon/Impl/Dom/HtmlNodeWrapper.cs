using System.Collections.Generic;
using AngleSharp.Dom;
using Saxon.Api;
using Saxon.Ejava.lang;
using Saxon.Hej.om;
using Saxon.Hej.tree.iter;
using Saxon.Hej.tree.util;
using Saxon.Hej.tree.wrapper;
using Saxon.Hej.type;

namespace Saxon.Impl.Dom {
    
    
    /// <summary>
    /// Implements Saxon.Hej.om.NodeInfo as a wrapper over an element, text, or comment node in
    /// the AngleSharp HTML document tree. The node always has a parent (which may be an element
    /// or document node), and is always part of a tree rooted at a document node.
    /// There are no namespaces, and names comprise a local name only. There are no
    /// processing instructions. It is assumed that there are no adjacent text nodes (if this
    /// is not the case, we make no attempt to merge them).
    /// </summary>
    public class HtmlNodeWrapper : AbstractNodeWrapper, HtmlContainerNodeWrapper, SteppingNode, SiblingCountingNode {
        private readonly AngleSharp.Dom.INode node;
        private short nodeKind;
        private HtmlContainerNodeWrapper parent;
        private HtmlDocumentWrapper docWrapper;
        private int index;
        
        public static NamespaceMap XhtmlNamespaceMap;

        static HtmlNodeWrapper() {
            XhtmlNamespaceMap = new NamespaceMap().put("", NamespaceUri.XHTML);
        }

        
        private HtmlNodeWrapper(AngleSharp.Dom.INode node, HtmlContainerNodeWrapper parent, int index) {
            this.node = node;
            this.parent = parent;
            this.index = index;
        }

        protected internal static HtmlNodeWrapper makeWrapper(AngleSharp.Dom.INode node, HtmlDocumentWrapper docWrapper) {
            if ((node == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("HtmlNodeWrapper#makeWrapper: Node must not be null");
            }

            if ((docWrapper == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("HtmlNodeWrapper#makeWrapper: DocumentWrapper must not be null");
            }

            return makeWrapper(node, docWrapper, null, -1);
        }

        protected internal static HtmlNodeWrapper makeWrapper(
            AngleSharp.Dom.INode node, HtmlDocumentWrapper docWrapper,
                                            HtmlContainerNodeWrapper parent, int index) {
            HtmlNodeWrapper wrapper;
            switch (node.NodeType) {
                case AngleSharp.Dom.NodeType.Element:
                    wrapper = new HtmlNodeWrapper(node, parent, index);
                    wrapper.nodeKind = Saxon.Hej.type.Type.ELEMENT;
                    break;
                case AngleSharp.Dom.NodeType.Text:
                    wrapper = new HtmlNodeWrapper(node, parent, index);
                    wrapper.nodeKind = Saxon.Hej.type.Type.TEXT;
                    break;
                case AngleSharp.Dom.NodeType.Comment:
                    wrapper = new HtmlNodeWrapper(node, parent, index);
                    wrapper.nodeKind = Saxon.Hej.type.Type.COMMENT;
                    break;
                default:
                    throw new AssertionError();
            }

            wrapper.docWrapper = docWrapper;
            wrapper.treeInfo = docWrapper.treeInfo;
            return wrapper;
        }

        public override object getUnderlyingNode() {
            return node;
        }

        public override int getNodeKind() {
            return nodeKind;
        }

        public override bool Equals(object other) {
            return ((other is HtmlNodeWrapper wrapper) && (node == wrapper.node));
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
            return node.TextContent;
        }

        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return Saxon.Hej.str.StringView.tidy(node.TextContent);
        }

        public override string getLocalPart() {
            if (node.NodeType == NodeType.Element) {
                return ((IElement)node).LocalName.ToLower();
            }
            else {
                return "";
            }
        }

        
        public override Saxon.Hej.om.NamespaceUri getNamespaceUri() {
            return Saxon.Hej.om.NamespaceUri.of(getURI());
        }

        public override string getURI() {
            if (node.NodeType == NodeType.Element) {
                return ((IElement)node).NamespaceUri;
            }
            else {
                return "";
            }
        }

        public override string getPrefix() {
            return "";
        }

        public override string getDisplayName() {
            return nodeKind == Saxon.Hej.type.Type.ELEMENT ? getLocalPart() : "";
        }

        public override NodeInfo getParent() {
            if ((parent == null)) {
                AngleSharp.Dom.INode p = node.Parent;
                if ((p == null)) {
                    return parent = docWrapper;
                }
                else {
                    return parent = makeWrapper(p, docWrapper);
                }
            }
            return parent;
        }

        public override string getSystemId() {
            return ((GenericTreeInfo)getTreeInfo()).getSystemId();
        }

        public int getSiblingPosition() {
            if ((index == -1)) {
                int ix = 0;
                AngleSharp.Dom.INode start = node;
                while (true) {
                    start = start.PreviousSibling;
                    if ((start == null)) {
                        return index = ix;
                    }
                    ix++;
                }
            }

            return index;
        }

        protected internal override AxisIterator iterateAttributes(Saxon.Hej.pattern.NodeTest nodeTest) {
            List<NodeInfo> attList = new List<NodeInfo>();
            int indexPosition = 0;
            foreach (AngleSharp.Dom.IAttr att in ((AngleSharp.Dom.IElement)node).Attributes) {
                if (att.Name != "xmlns" && !att.Name.StartsWith("xmlns:")) {
                    HtmlAttributeWrapper wrapper = HtmlAttributeWrapper.makeWrapper(att, this, indexPosition++);
                    if (nodeTest.test(wrapper)) {
                        attList.Add(HtmlAttributeWrapper.makeWrapper(att, this, indexPosition++));
                    }
                }
            }
            return new NodeListIterator(attList);
        }

        protected internal override AxisIterator iterateChildren(Saxon.Hej.pattern.NodeTest nodeTest) {
            if (!node.HasChildNodes) {
                return EmptyIterator.ofNodes();
            }
            List<NodeInfo> childList = new List<NodeInfo>();
            int indexPosition = 0;
            foreach (AngleSharp.Dom.INode child in node.ChildNodes) {
                HtmlNodeWrapper wrapper = makeWrapper(child, docWrapper, this, indexPosition++);
                if (nodeTest.test(wrapper)) {
                    childList.Add(wrapper);
                }
            }

            return new NodeListIterator(childList);
        }

        protected internal override AxisIterator iterateSiblings(Saxon.Hej.pattern.NodeTest nodeTest, bool forwards) {
            List<NodeInfo> siblingList = new List<NodeInfo>();
            int ix = getSiblingPosition();
            AngleSharp.Dom.INode next = node;
            while (true) {
                next = forwards ? next.NextSibling : next.PreviousSibling;
                if (next == null) {
                    return new NodeListIterator(siblingList);
                }
                if (next.NodeType == AngleSharp.Dom.NodeType.DocumentType) {
                    continue;
                }
                HtmlNodeWrapper wrapper = makeWrapper(next, docWrapper, parent, ix++);
                if (nodeTest.test(wrapper)) {
                    siblingList.Add(wrapper);
                }
            }
        }

        public override string getAttributeValue(string uri, string local) {
            return ((AngleSharp.Dom.Element)node).GetAttribute(local);
        }

        public override bool hasChildNodes() {
            return node.HasChildNodes;
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

        public override NamespaceMap getAllNamespaces() {
            return XhtmlNamespaceMap;
        }

        public SteppingNode getNextSibling() {
            AngleSharp.Dom.INode currNode = node.NextSibling;
            if ((currNode != null)) {
                return makeWrapper(currNode, docWrapper, parent, getSiblingPosition()+1);
            }

            return null;
        }

        public SteppingNode getFirstChild() {
            AngleSharp.Dom.INode currNode = node.FirstChild;
            if ((currNode != null)) {
                return makeWrapper(currNode, docWrapper, this, 0);
            }

            return null;
        }

        public SteppingNode getPreviousSibling() {
            AngleSharp.Dom.INode currNode = node.PreviousSibling;
            if ((currNode != null)) {
                return makeWrapper(currNode, docWrapper, parent, getSiblingPosition()-1);
            }

            return null;
        }

        public SteppingNode getSuccessorElement(SteppingNode anchor, NamespaceUri uri, string local) {
            AngleSharp.Dom.INode stop = (((anchor == null) ? null : ((HtmlNodeWrapper) anchor).node));
            AngleSharp.Dom.INode next = node;
            while (true) {
                next = getSuccessorNode(next, stop);
                if (next == null) {
                    return null;
                }

                if (next.NodeType != AngleSharp.Dom.NodeType.Element) {
                    continue;
                }

                if (next.NodeName == local) {
                    return makeWrapper(next, docWrapper);
                }
            }
        }

        private static AngleSharp.Dom.INode getSuccessorNode(AngleSharp.Dom.INode start, AngleSharp.Dom.INode anchor) {
            if (start.HasChildNodes) {
                return start.FirstChild;
            }

            AngleSharp.Dom.INode p = start;
            while (true) {
                AngleSharp.Dom.INode s = p.NextSibling;
                if ((s != null)) {
                    return s;
                }

                p = p.Parent;
                if (((p == null) || (((anchor != null) && (p == anchor))))) {
                    return null;
                }
            }
        }

       
    }
    
}

// Copyright (c) 2021 Saxonica Limited