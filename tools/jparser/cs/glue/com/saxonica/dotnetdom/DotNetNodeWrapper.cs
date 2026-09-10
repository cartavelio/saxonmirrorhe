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

namespace com.saxonica.dotnetdom {
    public class DotNetNodeWrapper : AbstractNodeWrapper, SteppingNode, SiblingCountingNode {
        private XmlNode node;
        private short nodeKind;
        private DotNetNodeWrapper parent;
        protected DotNetDocumentWrapper docWrapper;
        private int index;
        protected int span = 1;

        private DotNetNodeWrapper(XmlNode node, DotNetNodeWrapper parent, int index)  {
            this.node = node;
            this.parent = parent;
            this.index = index;
        }
        internal DotNetNodeWrapper()  {
        }
        protected internal static DotNetNodeWrapper makeWrapper(XmlNode node, DotNetDocumentWrapper docWrapper) {
            if ((node==null)) {
                throw new java.lang.NullPointerException("NodeWrapper#makeWrapper: Node must not be null");
            }
            if ((docWrapper==null)) {
                throw new java.lang.NullPointerException("NodeWrapper#makeWrapper: DocumentWrapper must not be null");
            }
            return makeWrapper(node, docWrapper, null, -1);
        }
        protected internal static DotNetNodeWrapper makeWrapper(XmlNode node, DotNetDocumentWrapper docWrapper, DotNetNodeWrapper parent, int index) {
            DotNetNodeWrapper wrapper;
            switch (node.NodeType) {
              case XmlNodeType.Document:
                wrapper = (DotNetNodeWrapper)docWrapper.getRootNode();
                if ((wrapper==null)) {
                    wrapper = new DotNetNodeWrapper(node, parent, index);
                    wrapper.nodeKind = net.sf.saxon.type.Type.DOCUMENT;
                }
                break ;
              case XmlNodeType.Element:
                wrapper = new DotNetNodeWrapper(node, parent, index);
                wrapper.nodeKind = net.sf.saxon.type.Type.ELEMENT;
                break ;
              case XmlNodeType.Attribute:
                wrapper = new DotNetNodeWrapper(node, parent, index);
                wrapper.nodeKind = net.sf.saxon.type.Type.ATTRIBUTE;
                break ;
              case XmlNodeType.Text:
              case XmlNodeType.CDATA:
              case XmlNodeType.Whitespace:
              case XmlNodeType.SignificantWhitespace:
                wrapper = new DotNetNodeWrapper(node, parent, index);
                wrapper.nodeKind = net.sf.saxon.type.Type.TEXT;
                break ;
              case XmlNodeType.Comment:
                wrapper = new DotNetNodeWrapper(node, parent, index);
                wrapper.nodeKind = net.sf.saxon.type.Type.COMMENT;
                break ;
              case XmlNodeType.ProcessingInstruction:
                wrapper = new DotNetNodeWrapper(node, parent, index);
                wrapper.nodeKind = net.sf.saxon.type.Type.PROCESSING_INSTRUCTION;
                break ;
              default:
                throw new java.lang.IllegalArgumentException(((("Unsupported node type in DOM! "+node.NodeType +" instance ")+node.ToString));
            }
            wrapper.docWrapper = docWrapper;
            wrapper.treeInfo = docWrapper;
            return wrapper;
        }
        public object getUnderlyingNode() {
            return node;
        }
        public int getNodeKind() {
            return nodeKind;
        }
        public bool equals(NodeInfo other) {
            return ((other is DotNetNodeWrapper)&&(node==((DotNetNodeWrapper)other).node));
        }
        public override int hashCode() {
            java.lang.StringBuilder buffer = new java.lang.StringBuilder(64);
            generateId(buffer);
            return buffer.ToString().GetHashCode();
        }
        public int compareOrder(NodeInfo other) {
            if ((other is SiblingCountingNode)) {
                return Navigator.compareOrder(this, (SiblingCountingNode)other);
            } else {
                return -(other.compareOrder(this));
            }
        }
        public net.sf.saxon.str.UnicodeString getUnicodeStringValue() {
            switch (nodeKind) {
              case net.sf.saxon.type.Type.DOCUMENT:
              case net.sf.saxon.type.Type.ELEMENT:
                return net.sf.saxon.str.StringView.tidy(node.InnerText);
              case net.sf.saxon.type.Type.ATTRIBUTE:
                return net.sf.saxon.str.StringView.tidy(node.Value);
              case net.sf.saxon.type.Type.TEXT:
                if ((span==1)) {
                    return net.sf.saxon.str.StringView.tidy(node.InnerText);
                } else {
                    net.sf.saxon.str.UnicodeBuilder fsb = new net.sf.saxon.str.UnicodeBuilder();
                    XmlNode textNode = node;
                    for (int i = 0; (i<span); i++) {
                        fsb.append(textNode.InnerText);
                        textNode = textNode.NextSibling;
                    }
                    return fsb.toUnicodeString();
                }
              case net.sf.saxon.type.Type.COMMENT:
              case net.sf.saxon.type.Type.PROCESSING_INSTRUCTION:
                return net.sf.saxon.str.StringView.tidy(node.Value);
              default:
                return net.sf.saxon.str.EmptyUniString.getInstance();
            }
        }
        public string getLocalPart() {
            return node.LocalName;
        }
        public string getURI() {
            NodeInfo element;
            if ((nodeKind==net.sf.saxon.type.Type.ELEMENT)) {
                element = this;
            } else {
                if ((nodeKind==net.sf.saxon.type.Type.ATTRIBUTE)) {
                    element = parent;
                } else {
                    return "";
                }
            }
            string uri = node.NamespaceURI;
            if ((uri!=null)) {
                return uri;
            }
            if (node.Name.StartsWith("xml:")) {
                return net.sf.saxon.lib.NamespaceConstant.XML;
            }
            string[] parts;
            try {
                parts = NameChecker.getQNameParts(node.Name);
            } catch (QNameException e) {
                throw new java.lang.IllegalStateException(("Invalid QName in DOM node. "+e));
            }
            if (((nodeKind==net.sf.saxon.type.Type.ATTRIBUTE)&&(parts[0].Length==0))) {
                uri = "";
            } else {
                AxisIterator nsiter = element.iterateAxis(AxisInfo.NAMESPACE);
                NodeInfo ns;
                while (((ns = ((NodeInfo)nsiter.next()))!=null)) {
                    if (ns.getLocalPart().Equals(parts[0])) {
                        uri = ns.getStringValue();
                        break ;
                    }
                }
                if ((uri==null)) {
                    if ((parts[0].Length==0)) {
                        uri = "";
                    } else {
                        throw new java.lang.IllegalStateException(("Undeclared namespace prefix in DOM input: "+parts[0]));
                    }
                }
            }
            return uri;
        }
        public string getPrefix() {
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
        public NodeInfo getParent() {
            if ((parent==null)) {
                switch (getNodeKind()) {
                  case net.sf.saxon.type.Type.ATTRIBUTE:
                    parent = makeWrapper(((XmlAttribute)node).OwnerElement, docWrapper);
                    break ;
                  default:
                    XmlNode p = node.ParentNode;
                    if ((p==null)) {
                        return null;
                    } else {
                        parent = makeWrapper(p, docWrapper);
                    }
                    break;
                }
            }
            return parent;
        }
        public int getSiblingPosition() {
            if ((index==-1)) {
                switch (nodeKind) {
                  case net.sf.saxon.type.Type.ELEMENT:
                  case net.sf.saxon.type.Type.TEXT:
                  case net.sf.saxon.type.Type.COMMENT:
                  case net.sf.saxon.type.Type.PROCESSING_INSTRUCTION:
                    int ix = 0;
                    XmlNode start = node;
                    while (true) {
                        start = start.PreviousSibling;
                        if ((start==null)) {
                            index = ix;
                            return ix;
                        }
                        ix++;
                    }
                  case net.sf.saxon.type.Type.ATTRIBUTE:
                    ix = 0;
                    AxisIterator iter = parent.iterateAxis(AxisInfo.ATTRIBUTE);
                    while (true) {
                        NodeInfo n = ((NodeInfo)iter.next());
                        if (((n==null)||Navigator.haveSameName(this, n))) {
                            index = ix;
                            return ix;
                        }
                        ix++;
                    }
                  case net.sf.saxon.type.Type.NAMESPACE:
                    ix = 0;
                    iter = parent.iterateAxis(AxisInfo.NAMESPACE);
                    while (true) {
                        NodeInfo n = ((NodeInfo)iter.next());
                        if (((n==null)||Navigator.haveSameName(this, n))) {
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
            AxisIterator iter = new DotNetNodeWrapper.AttributeEnumeration(this);
            if ((nodeTest!=net.sf.saxon.pattern.AnyNodeTest.getInstance())) {
                iter = new Navigator.AxisFilter(iter, nodeTest);
            }
            return iter;
        }
        protected internal override AxisIterator iterateChildren(net.sf.saxon.pattern.NodeTest nodeTest) {
            AxisIterator iter = new DotNetNodeWrapper.ChildEnumeration(this, true, true);
            if ((nodeTest!=net.sf.saxon.pattern.AnyNodeTest.getInstance())) {
                iter = new Navigator.AxisFilter(iter, nodeTest);
            }
            return iter;
        }
        protected internal override AxisIterator iterateSiblings(net.sf.saxon.pattern.NodeTest nodeTest, bool forwards) {
            AxisIterator iter = new DotNetNodeWrapper.ChildEnumeration(this, false, forwards);
            if ((nodeTest!=net.sf.saxon.pattern.AnyNodeTest.getInstance())) {
                iter = new Navigator.AxisFilter(iter, nodeTest);
            }
            return iter;
        }
        protected internal override AxisIterator iterateDescendants(net.sf.saxon.pattern.NodeTest nodeTest, bool includeSelf) {
            return new SteppingNavigator.DescendantAxisIterator(this, includeSelf, nodeTest);
        }
        public override string getAttributeValue(string uri, string local) {
            net.sf.saxon.pattern.NameTest test = new net.sf.saxon.pattern.NameTest(net.sf.saxon.type.Type.ATTRIBUTE, uri, local, getNamePool());
            AxisIterator iterator = iterateAxis(AxisInfo.ATTRIBUTE, test);
            NodeInfo attribute = ((NodeInfo)iterator.next());
            if ((attribute==null)) {
                return null;
            } else {
                return attribute.getStringValue();
            }
        }
        public override bool hasChildNodes() {
            return ((node.NodeType!=XmlNodeType.Attribute) && node.HasChildNodes);
        }
        public void generateId(java.lang.StringBuilder buffer) {
            Navigator.appendSequentialKey(this, buffer, true);
        }
        public void copy(net.sf.saxon.@event.Receiver @out, int copyOptions, net.sf.saxon.s9api.Location locationId) {
            net.sf.saxon.@event.Receiver r = new net.sf.saxon.@event.NamespaceReducer(@out);
            Navigator.copy(this, r, copyOptions, locationId);
        }
        public override NamespaceBinding[] getDeclaredNamespaces(NamespaceBinding[] buffer) {
            if ((node.NodeType == XmlNodeType.Element)) {
                XmlElement elem = (XmlElement)node;
                XmlNamedNodeMap atts = elem.Attributes;
                java.util.Set<NamespaceBinding> codes = new java.util.HashSet<NamespaceBinding>();
                for (int i = 0; (i<atts.Count); i++) {
                    XmlAttribute att = (XmlAttribute)atts.Item(i);
                    string attName = att.Name;
                    if (attName.Equals("xmlns")) {
                        string prefix = "";
                        string uri = att.Value;
                        codes.add(new NamespaceBinding(prefix, uri));
                    } else {
                        if (attName.StartsWith("xmlns:")) {
                            string prefix = attName.Substring(6);
                            string uri = att.Value;
                            codes.add(new NamespaceBinding(prefix, uri));
                        } else {
                            if ((att.NamespaceURI.Length != 0)) {
                                codes.add(new NamespaceBinding(att.Prefix, att.NamespaceURI));
                            }
                        }
                    }
                }
                if ((elem.NamespaceURI.Length!=0)) {
                    codes.add(new NamespaceBinding(elem.Prefix, elem.NamespaceURI));
                }
                int count = codes.size();
                NamespaceBinding[] result = new NamespaceBinding[count];
                int p = 0;
                foreach (NamespaceBinding code in codes) {
                    result[p++] = code;
                }
                return result;
            } else {
                return null;
            }
        }
        public SteppingNode getNextSibling() {
            XmlNode currNode = node.NextSibling;
            if ((currNode!=null)) {
                return makeWrapper(currNode, docWrapper);
            }
            return null;
        }
        public SteppingNode getFirstChild() {
            XmlNode currNode = node.FirstChild;
            if ((currNode!=null)) {
                return makeWrapper(currNode, docWrapper);
            }
            return null;
        }
        public SteppingNode getPreviousSibling() {
            XmlNode currNode = node.PreviousSibling;
            if ((currNode!=null)) {
                return makeWrapper(currNode, docWrapper);
            }
            return null;
        }
        public SteppingNode getSuccessorElement(SteppingNode anchor, string uri, string local) {
            XmlNode stop = (((anchor==null) ? null : ((DotNetNodeWrapper)anchor).node));
            XmlNode next = node;
            do {
                next = getSuccessorNode(next, stop);
            } while (((next != null) &&
                      !((((next.NodeType == XmlNodeType.Element) &&
                          (((uri == null) || uri.Equals(next.NamespaceURI)))) &&
                         (((local == null) || local.Equals(next.LocalName)))))));
            if ((next==null)) {
                return null;
            } else {
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
                if ((s!=null)) {
                    return s;
                }
                p = p.ParentNode;
                if (((p==null)||(((anchor!=null)&&(p==anchor))))) {
                    return null;
                }
            }
        }
        private sealed class AttributeEnumeration : AxisIterator, LookaheadIterator {
            private readonly SysCollGen.IList<XmlNode> attList = new SysCollGen.List<XmlNode>(10);
            private int ix = 0;
            private readonly DotNetNodeWrapper start;
            private DotNetNodeWrapper current;
            public AttributeEnumeration(DotNetNodeWrapper start)  {
                this.start = start;
                XmlNamedNodeMap atts = start.node.Attributes;
                if ((atts!=null)) {
                    for (int i = 0; (i<atts.Count); i++) {
                        string name = atts.Item(i).Name;
                        if (!(((name.StartsWith("xmlns")&&(((name.Length==5)||(name[5]==':'))))))) {
                            attList.Add(atts.Item(i));
                        }
                    }
                }
                ix = 0;
            }
            public bool hasNext() {
                return (ix<attList.Count);
            }
            public Item next() {
                if ((ix>=attList.Count)) {
                    return null;
                }
                current = makeWrapper(attList[ix], docWrapper, start, ix);
                ix++;
                return current;
            }
            public java.util.EnumSet<SequenceIteratorProperty> getProperties() {
                return java.util.EnumSet<SequenceIteratorProperty>.of(SequenceIteratorProperty.LOOKAHEAD);
            }
        
            /* Expanding default method forEach from interface UnfailingIterator */
            public void forEach(java.util.function.Consumer<Item> consumer) {
                Item item;
                while (((item = next())!=null)) {
                    consumer(item);
                }
            }

            /* Expanding default method iterator from interface UnfailingIterator */
            public java.util.Iterator<Item> iterator() {
                return new net.sf.saxon.tree.jiter.WrappingJavaIterator<Item>(this);
            }
            // Extra methods to make the class IEnumerable as well as Iterable
            public System.Collections.Generic.IEnumerator<Item> GetEnumerator() {
                return new com.saxonica.glue.EnumeratorFromIterator<Item>(iterator());
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            /* Expanding default method close from interface SequenceIterator */
            public void close() {
            }

            /* Expanding default method forEachOrFail from interface SequenceIterator */
            public void forEachOrFail(ItemConsumer<Item> consumer) {
                Item item;
                while (((item = next())!=null)) {
                    consumer(item);
                }
            }

            /* Expanding default method materialize from interface SequenceIterator */
            public GroundedValue materialize() {
                return new net.sf.saxon.value.SequenceExtent(this).reduce();
            }
}
        private sealed class ChildEnumeration : AxisIterator, LookaheadIterator {
            private readonly DotNetNodeWrapper start;
            private readonly java.util.ArrayList<DotNetNodeWrapper> items = new java.util.ArrayList<DotNetNodeWrapper>(20);
            private int ix;
            private int position;
            private readonly bool downwards;
            private readonly bool forwards;
            public ChildEnumeration(DotNetNodeWrapper start, bool downwards, bool forwards)  {
                this.start = start;
                this.downwards = downwards;
                this.forwards = forwards;
                position = 0;
                DotNetNodeWrapper commonParent;
                if (downwards) {
                    commonParent = start;
                } else {
                    commonParent = ((DotNetNodeWrapper)start.getParent());
                }
                XmlNodeList childNodes = commonParent.node.ChildNodes;
                if (downwards) {
                    if (!(forwards)) {
                        ix = (childNodes.Count-1);
                    }
                } else {
                    ix = (start.getSiblingPosition()+((forwards ? span : -1)));
                }
                if (forwards) {
                    bool previousText = false;
                    for (int i = ix; (i<childNodes.Count); i++) {
                        bool thisText = false;
                        XmlNode node = childNodes.Item(i);
                        switch (node.NodeType) {
                          case XmlNodeType.DocumentType:
                          case XmlNodeType.XmlDeclaration:
                            break ;
                          case XmlNodeType.EntityReference:
                          case XmlNodeType.Text:
                          case XmlNodeType.CDATA:
                          case XmlNodeType.Whitespace:
                          case XmlNodeType.SignificantWhitespace:
                            thisText = true;
                            if (previousText) {
                                DotNetNodeWrapper old = items.get((items.size()-1));
                                old.span++;
                                break ;
                            }
                            goto default;
                          default:
                            previousText = thisText;
                            items.add(makeWrapper(node, docWrapper, commonParent, i));
                            break;
                        }
                    }
                } else {
                    bool previousText = false;
                    for (int i = ix; (i>=0); i--) {
                        bool thisText = false;
                        XmlNode node = childNodes.Item(i);
                        switch (node.NodeType) {
                          case XmlNodeType.DocumentType:
                          case XmlNodeType.XmlDeclaration:
                            break ;
                          case XmlNodeType.EntityReference:
                          case XmlNodeType.Text:
                          case XmlNodeType.CDATA:
                          case XmlNodeType.Whitespace:
                          case XmlNodeType.SignificantWhitespace:
                            thisText = true;
                            if (previousText) {
                                DotNetNodeWrapper old = items.get((items.size()-1));
                                old.node = node;
                                old.span++;
                                break ;
                            }
                            goto default;
                          default:
                            previousText = thisText;
                            items.add(makeWrapper(node, docWrapper, commonParent, i));
                        }
                    }
                }
            }
            public bool hasNext() {
                return (position<items.size());
            }
            public Item next() {
                if (((position>-1)&&(position<items.size()))) {
                    return items.get(position++);
                } else {
                    position = -1;
                    return null;
                }
            }
            public java.util.EnumSet<SequenceIteratorProperty> getProperties() {
                return java.util.EnumSet<SequenceIteratorProperty>.of(SequenceIteratorProperty.LOOKAHEAD);
            }
        
            /* Expanding default method forEach from interface UnfailingIterator */
            public void forEach(java.util.function.Consumer<Item> consumer) {
                Item item;
                while (((item = next())!=null)) {
                    consumer(item);
                }
            }

            /* Expanding default method iterator from interface UnfailingIterator */
            public java.util.Iterator<Item> iterator() {
                return new net.sf.saxon.tree.jiter.WrappingJavaIterator<Item>(this);
            }
            // Extra methods to make the class IEnumerable as well as Iterable
            public System.Collections.Generic.IEnumerator<Item> GetEnumerator() {
                return new com.saxonica.glue.EnumeratorFromIterator<Item>(iterator());
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            /* Expanding default method close from interface SequenceIterator */
            public void close() {
            }

            /* Expanding default method forEachOrFail from interface SequenceIterator */
            public void forEachOrFail(ItemConsumer<Item> consumer) {
                Item item;
                while (((item = next())!=null)) {
                    consumer(item);
                }
            }

            /* Expanding default method materialize from interface SequenceIterator */
            public GroundedValue materialize() {
                return new net.sf.saxon.value.SequenceExtent(this).reduce();
            }
}
    }
}