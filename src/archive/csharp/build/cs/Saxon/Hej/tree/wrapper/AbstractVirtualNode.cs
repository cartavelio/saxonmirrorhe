using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.wrapper {
    public abstract class AbstractVirtualNode : Saxon.Hej.tree.wrapper.VirtualNode {
        protected internal Saxon.Hej.om.NodeInfo node;
        protected internal Saxon.Hej.tree.wrapper.AbstractVirtualNode parent;
        protected internal Saxon.Hej.om.TreeInfo docWrapper;
        public Saxon.Hej.om.TreeInfo getTreeInfo() {
            return docWrapper;
        }
        public System.Object getUnderlyingNode() {
            return node;
        }
        public int getFingerprint() {
            if (node.hasFingerprint()) {
                return node.getFingerprint();
            } else {
                throw new Saxon.Ejava.lang.UnsupportedOperationException();
            }
        }
        public bool hasFingerprint() {
            return node.hasFingerprint();
        }
        public System.Object getRealNode() {
            System.Object u = this;
            do {
                u = ((Saxon.Hej.tree.wrapper.VirtualNode)(u)).getUnderlyingNode();
            } while ((u is Saxon.Hej.tree.wrapper.VirtualNode));
            return u;
        }
        public int getNodeKind() {
            return node.getNodeKind();
        }
        public virtual Saxon.Hej.om.AtomicSequence atomize() {
            return node.atomize();
        }
        public virtual Saxon.Hej.type.SchemaType getSchemaType() {
            return node.getSchemaType();
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.tree.wrapper.AbstractVirtualNode)) {
                return node.Equals(((Saxon.Hej.tree.wrapper.AbstractVirtualNode)(other)).node);
            } else {
                return node.Equals(other);
            }
        }
        public override int GetHashCode() {
            return (node.GetHashCode()^0x3c3c3c3c);
        }
        public virtual string getSystemId() {
            return node.getSystemId();
        }
        public void setSystemId(string uri) {
            node.setSystemId(uri);
        }
        public virtual string getBaseURI() {
            return node.getBaseURI();
        }
        public int getLineNumber() {
            return node.getLineNumber();
        }
        public int getColumnNumber() {
            return node.getColumnNumber();
        }
        public Saxon.Hej.s9api.Location saveLocation() {
            return this;
        }
        public virtual int compareOrder(Saxon.Hej.om.NodeInfo other) {
            if ((other is Saxon.Hej.tree.wrapper.AbstractVirtualNode)) {
                return node.compareOrder(((Saxon.Hej.tree.wrapper.AbstractVirtualNode)(other)).node);
            } else {
                return node.compareOrder(other);
            }
        }
        public virtual Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return node.getUnicodeStringValue();
        }
        public string getLocalPart() {
            return node.getLocalPart();
        }
        public string getURI() {
            return node.getURI();
        }
        public string getPrefix() {
            return node.getPrefix();
        }
        public string getDisplayName() {
            return node.getDisplayName();
        }
        public virtual Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber, Saxon.Hej.pattern.NodePredicate nodeTest) {
            return new Saxon.Hej.tree.util.Navigator.AxisFilter(iterateAxis(axisNumber), nodeTest);
        }
        public string getAttributeValue(string uri, string local) {
            return node.getAttributeValue(uri, local);
        }
        public Saxon.Hej.om.NodeInfo getRoot() {
            Saxon.Hej.om.NodeInfo p = this;
            while (true) {
                Saxon.Hej.om.NodeInfo q = p.getParent();
                if ((q == null)) {
                    return p;
                }
                p = q;
            }
        }
        public bool hasChildNodes() {
            return node.hasChildNodes();
        }
        public void generateId(Saxon.Ejava.lang.StringBuilder buffer) {
            node.generateId(buffer);
        }
        public Saxon.Hej.om.NamespaceBinding[] getDeclaredNamespaces(Saxon.Hej.om.NamespaceBinding[] buffer) {
            return node.getDeclaredNamespaces(buffer);
        }
        public Saxon.Hej.om.NamespaceMap getAllNamespaces() {
            return node.getAllNamespaces();
        }
        public bool isId() {
            return node.isId();
        }
        public bool isIdref() {
            return node.isIdref();
        }
        public virtual bool isNilled() {
            return node.isNilled();
        }
    
        /* Expanding default method effectiveBooleanValue from interface GroundedValue */
        public virtual bool effectiveBooleanValue() {
            return Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(iterate());
        }

        /* Expanding default method reduce from interface Item */
        public virtual Saxon.Hej.om.GroundedValue reduce() {
            return this;
        }

        /* Expanding default method materialize from interface Sequence */
        public virtual Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.om.SequenceTool.toGroundedValue(iterate());
        }

        /* Expanding default method toShortString from interface NodeInfo */
        public virtual string toShortString() {
            switch (getNodeKind()) {
              case Saxon.Hej.type.Type.DOCUMENT:
                return "document-node()";
              case Saxon.Hej.type.Type.ELEMENT:
                return (("<"+getDisplayName())+"/>");
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return ("@"+getDisplayName());
              case Saxon.Hej.type.Type.TEXT:
                return (("text(\""+Saxon.Hej.trans.Err.truncate30(getUnicodeStringValue()))+"\")");
              case Saxon.Hej.type.Type.COMMENT:
                return (("<!--"+Saxon.Hej.trans.Err.truncate30(getUnicodeStringValue()))+"-->");
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                return (("<?"+getDisplayName())+"?>");
              case Saxon.Hej.type.Type.NAMESPACE:
                string prefix = getLocalPart();
                return (((("xmlns"+((prefix.Equals("") ? "" : (":"+prefix))))+"=\"")+getUnicodeStringValue())+'"');
              default:
                return "";
            }
        }

        /* Expanding default method asIterable from interface GroundedValue */
        public virtual System.Collections.Generic.IEnumerable<Saxon.Hej.om.Item> asIterable() {
            return new Saxon.Hej.om.GroundedValueAsIterable<Saxon.Hej.om.Item>(this);
        }

        /* Expanding default method containsNode from interface GroundedValue */
        public virtual bool containsNode(Saxon.Hej.om.NodeInfo sought) {
            return Saxon.Hej.expr.SingletonIntersectExpression.containsNode(iterate(), sought);
        }

        /* Expanding default method concatenate from interface GroundedValue */
        public virtual Saxon.Hej.om.GroundedValue concatenate(params Saxon.Hej.om.GroundedValue[]  others) {
            Saxon.Hej.ma.zeno.ZenoSequence chain = new Saxon.Hej.ma.zeno.ZenoSequence();
            chain.appendSequence(this);
            foreach (Saxon.Hej.om.GroundedValue seq in others) {
                chain.appendSequence(seq);
            }
            return chain;
        }

        /* Expanding default method head from interface Item */
        public virtual Saxon.Hej.om.Item head() {
            return this;
        }

        /* Expanding default method getStringValue from interface Item */
        public virtual string getStringValue() {
            return getUnicodeStringValue().ToString();
        }

        /* Expanding default method itemAt from interface Item */
        public virtual Saxon.Hej.om.Item itemAt(int n) {
            return ((n == 0) ? head() : null);
        }

        /* Expanding default method subsequence from interface Item */
        public virtual Saxon.Hej.om.GroundedValue subsequence(int start, int length) {
            if (((start<=0) && (((start+length))>0))) {
                return this;
            } else {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        }

        /* Expanding default method getLength from interface Item */
        public virtual int getLength() {
            return 1;
        }

        /* Expanding default method iterate from interface Item */
        public virtual Saxon.Hej.om.SequenceIterator iterate() {
            return new Saxon.Hej.tree.iter.SingletonIterator(this);
        }

        /* Expanding default method isStreamed from interface NodeInfo */
        public virtual bool isStreamed() {
            return false;
        }

        /* Expanding default method getConfiguration from interface NodeInfo */
        public virtual Saxon.Hej.Configuration getConfiguration() {
            return getTreeInfo().getConfiguration();
        }

        /* Expanding default method isSameNodeInfo from interface NodeInfo */
        public virtual bool isSameNodeInfo(Saxon.Hej.om.NodeInfo other) {
            return Equals(other);
        }

        /* Expanding default method getPublicId from interface NodeInfo */
        public virtual string getPublicId() {
            return null;
        }

        /* Expanding default method iterateAxis from interface NodeInfo */
        public virtual Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber) {
            return iterateAxis(axisNumber, Saxon.Hej.pattern.AnyNodeTest.getInstance());
        }

        /* Expanding default method children from interface NodeInfo */
        public virtual System.Collections.Generic.IEnumerable<Saxon.Hej.om.NodeInfo> children() {
            return new Saxon.Hej.tree.util.Navigator.ChildrenAsIterable(this);
        }

        /* Expanding default method children from interface NodeInfo */
        public virtual System.Collections.Generic.IEnumerable<Saxon.Hej.om.NodeInfo> children(Saxon.Hej.pattern.NodePredicate filter) {
            return new Saxon.Hej.tree.util.Navigator.ChildrenAsIterable(this, filter);
        }

        /* Expanding default method attributes from interface NodeInfo */
        public virtual Saxon.Hej.om.AttributeMap attributes() {
            Saxon.Hej.om.AttributeMap atts = Saxon.Hej.om.EmptyAttributeMap.getInstance();
            if ((getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                Saxon.Hej.tree.iter.AxisIterator iter = iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE);
                Saxon.Hej.om.NodeInfo attr;
                while (((attr = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
                    atts = atts.put(new Saxon.Hej.om.AttributeInfo(Saxon.Hej.om.NameOfNode.makeName(attr), (Saxon.Hej.type.SimpleType)(attr.getSchemaType()), attr.getStringValue(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE));
                }
            }
            return atts;
        }

        /* Expanding default method copy from interface NodeInfo */
        public virtual void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            Saxon.Hej.tree.util.Navigator.copy(this, @out, copyOptions, locationId);
        }

        /* Expanding default method asActiveSource from interface NodeInfo */
        public virtual Saxon.Hej.lib.ActiveSource asActiveSource() {
            return new Saxon.Hej.om.NodeSource(this);
        }

        /* Expanding default method getGenre from interface NodeInfo */
        public virtual Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.NODE;
        }

        /* Expanding default method makeRepeatable from interface Sequence */
        public virtual Saxon.Hej.om.Sequence makeRepeatable() {
            return this;
        }

        /* Creating stub method getParent to satisfy interface NodeInfo */
        public virtual Saxon.Hej.om.NodeInfo getParent() {
            throw new System.NotImplementedException();
        }

    }
}