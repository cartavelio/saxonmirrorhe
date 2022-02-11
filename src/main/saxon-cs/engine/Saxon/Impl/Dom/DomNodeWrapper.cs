using System;
using System.Collections.Generic;
using SysCollGen = System.Collections.Generic;
using System.Xml;
using java.util;
using net.sf.saxon.dotnet;
using net.sf.saxon.om;
using net.sf.saxon.tree.iter;
using net.sf.saxon.tree.util;
using net.sf.saxon.tree.wrapper;

namespace Saxon.Impl.Dom {
    public class DomNodeWrapper : AbstractNodeWrapper, SteppingNode, SiblingCountingNode {
        private XmlNode node;
        private short nodeKind;
        private DomNodeWrapper parent;
        protected DomDocumentWrapper docWrapper;
        private int index;
        
        // the number of adjacent text nodes wrapped by this NodeWrapper.
        // If span>1, node will always be the first of a sequence of adjacent text nodes
        protected int span = 1;

        private DomNodeWrapper(XmlNode node, DomNodeWrapper parent, int index) {
            this.node = node;
            this.parent = parent;
            this.index = index;
        }

        internal DomNodeWrapper() {
        }

        protected internal static DomNodeWrapper makeWrapper(XmlNode node, DomDocumentWrapper docWrapper) {
            if ((node == null)) {
                throw new java.lang.NullPointerException("NodeWrapper#makeWrapper: Node must not be null");
            }

            if ((docWrapper == null)) {
                throw new java.lang.NullPointerException("NodeWrapper#makeWrapper: DocumentWrapper must not be null");
            }

            return makeWrapper(node, docWrapper, null, -1);
        }

        protected internal static DomNodeWrapper makeWrapper(XmlNode node, DomDocumentWrapper docWrapper,
            DomNodeWrapper parent, int index) {
            DomNodeWrapper wrapper;
            switch (node.NodeType) {
                case XmlNodeType.Document:
                    wrapper = (DomNodeWrapper) docWrapper.getRootNode();
                    if ((wrapper == null)) {
                        wrapper = new DomNodeWrapper(node, parent, index);
                        wrapper.nodeKind = net.sf.saxon.type.Type.DOCUMENT;
                    }

                    break;
                case XmlNodeType.Element:
                    wrapper = new DomNodeWrapper(node, parent, index);
                    wrapper.nodeKind = net.sf.saxon.type.Type.ELEMENT;
                    break;
                case XmlNodeType.Attribute:
                    wrapper = new DomNodeWrapper(node, parent, index);
                    wrapper.nodeKind = net.sf.saxon.type.Type.ATTRIBUTE;
                    break;
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    wrapper = new DomNodeWrapper(node, parent, index);
                    wrapper.nodeKind = net.sf.saxon.type.Type.TEXT;
                    break;
                case XmlNodeType.Comment:
                    wrapper = new DomNodeWrapper(node, parent, index);
                    wrapper.nodeKind = net.sf.saxon.type.Type.COMMENT;
                    break;
                case XmlNodeType.ProcessingInstruction:
                    wrapper = new DomNodeWrapper(node, parent, index);
                    wrapper.nodeKind = net.sf.saxon.type.Type.PROCESSING_INSTRUCTION;
                    break;
                default:
                    throw new java.lang.IllegalArgumentException(
                        ("Unsupported node type in DOM! " + node.NodeType + " instance " + node.ToString()));
            }

            wrapper.docWrapper = docWrapper;
            wrapper.treeInfo = docWrapper;
            return wrapper;
        }

        public override object getUnderlyingNode() {
            return node;
        }

        public override int getNodeKind() {
            return nodeKind;
        }

        public override bool Equals(object other) {
            return ((other is DomNodeWrapper wrapper) && (node == wrapper.node));
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

        public override net.sf.saxon.str.UnicodeString getUnicodeStringValue() {
            switch (nodeKind) {
                case net.sf.saxon.type.Type.DOCUMENT:
                case net.sf.saxon.type.Type.ELEMENT:
                    return net.sf.saxon.str.StringView.tidy(node.InnerText);
                case net.sf.saxon.type.Type.ATTRIBUTE:
                    return net.sf.saxon.str.StringView.tidy(node.Value);
                case net.sf.saxon.type.Type.TEXT:
                    if ((span == 1)) {
                        return net.sf.saxon.str.StringView.tidy(node.InnerText);
                    }
                    else {
                        net.sf.saxon.str.UnicodeBuilder fsb = new net.sf.saxon.str.UnicodeBuilder();
                        XmlNode textNode = node;
                        for (int i = 0; (i < span); i++) {
                            fsb.append(textNode.InnerText);
                            textNode = textNode.NextSibling;
                        }

                        return fsb.toUnicodeString();
                    }
                case net.sf.saxon.type.Type.COMMENT:
                case net.sf.saxon.type.Type.PROCESSING_INSTRUCTION:
                    return net.sf.saxon.str.StringView.tidy(node.Value);
                default:
                    return net.sf.saxon.str.EmptyUnicodeString.getInstance();
            }
        }

        public override string getLocalPart() {
            return node.LocalName;
        }

        public override string getURI() {
            NodeInfo element;
            if ((nodeKind == net.sf.saxon.type.Type.ELEMENT)) {
                element = this;
            }
            else {
                if ((nodeKind == net.sf.saxon.type.Type.ATTRIBUTE)) {
                    element = parent;
                }
                else {
                    return "";
                }
            }

            string uri = node.NamespaceURI;
            if ((uri.Length > 0)) {
                return uri;
            }

            if (node.Name.StartsWith("xml:")) {
                return net.sf.saxon.lib.NamespaceConstant.XML;
            }

            string[] parts;
            try {
                parts = NameChecker.getQNameParts(node.Name);
            }
            catch (QNameException e) {
                throw new java.lang.IllegalStateException(("Invalid QName in DOM node. " + e));
            }

            if (((nodeKind == net.sf.saxon.type.Type.ATTRIBUTE) && (parts[0].Length == 0))) {
                uri = "";
            }
            else {
                AxisIterator nsIter = element.iterateAxis(AxisInfo.NAMESPACE);
                NodeInfo ns;
                while (((ns = ((NodeInfo) nsIter.next())) != null)) {
                    if (ns.getLocalPart().Equals(parts[0])) {
                        uri = ns.getStringValue();
                        break;
                    }
                }

                if ((uri == null)) {
                    if ((parts[0].Length == 0)) {
                        uri = "";
                    }
                    else {
                        throw new java.lang.IllegalStateException(("Undeclared namespace prefix in DOM input: " +
                                                                   parts[0]));
                    }
                }
            }

            return uri;
        }

        public override string getPrefix() {
            return node.Prefix;
        }

        public override string getDisplayName() {
            switch (nodeKind) {
                case net.sf.saxon.type.Type.ELEMENT:
                case net.sf.saxon.type.Type.ATTRIBUTE:
                case net.sf.saxon.type.Type.PROCESSING_INSTRUCTION:
                    return node.Name;
                default:
                    return "";
            }
        }

        public override NodeInfo getParent() {
            if ((parent == null)) {
                switch (getNodeKind()) {
                    case net.sf.saxon.type.Type.ATTRIBUTE:
                        parent = makeWrapper(((XmlAttribute) node).OwnerElement, docWrapper);
                        break;
                    default:
                        XmlNode p = node.ParentNode;
                        if ((p == null)) {
                            return null;
                        }
                        else {
                            parent = makeWrapper(p, docWrapper);
                        }

                        break;
                }
            }

            return parent;
        }

        public int getSiblingPosition() {
            if ((index == -1)) {
                switch (nodeKind) {
                    case net.sf.saxon.type.Type.ELEMENT:
                    case net.sf.saxon.type.Type.TEXT:
                    case net.sf.saxon.type.Type.COMMENT:
                    case net.sf.saxon.type.Type.PROCESSING_INSTRUCTION:
                        int ix = 0;
                        XmlNode start = node;
                        while (true) {
                            start = start.PreviousSibling;
                            if ((start == null)) {
                                index = ix;
                                return ix;
                            }

                            ix++;
                        }
                    case net.sf.saxon.type.Type.ATTRIBUTE:
                        ix = 0;
                        AxisIterator iter = parent.iterateAxis(AxisInfo.ATTRIBUTE);
                        while (true) {
                            NodeInfo n = ((NodeInfo) iter.next());
                            if (((n == null) || Navigator.haveSameName(this, n))) {
                                index = ix;
                                return ix;
                            }

                            ix++;
                        }
                    case net.sf.saxon.type.Type.NAMESPACE:
                        ix = 0;
                        iter = parent.iterateAxis(AxisInfo.NAMESPACE);
                        while (true) {
                            NodeInfo n = ((NodeInfo) iter.next());
                            if (((n == null) || Navigator.haveSameName(this, n))) {
                                index = ix;
                                return ix;
                            }

                            ix++;
                        }
                    default:
                        index = 0;
                        return index;
                }
            }

            return index;
        }

        protected internal override AxisIterator iterateAttributes(net.sf.saxon.pattern.NodeTest nodeTest) {
            AxisIterator iter = new DomNodeWrapper.AttributeEnumeration(docWrapper, this);
            if ((nodeTest != net.sf.saxon.pattern.AnyNodeTest.getInstance())) {
                iter = new Navigator.AxisFilter(iter, nodeTest);
            }

            return iter;
        }

        protected internal override AxisIterator iterateChildren(net.sf.saxon.pattern.NodeTest nodeTest) {
            AxisIterator iter = new DomNodeWrapper.ChildEnumeration(docWrapper, this, true, true, span);
            if ((nodeTest != net.sf.saxon.pattern.AnyNodeTest.getInstance())) {
                iter = new Navigator.AxisFilter(iter, nodeTest);
            }

            return iter;
        }

        protected internal override AxisIterator
            iterateSiblings(net.sf.saxon.pattern.NodeTest nodeTest, bool forwards) {
            AxisIterator iter = new DomNodeWrapper.ChildEnumeration(docWrapper, this, false, forwards, span);
            if ((nodeTest != net.sf.saxon.pattern.AnyNodeTest.getInstance())) {
                iter = new Navigator.AxisFilter(iter, nodeTest);
            }

            return iter;
        }

        protected override AxisIterator iterateDescendants(net.sf.saxon.pattern.NodeTest nodeTest,
            bool includeSelf) {
            return new SteppingNavigator.DescendantAxisIterator(this, includeSelf, nodeTest);
        }

        public override string getAttributeValue(string uri, string local) {
            net.sf.saxon.pattern.NameTest test =
                new net.sf.saxon.pattern.NameTest(net.sf.saxon.type.Type.ATTRIBUTE, uri, local, getNamePool());
            AxisIterator iterator = iterateAxis(AxisInfo.ATTRIBUTE, test);
            NodeInfo attribute = ((NodeInfo) iterator.next());
            return attribute?.getStringValue();
        }

        public override bool hasChildNodes() {
            return ((node.NodeType != XmlNodeType.Attribute) && node.HasChildNodes);
        }

        public override void generateId(java.lang.StringBuilder buffer) {
            Navigator.appendSequentialKey(this, buffer, true);
        }

        public void copy(net.sf.saxon.@event.Receiver @out, int copyOptions, net.sf.saxon.s9api.Location locationId) {
            net.sf.saxon.@event.Receiver r = new net.sf.saxon.@event.NamespaceReducer(@out);
            Navigator.copy(this, r, copyOptions, locationId);
        }

        public override NamespaceBinding[] getDeclaredNamespaces(NamespaceBinding[] buffer) {
            if ((node.NodeType == XmlNodeType.Element)) {
                XmlElement elem = (XmlElement) node;
                XmlNamedNodeMap atts = elem.Attributes;
                SysCollGen.HashSet<NamespaceBinding> codes = new SysCollGen.HashSet<NamespaceBinding>();
                for (int i = 0; (i < atts.Count); i++) {
                    XmlAttribute att = (XmlAttribute) atts.Item(i);
                    string attName = att.Name;
                    if (attName.Equals("xmlns")) {
                        string prefix = "";
                        string uri = att.Value;
                        codes.Add(new NamespaceBinding(prefix, uri));
                    }
                    else {
                        if (attName.StartsWith("xmlns:")) {
                            string prefix = attName.Substring(6);
                            string uri = att.Value;
                            codes.Add(new NamespaceBinding(prefix, uri));
                        }
                        else {
                            if ((att.NamespaceURI.Length != 0)) {
                                codes.Add(new NamespaceBinding(att.Prefix, att.NamespaceURI));
                            }
                        }
                    }
                }

                if ((elem.NamespaceURI.Length != 0)) {
                    codes.Add(new NamespaceBinding(elem.Prefix, elem.NamespaceURI));
                }

                int count = codes.Count;
                NamespaceBinding[] result = new NamespaceBinding[count];
                int p = 0;
                foreach (NamespaceBinding code in codes) {
                    result[p++] = code;
                }

                return result;
            }
            else {
                return null;
            }
        }

        public SteppingNode getNextSibling() {
            XmlNode currNode = node.NextSibling;
            if ((currNode != null)) {
                return makeWrapper(currNode, docWrapper);
            }

            return null;
        }

        public SteppingNode getFirstChild() {
            XmlNode currNode = node.FirstChild;
            if ((currNode != null)) {
                return makeWrapper(currNode, docWrapper);
            }

            return null;
        }

        public SteppingNode getPreviousSibling() {
            XmlNode currNode = node.PreviousSibling;
            if ((currNode != null)) {
                return makeWrapper(currNode, docWrapper);
            }

            return null;
        }

        public SteppingNode getSuccessorElement(SteppingNode anchor, string uri, string local) {
            XmlNode stop = (((anchor == null) ? null : ((DomNodeWrapper) anchor).node));
            XmlNode next = node;
            do {
                next = getSuccessorNode(next, stop);
            } while (((next != null) &&
                      !((((next.NodeType == XmlNodeType.Element) &&
                          (((uri == null) || uri.Equals(next.NamespaceURI)))) &&
                         (((local == null) || local.Equals(next.LocalName)))))));

            if ((next == null)) {
                return null;
            }
            else {
                return makeWrapper(next, docWrapper);
            }
        }

        private static XmlNode getSuccessorNode(XmlNode start, XmlNode anchor) {
            if (start.HasChildNodes) {
                return start.FirstChild;
            }

            XmlNode p = start;
            while (true) {
                XmlNode s = p.NextSibling;
                if ((s != null)) {
                    return s;
                }

                p = p.ParentNode;
                if (((p == null) || (((anchor != null) && (p == anchor))))) {
                    return null;
                }
            }
        }

        private sealed class AttributeEnumeration : AxisIterator, LookaheadIterator {
            private readonly SysCollGen.IList<XmlNode> attList = new SysCollGen.List<XmlNode>(10);
            private int ix = 0;
            private readonly DomDocumentWrapper docWrapper;
            private readonly DomNodeWrapper start;
            private DomNodeWrapper current;

            public AttributeEnumeration(DomDocumentWrapper docWrapper, DomNodeWrapper start) {
                this.start = start;
                this.docWrapper = docWrapper;
                XmlNamedNodeMap atts = start.node.Attributes;
                if ((atts != null)) {
                    for (int i = 0; (i < atts.Count); i++) {
                        string name = atts.Item(i).Name;
                        if (!(((name.StartsWith("xmlns") && (((name.Length == 5) || (name[5] == ':'))))))) {
                            attList.Add(atts.Item(i));
                        }
                    }
                }

                ix = 0;
            }

            public bool supportsHasNext() {
                return true;
            }

            public bool hasNext() {
                return (ix < attList.Count);
            }

            public Item next() {
                if ((ix >= attList.Count)) {
                    return null;
                }

                current = makeWrapper(attList[ix], docWrapper, start, ix);
                ix++;
                return current;
            }
            

            public void close() {
            }
            
        }

        private sealed class ChildEnumeration : AxisIterator, LookaheadIterator {
            private readonly DomDocumentWrapper docWrapper;
            private readonly DomNodeWrapper start;

            private readonly SysCollGen.List<DomNodeWrapper> items =
                new SysCollGen.List<DomNodeWrapper>(20);

            private int ix;
            private int position;
            private readonly bool downwards;
            private readonly bool forwards;

            public ChildEnumeration(DomDocumentWrapper docWrapper, DomNodeWrapper start, bool downwards, bool forwards, int span) {
                this.docWrapper = docWrapper;
                this.start = start;
                this.downwards = downwards;
                this.forwards = forwards;
                position = 0;
                DomNodeWrapper commonParent;
                if (downwards) {
                    commonParent = start;
                }
                else {
                    commonParent = ((DomNodeWrapper) start.getParent());
                }

                XmlNodeList childNodes = commonParent.node.ChildNodes;
                if (downwards) {
                    if (!(forwards)) {
                        ix = (childNodes.Count - 1);
                    }
                }
                else {
                    ix = (start.getSiblingPosition() + ((forwards ? span : -1)));
                }

                if (forwards) {
                    bool previousText = false;
                    for (int i = ix; (i < childNodes.Count); i++) {
                        bool thisText = false;
                        XmlNode node = childNodes.Item(i);
                        switch (node.NodeType) {
                            case XmlNodeType.DocumentType:
                            case XmlNodeType.XmlDeclaration:
                                break;
                            case XmlNodeType.EntityReference:
                            case XmlNodeType.Text:
                            case XmlNodeType.CDATA:
                            case XmlNodeType.Whitespace:
                            case XmlNodeType.SignificantWhitespace:
                                thisText = true;
                                if (previousText) {
                                    DomNodeWrapper old = items[^1];
                                    old.span++;
                                    break;
                                }

                                goto default;
                            default:
                                previousText = thisText;
                                items.Add(makeWrapper(node, docWrapper, commonParent, i));
                                break;
                        }
                    }
                }
                else {
                    bool previousText = false;
                    for (int i = ix; (i >= 0); i--) {
                        bool thisText = false;
                        XmlNode node = childNodes.Item(i);
                        switch (node.NodeType) {
                            case XmlNodeType.DocumentType:
                            case XmlNodeType.XmlDeclaration:
                                break;
                            case XmlNodeType.EntityReference:
                            case XmlNodeType.Text:
                            case XmlNodeType.CDATA:
                            case XmlNodeType.Whitespace:
                            case XmlNodeType.SignificantWhitespace:
                                thisText = true;
                                if (previousText) {
                                    DomNodeWrapper old = items[^1];
                                    old.node = node;
                                    old.span++;
                                    break;
                                }

                                goto default;
                            default:
                                previousText = thisText;
                                items.Add(makeWrapper(node, docWrapper, commonParent, i));
                                break;
                        }
                    }
                }
            }

            public bool supportsHasNext() {
                return true;
            }

            public bool hasNext() {
                return (position < items.Count);
            }

            public Item next() {
                if (((position > -1) && (position < items.Count))) {
                    return items[position++];
                }
                else {
                    position = -1;
                    return null;
                }
            }
            
            public void close() {
            }
        }
    }
}