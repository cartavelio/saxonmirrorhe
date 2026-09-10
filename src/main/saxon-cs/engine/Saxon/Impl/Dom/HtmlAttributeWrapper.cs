using HtmlAgilityPack;
using net.sf.saxon.om;
using net.sf.saxon.str;
using net.sf.saxon.tree.iter;
using net.sf.saxon.tree.util;
using net.sf.saxon.tree.wrapper;

namespace Saxon.Impl.Dom {
    /// <summary>
    /// Implements net.sf.saxon.om.NodeInfo as a wrapper over an attribute node in the HtmlAgilityPack
    /// HTML document tree. The attribute node always has a parent element, and is always part of a tree
    /// rooted at a document node. There are no namespaces, and names comprise a local name only. The
    /// attributes of an element have an arbitrary ordering (document order), represented by an
    /// "index" property indicating the position relative to the containing element.
    /// </summary>
    public class HtmlAttributeWrapper : AbstractNodeWrapper, SteppingNode, SiblingCountingNode {
        private HtmlAttribute htmlAttribute;
        private HtmlNodeWrapper parent;
        private int index; // reflects the position of the attribute in document order, -1 if unknown

        private HtmlAttributeWrapper(HtmlAttribute htmlAttribute, HtmlNodeWrapper parent, int index) {
            this.htmlAttribute = htmlAttribute;
            this.parent = parent;
            this.index = index;
        }

        protected internal static HtmlAttributeWrapper makeWrapper(HtmlAttribute node, HtmlNodeWrapper elementWrapper) {
            if ((node == null)) {
                throw new java.lang.NullPointerException("HtmlAttributeWrapper#makeWrapper: Node must not be null");
            }

            if ((elementWrapper == null)) {
                throw new java.lang.NullPointerException("HtmlAttributeWrapper#makeWrapper: elementWrapper must not be null");
            }

            return new HtmlAttributeWrapper(node, elementWrapper, -1);
        }

        protected internal static HtmlAttributeWrapper makeWrapper(HtmlAttribute node, HtmlNodeWrapper elementWrapper, int index) {
            return new HtmlAttributeWrapper(node, elementWrapper, index);
        }

        public override object getUnderlyingNode() {
            return htmlAttribute;
        }

        public override int getNodeKind() {
            return net.sf.saxon.type.Type.ATTRIBUTE;
        }

        public override bool Equals(object other) {
            return ((other is HtmlAttributeWrapper wrapper) && (htmlAttribute == wrapper.htmlAttribute));
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
            return htmlAttribute.Value;
        }
        
        public override net.sf.saxon.str.UnicodeString getUnicodeStringValue() {
            return StringView.wrap(htmlAttribute.Value);
        }

        public override string getLocalPart() {
            return htmlAttribute.Name;
        }

        public override string getURI() {
            return "";
        }

        public override string getPrefix() {
            return "";
        }

        public override string getDisplayName() {
            return htmlAttribute.Name;
        }

        public override NodeInfo getParent() {
            return parent;
        }

        public int getSiblingPosition() {
            if ((index == -1)) {
                int ix = 0;
                AxisIterator iter = parent.iterateAxis(AxisInfo.ATTRIBUTE);
                while (true) {
                    NodeInfo n = ((NodeInfo) iter.next());
                    if (((n == null) || Navigator.haveSameName(this, n))) {
                        return index = ix;
                    }
                    ix++;
                }
            }
            return index;
        }

        protected internal override AxisIterator iterateAttributes(net.sf.saxon.pattern.NodeTest nodeTest) {
            return EmptyIterator.ofNodes();
        }

        protected internal override AxisIterator iterateChildren(net.sf.saxon.pattern.NodeTest nodeTest) {
            return EmptyIterator.ofNodes();
        }

        protected internal override AxisIterator iterateSiblings(net.sf.saxon.pattern.NodeTest nodeTest, bool forwards) {
            return EmptyIterator.ofNodes();
        }

        protected override AxisIterator iterateDescendants(net.sf.saxon.pattern.NodeTest nodeTest, bool includeSelf) {
            return new SteppingNavigator.DescendantAxisIterator(this, includeSelf, nodeTest);
        }

        public override string getAttributeValue(string uri, string local) {
            return null;
        }

        public override bool hasChildNodes() {
            return false;
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
            return null;
        }

        public SteppingNode getPreviousSibling() {
            return null;
        }

        public SteppingNode getSuccessorElement(SteppingNode anchor, string uri, string local) {
            return parent.getSuccessorElement(anchor, uri, local);
        }

    }
}