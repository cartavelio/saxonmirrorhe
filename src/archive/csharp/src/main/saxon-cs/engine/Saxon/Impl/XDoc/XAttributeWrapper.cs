using System;
using System.Xml.Linq;
using Saxon.Hej.om;
using Saxon.Hej.pattern;
using Saxon.Hej.str;
using Saxon.Hej.tree.iter;
using Saxon.Hej.tree.util;
using Saxon.Hej.tree.wrapper;
using Type = Saxon.Hej.type.Type;

namespace Saxon.Impl.XDoc {
    
    /// <summary>
    /// A Saxon XDM node wrapping an XAttribute. Note that an XAttribute is not linked
    /// to its containing element, so we need to keep this reference separately to support
    /// the XPath parent axis. The XAttribute must not be a namespace declaration.
    /// </summary>
    public class XAttributeWrapper : AbstractNodeWrapper {
        private readonly XAttribute node;
        private readonly XNodeWrapper parent;
        private readonly int index;
        

        internal XAttributeWrapper(XAttribute node, XNodeWrapper parent, int index) {
            this.node = node;
            this.parent = parent;
            this.index = index;
            if (node.IsNamespaceDeclaration) {
                throw new ArgumentException("Attribute must not be a namespace declaration");
            }
        }

 

        
        public override string getSystemId() {
            return parent.getSystemId();
        }

        public override object getUnderlyingNode() {
            return node;
        }

        public override int getNodeKind() {
            return Type.ATTRIBUTE;
        }

        public override bool Equals(object other) {
            return ((other is XAttributeWrapper wrapper) && (node == wrapper.node));
        }

        public override int GetHashCode() {
            Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(64);
            generateId(buffer);
            return buffer.ToString().GetHashCode();
        }

        public override int compareOrder(NodeInfo other) {
            if ((other is XAttributeWrapper aw)) {
                if (parent.getUnderlyingNode() == aw.parent.getUnderlyingNode()) {
                    return index - aw.index;
                }

                return parent.compareOrder(aw.parent);
            }

            return parent.compareOrder(other);
        }

        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return StringView.of(node.Value);
        }


        public override string getLocalPart() {
            return node.Name.LocalName;
        }

        public override string getURI() {
            return node.Name.Namespace == XNamespace.None ? "" : node.Name.NamespaceName;
        }

        public override string getPrefix() {
            XNamespace ns = node.Name.Namespace;
            if (ns == XNamespace.None) {
                return "";
            }

            return ((XElement)parent.getUnderlyingNode()).GetPrefixOfNamespace(ns);
        }

        public override string getDisplayName() {
            XNamespace ns = node.Name.Namespace;
            if (ns == XNamespace.None) {
                return getLocalPart();
            }

            return getPrefix() + ":" + getLocalPart();
        }

        public override NodeInfo getParent() {
            return parent;
        }
        

        protected internal override AxisIterator iterateAttributes(Saxon.Hej.pattern.NodeTest nodeTest) {
            return EmptyIterator.ofNodes();
        }


        protected internal override AxisIterator iterateChildren(NodeTest nodeTest) {
            return EmptyIterator.ofNodes();
        }
        
        protected internal override AxisIterator iterateSiblings(NodeTest nodeTest, bool forwards) {
            return EmptyIterator.ofNodes();
        }
        
        protected override AxisIterator iterateDescendants(Saxon.Hej.pattern.NodeTest nodeTest,
            bool includeSelf) {
            return EmptyIterator.ofNodes();
        }

        public override string getAttributeValue(string uri, string local) {
            return null;
        }
        
        

        public override bool hasChildNodes() {
            return false;
        }

        public override void generateId(Saxon.Ejava.lang.StringBuilder buffer) {
            parent.generateId(buffer);
            buffer.append("a").append(index);
        }

        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            Saxon.Hej.@event.Receiver r = new Saxon.Hej.@event.NamespaceReducer(@out);
            Navigator.copy(this, r, copyOptions, locationId);
        }

        
    }
}