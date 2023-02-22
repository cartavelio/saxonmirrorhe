using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Saxon.Hej.om;
using Saxon.Hej.pattern;
using Saxon.Hej.str;
using Saxon.Hej.tree.iter;
using Saxon.Hej.tree.util;
using Saxon.Hej.tree.wrapper;
using Saxon.Hej.type;
using Saxon.Impl.DataModel;
using Saxon.Impl.Dom;

namespace Saxon.Impl.XDoc {
    
    /// <summary>
    /// A Saxon XDM node wrapping a LINQ XNode. Used for document, element, text, comment,
    /// and processing instruction nodes.
    /// </summary>
    public class XNodeWrapper : AbstractNodeWrapper, SiblingCountingNode {
        private XNode node;
        private short nodeKind;
        private XNodeWrapper parent;
        private XDocumentTree docTree;
        private NamespaceMap inScopeNamespaces;
        private int index;


        private XNodeWrapper(XNode node, XNodeWrapper parent, int index) {
            this.node = node;
            this.parent = parent;
            this.index = index;
        }


        protected internal static XNodeWrapper makeWrapper(XNode node, XDocumentTree docTree) {
            if ((node == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("NodeWrapper#makeWrapper: Node must not be null");
            }

            if ((docTree == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("NodeWrapper#makeWrapper: DocumentWrapper must not be null");
            }

            return makeWrapper(node, docTree, null, -1);
        }

        protected internal static XNodeWrapper makeWrapper(XNode node, XDocumentTree docTree,
            XNodeWrapper parent, int index) {
            XNodeWrapper wrapper;
            switch (node.NodeType) {
                case XmlNodeType.Document:
                    wrapper = (XNodeWrapper) docTree.getRootNode();
                    if ((wrapper == null)) {
                        wrapper = new XNodeWrapper(node, parent, index);
                        wrapper.nodeKind = Saxon.Hej.type.Type.DOCUMENT;
                    }

                    break;
                case XmlNodeType.Element:
                    wrapper = new XNodeWrapper(node, parent, index);
                    wrapper.nodeKind = Saxon.Hej.type.Type.ELEMENT;
                    break;

                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    wrapper = new XNodeWrapper(node, parent, index);
                    wrapper.nodeKind = Saxon.Hej.type.Type.TEXT;
                    break;
                case XmlNodeType.Comment:
                    wrapper = new XNodeWrapper(node, parent, index);
                    wrapper.nodeKind = Saxon.Hej.type.Type.COMMENT;
                    break;
                case XmlNodeType.ProcessingInstruction:
                    wrapper = new XNodeWrapper(node, parent, index);
                    wrapper.nodeKind = Saxon.Hej.type.Type.PROCESSING_INSTRUCTION;
                    break;
                default:
                    throw new Saxon.Ejava.lang.IllegalArgumentException(
                        ("Unsupported node type in DOM! " + node.NodeType + " instance " + node.ToString()));
            }

            wrapper.docTree = docTree;
            wrapper.treeInfo = docTree;
            return wrapper;
        }
        
        public override string getSystemId() {
            return docTree.getSystemId();
        }

        public override object getUnderlyingNode() {
            return node;
        }

        public override int getNodeKind() {
            return nodeKind;
        }

        public override bool Equals(object other) {
            return ((other is XNodeWrapper wrapper) && (node == wrapper.node));
        }

        public override int GetHashCode() {
            Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(64);
            generateId(buffer);
            return buffer.ToString().GetHashCode();
        }

        public override int compareOrder(NodeInfo other) {
            if ((other is XNodeWrapper otherNode)) {
                System.Collections.IComparer comparer = XNode.DocumentOrderComparer;
                return comparer.Compare(node, otherNode.node);
            }
            else {
                return -(other.compareOrder(this));
            }
        }
        
        public override string getStringValue() {
            switch (nodeKind) {
                case Saxon.Hej.type.Type.DOCUMENT:
                    XElement outermost = ((XDocument)node).Root;
                    return outermost == null ? "" : outermost.Value;
                case Saxon.Hej.type.Type.ELEMENT:
                    return ((XElement)node).Value;
                case Saxon.Hej.type.Type.TEXT:
                    return ((XText)node).Value;
                case Saxon.Hej.type.Type.COMMENT:
                    return ((XComment)node).Value;
                case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                    return ((XProcessingInstruction)node).Data;
                default:
                    return "";
            }
        }

        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return StringView.of(getStringValue());
        }

        public override string getLocalPart() {
            return node.NodeType switch {
                XmlNodeType.Element => ((XElement)node).Name.LocalName,
                XmlNodeType.ProcessingInstruction => ((XProcessingInstruction)node).Target,
                _ => ""
            };
        }

        public override string getURI() {
            if (node is XElement element) {
                XNamespace nse = element.Name.Namespace;
                return nse == XNamespace.None ? "" : nse.NamespaceName;
            }

            return "";
        }

        public override string getPrefix() {
            if (node is XElement element) {
                XNamespace nse = element.Name.Namespace;
                return element.GetPrefixOfNamespace(nse)??"";
            }

            return "";
        }

        public override string getDisplayName() {
            if (node is XElement) {
                string p = getPrefix();
                return p == "" ? getLocalPart() : p + ":" + getLocalPart();
            }
            return "";
        }

        public override NodeInfo getParent() {
            if (node is XDocument) {
                return null;
            }
            if ((parent == null)) {
                XNode xParent = node.Parent;
                if (xParent == null) {
                    return docTree.root;
                }
                parent = makeWrapper(xParent, docTree);
            }
            return parent;
        }

        protected internal override AxisIterator iterateAttributes(Saxon.Hej.pattern.NodeTest nodeTest) {
            if (node is XElement element) {
                IList<NodeInfo> attList = new List<NodeInfo>(); 
                foreach (XAttribute att in element.Attributes()) {
                    if (!att.IsNamespaceDeclaration) {
                        var wrapper = new XAttributeWrapper(att, this, index++);
                        if (nodeTest.test(wrapper)) {
                            attList.Add(wrapper);
                        }
                    }
                }
                return new NodeListIterator(attList);
            }

            return EmptyIterator.ofNodes();
            
        }

        private bool isElementOnly(NodeTest nodeTest) {
            return nodeTest.getUType() == UType.ELEMENT; 
        }

        protected internal override AxisIterator iterateChildren(NodeTest nodeTest) {
            if (node is XContainer container) {
                System.Collections.Generic.IEnumerator<XNode> childEnum = 
                    isElementOnly(nodeTest) 
                        ? container.Elements().GetEnumerator() 
                        : container.Nodes().GetEnumerator();
                int index = 0;
                AxisIterator axisIter = new AxisIteratorOverEnumerator<XNode>(
                    childEnum, xNode => makeWrapper(xNode, docTree, this, index++));
                if (nodeTest != AnyNodeTest.getInstance()) {
                    axisIter = new Navigator.AxisFilter(axisIter, nodeTest);
                }

                return axisIter;
            }

            return EmptyIterator.ofNodes();
        }
        
        protected internal override AxisIterator iterateSiblings(NodeTest nodeTest, bool forwards) {
            AxisIterator baseIter;
            if (forwards) {
                baseIter = new IncrementalIterator(this, n => {
                    XNode following = ((XNodeWrapper)n).node.NextNode;
                    return following == null ? null : makeWrapper(following, docTree, parent, index + 1);
                });
            }
            else {
                baseIter = new IncrementalIterator(this, n => {
                    XNode preceding = ((XNodeWrapper)n).node.PreviousNode;
                    return preceding == null ? null : makeWrapper(preceding, docTree, parent, index + 1);
                });
            }
            
            if (nodeTest != AnyNodeTest.getInstance()) {
                baseIter = new Navigator.AxisFilter(baseIter, nodeTest);
            }
            return baseIter;
        }
        
        protected override AxisIterator iterateDescendants(Saxon.Hej.pattern.NodeTest nodeTest,
            bool includeSelf) {
            AxisIterator descendants = new Navigator.DescendantEnumeration(this, includeSelf, true);
            if (nodeTest != AnyNodeTest.getInstance()) {
                descendants = new Navigator.AxisFilter(descendants, nodeTest);
            }

            return descendants;
        }

        public override string getAttributeValue(string uri, string local) {
            if (node is XElement element) {
                // TODO: disallow requests for a namespace
                XNamespace ns = uri ?? XNamespace.None;
                XAttribute att = element.Attribute(ns + local);
                return att?.Value;
            }
            return null;
        }
        
        

        public override bool hasChildNodes() {
            return node switch {
                XDocument => true,
                XElement element => element.FirstNode != null,
                _ => false
            };
        }

        public override void generateId(Saxon.Ejava.lang.StringBuilder buffer) {
            Navigator.appendSequentialKey(this, buffer, true);
        }

        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            Saxon.Hej.@event.Receiver r = new Saxon.Hej.@event.NamespaceReducer(@out);
            Navigator.copy(this, r, copyOptions, locationId);
        }

        public override NamespaceBinding[] getDeclaredNamespaces(NamespaceBinding[] buffer) {
            if (node is XElement element) {
                int counter = 0;
                foreach (XAttribute att in element.Attributes()) {
                    if (att.IsNamespaceDeclaration) {
                        counter++;
                    }
                }

                if (counter > 0) {
                    int i = 0;
                    NamespaceBinding[] bindings = new NamespaceBinding[counter];
                    foreach (XAttribute att in element.Attributes()) {
                        if (att.IsNamespaceDeclaration) {
                            string prefix = att.Name.LocalName == "xmlns" ? "" : att.Name.LocalName;
                            bindings[i++] = new NamespaceBinding(prefix, att.Value);
                        }
                    }

                    return bindings;
                }

            }

            return Array.Empty<NamespaceBinding>();
        }

        public override NamespaceMap getAllNamespaces() {
            if (node is XElement elem) {
                if (inScopeNamespaces != null) {
                    return inScopeNamespaces;
                }

                NodeInfo parent = getParent();
                NamespaceMap nsMap = parent != null && parent.getNodeKind() == Saxon.Hej.type.Type.ELEMENT
                    ? parent.getAllNamespaces()
                    : NamespaceMap.emptyMap();

                foreach (XAttribute att in elem.Attributes()) {
                    if (att.IsNamespaceDeclaration) {
                        if (att.Name.LocalName == "xmlns") {
                            nsMap = nsMap.bind("", att.Value);
                        } else {
                            nsMap = nsMap.bind(att.Name.LocalName, att.Value);
                        }
                    }
                }
                return inScopeNamespaces = nsMap;
            }

            // not an element node
            return null;
        }


        public int getSiblingPosition() {
            return node.NodesBeforeSelf().Count();
        }
    }
}