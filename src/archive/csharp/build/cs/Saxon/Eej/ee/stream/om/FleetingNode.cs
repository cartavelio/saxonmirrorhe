using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.om {
    public class FleetingNode : Saxon.Hej.om.NodeInfo {
        private Saxon.Eej.ee.stream.om.FleetingNode parent;
        protected internal Saxon.Eej.ee.stream.om.FleetingDocumentNode root;
        private int nodeKind;
        private Saxon.Hej.om.NodeName nodeName;
        private Saxon.Hej.type.SchemaType typeAnnotation;
        private Saxon.Hej.str.UnicodeString stringValue;
        private int nodeNumber;
        public FleetingNode()  {
        }
        public Saxon.Hej.om.Item head() {
            return this;
        }
        public virtual Saxon.Hej.om.TreeInfo getTreeInfo() {
            return root;
        }
        public int getDepth() {
            return ((parent == null) ? 0 : (parent.getDepth()+1));
        }
        public int getNodeNumber() {
            return nodeNumber;
        }
        public void setParent(Saxon.Eej.ee.stream.om.FleetingNode parent) {
            this.parent = parent;
            this.root = parent.root;
            this.nodeNumber = root.allocateNodeNumber();
        }
        public void setNodeName(Saxon.Hej.om.NodeName name) {
            this.nodeName = name;
        }
        public void setNodeKind(int nodeKind) {
            this.nodeKind = nodeKind;
        }
        public void setStringValue(Saxon.Hej.str.UnicodeString stringValue) {
            this.stringValue = stringValue;
        }
        public void setTypeAnnotation(Saxon.Hej.type.SchemaType typeAnnotation) {
            this.typeAnnotation = typeAnnotation;
        }
        public int getNodeKind() {
            return nodeKind;
        }
        public override bool Equals(System.Object other) {
            if ((this == other)) {
                return true;
            } else {
                if ((other is Saxon.Eej.ee.stream.om.FleetingNode)) {
                    Saxon.Eej.ee.stream.om.FleetingNode fn = (Saxon.Eej.ee.stream.om.FleetingNode)(other);
                    switch (getNodeKind()) {
                      case Saxon.Hej.type.Type.ATTRIBUTE:
                        return (((getNodeKind() == fn.getNodeKind()) && (getParent() == fn.getParent())) && getNodeName().Equals(fn.getNodeName()));
                      case Saxon.Hej.type.Type.NAMESPACE:
                        return (((getNodeKind() == fn.getNodeKind()) && (getParent() == fn.getParent())) && getLocalPart().Equals(fn.getLocalPart()));
                      default:
                        return false;
                    }
                } else {
                    return false;
                }
            }
        }
        public override int GetHashCode() {
            switch (getNodeKind()) {
              case Saxon.Hej.type.Type.ATTRIBUTE:
              case Saxon.Hej.type.Type.NAMESPACE:
                return (getParent().GetHashCode()^getLocalPart().GetHashCode());
              default:
                return base.GetHashCode();
            }
        }
        public virtual string getSystemId() {
            return root.getSystemId();
        }
        public virtual string getBaseURI() {
            return Saxon.Hej.tree.util.Navigator.getBaseURI(this);
        }
        public Saxon.Hej.s9api.Location saveLocation() {
            return this;
        }
        public int compareOrder(Saxon.Hej.om.NodeInfo other) {
            if (Equals(other)) {
                return 0;
            }
            switch (getNodeKind()) {
              case Saxon.Hej.type.Type.DOCUMENT:
                return -1;
              case Saxon.Hej.type.Type.ELEMENT:
                if (((other.getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE) || (other.getNodeKind() == Saxon.Hej.type.Type.NAMESPACE))) {
                    if (Equals(other.getParent())) {
                        return -1;
                    } else {
                        return compareOrder(other.getParent());
                    }
                }
                return getNodeNumber().CompareTo(((Saxon.Eej.ee.stream.om.FleetingNode)(other)).getNodeNumber());
              case Saxon.Hej.type.Type.COMMENT:
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
              case Saxon.Hej.type.Type.TEXT:
                if (((other.getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE) || (other.getNodeKind() == Saxon.Hej.type.Type.NAMESPACE))) {
                    return compareOrder(other.getParent());
                }
                return getNodeNumber().CompareTo(((Saxon.Eej.ee.stream.om.FleetingNode)(other)).getNodeNumber());
              case Saxon.Hej.type.Type.ATTRIBUTE:
                if ((other.getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE)) {
                    if (getParent().Equals(other.getParent())) {
                        return ((Saxon.Eej.ee.stream.om.FleetingElementNode)(getParent())).compareAttributeOrder(Saxon.Hej.om.NameOfNode.makeName(this), Saxon.Hej.om.NameOfNode.makeName(other));
                    } else {
                        return getParent().compareOrder(other.getParent());
                    }
                } else {
                    if ((other.getNodeKind() == Saxon.Hej.type.Type.NAMESPACE)) {
                        if (getParent().Equals(other.getParent())) {
                            return +(1);
                        } else {
                            return getParent().compareOrder(other.getParent());
                        }
                    } else {
                        if (other.Equals(getParent())) {
                            return +(1);
                        } else {
                            return getParent().compareOrder(other);
                        }
                    }
                }
              case Saxon.Hej.type.Type.NAMESPACE:
                if ((other.getNodeKind() == Saxon.Hej.type.Type.NAMESPACE)) {
                    if (getParent().Equals(other.getParent())) {
                        int i0 = ((Saxon.Eej.ee.stream.om.FleetingElementNode)(getParent())).getNamespaceNodeIndex(getLocalPart());
                        int i1 = ((Saxon.Eej.ee.stream.om.FleetingElementNode)(getParent())).getNamespaceNodeIndex(other.getLocalPart());
                        return ((i0<i1) ? -1 : +(1));
                    } else {
                        return getParent().compareOrder(other.getParent());
                    }
                } else {
                    if ((other.getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE)) {
                        if (getParent().Equals(other.getParent())) {
                            return -1;
                        } else {
                            return getParent().compareOrder(other.getParent());
                        }
                    } else {
                        if (other.Equals(getParent())) {
                            return +(1);
                        } else {
                            return getParent().compareOrder(other);
                        }
                    }
                }
            }
            throw new Saxon.Ejava.lang.IllegalStateException();
        }
        public Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            if ((stringValue == null)) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("The string value of a streamed node is not available");
            }
            return stringValue;
        }
        public bool hasFingerprint() {
            return true;
        }
        public int getFingerprint() {
            if ((nodeName == null)) {
                return -1;
            } else {
                return nodeName.obtainFingerprint(getNamePool());
            }
        }
        public Saxon.Hej.om.NodeName getNodeName() {
            return nodeName;
        }
        public string getLocalPart() {
            return ((nodeName == null) ? "" : nodeName.getLocalPart());
        }
        public Saxon.Hej.om.NamespaceUri getNamespaceUri() {
            return ((nodeName == null) ? Saxon.Hej.om.NamespaceUri.NULL : nodeName.getNamespaceUri());
        }
        public string getDisplayName() {
            return ((nodeName == null) ? "" : nodeName.getDisplayName());
        }
        public string getPrefix() {
            return ((nodeName == null) ? "" : nodeName.getPrefix());
        }
        public virtual Saxon.Hej.Configuration getConfiguration() {
            return root.getConfiguration();
        }
        public virtual Saxon.Hej.om.NamePool getNamePool() {
            return root.getNamePool();
        }
        public Saxon.Hej.type.SchemaType getSchemaType() {
            return typeAnnotation;
        }
        public virtual Saxon.Hej.om.AtomicSequence atomize() {
            return typeAnnotation.atomize(this);
        }
        public Saxon.Hej.om.NodeInfo getParent() {
            return parent;
        }
        public virtual Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber, Saxon.Hej.pattern.NodePredicate nodeTest) {
            if (Saxon.Hej.om.AxisInfo.isAlwaysEmpty(axisNumber, nodeKind)) {
                return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
            }
            switch (axisNumber) {
              case Saxon.Hej.om.AxisInfo.ANCESTOR:
                if ((nodeKind == Saxon.Hej.type.Type.DOCUMENT)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                }
                return new Saxon.Hej.tree.util.Navigator.AxisFilter(new Saxon.Hej.tree.util.Navigator.AncestorEnumeration(this, false), nodeTest);
              case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
                if ((nodeKind == Saxon.Hej.type.Type.DOCUMENT)) {
                    return Saxon.Hej.tree.util.Navigator.filteredSingleton(this, nodeTest);
                }
                return new Saxon.Hej.tree.util.Navigator.AxisFilter(new Saxon.Hej.tree.util.Navigator.AncestorEnumeration(this, true), nodeTest);
              case Saxon.Hej.om.AxisInfo.PARENT:
                return Saxon.Hej.tree.util.Navigator.filteredSingleton(getParent(), nodeTest);
              case Saxon.Hej.om.AxisInfo.SELF:
                return Saxon.Hej.tree.util.Navigator.filteredSingleton(this, nodeTest);
              case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
                return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
              case Saxon.Hej.om.AxisInfo.NAMESPACE:
                return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
              default:
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Navigation using "+Saxon.Hej.om.AxisInfo.axisName[axisNumber])+" axis is not supported from a streamed input node"), Saxon.Hej.trans.SaxonErrorCode.SXST0061);
                throw new Saxon.Ejava.lang.UnsupportedOperationException(err);
            }
        }
        public virtual string getAttributeValue(Saxon.Hej.om.NamespaceUri uri, string local) {
            return null;
        }
        public Saxon.Hej.om.NodeInfo getRoot() {
            return root;
        }
        public virtual bool hasChildNodes() {
            return false;
        }
        public void generateId(Saxon.Ejava.lang.StringBuilder buffer) {
            switch (getNodeKind()) {
              case Saxon.Hej.type.Type.ATTRIBUTE:
                getParent().generateId(buffer);
                buffer.append('a').append(((Saxon.Eej.ee.stream.om.FleetingElementNode)(getParent())).getAttributeIndex(getNamespaceUri(), getLocalPart()));
                break ;
              case Saxon.Hej.type.Type.NAMESPACE:
                getParent().generateId(buffer);
                buffer.append('n').append(((Saxon.Eej.ee.stream.om.FleetingElementNode)(getParent())).getNamespaceNodeIndex(getLocalPart()));
                break ;
              case Saxon.Hej.type.Type.DOCUMENT:
                buffer.append("s").append(System.Math.Abs(getTreeInfo().getDocumentNumber()));
                break ;
              default:
                buffer.append("s").append(System.Math.Abs(getTreeInfo().getDocumentNumber())).append("f").append(nodeNumber);
                break ;
            }
        }
        public virtual Saxon.Hej.om.NamespaceMap getInScopeNamespaces() {
            return null;
        }
        public bool isStreamed() {
            return true;
        }
        public virtual void setSystemId(string systemId) {
        }
        public Saxon.Hej.om.NamespaceBinding[] getDeclaredNamespaces(Saxon.Hej.om.NamespaceBinding[] buffer) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public virtual Saxon.Hej.om.NamespaceMap getAllNamespaces() {
            return null;
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

        /* Expanding default method toShortString from interface Item */
        public virtual string toShortString() {
            return ToString();
        }

        /* Expanding default method asIterable from interface GroundedValue */
        public virtual System.Collections.Generic.IEnumerable<Saxon.Hej.om.Item> asIterable() {
            return new Saxon.Hej.om.GroundedValueAsIterable(this);
        }

        /* Expanding default method containsNode from interface GroundedValue */
        public virtual bool containsNode(Saxon.Hej.om.NodeInfo sought) {
            return Saxon.Hej.expr.SingletonIntersectExpression.containsNode(iterate(), sought);
        }

        /* Expanding default method concatenate from interface GroundedValue */
        public virtual Saxon.Hej.om.GroundedValue concatenate(params Saxon.Hej.om.GroundedValue[]  others) {
            Saxon.Hej.ma.zeno.ZenoChain<Saxon.Hej.om.Item> chain = new Saxon.Hej.ma.zeno.ZenoChain<Saxon.Hej.om.Item>();
            chain = chain.addAll(asIterable());
            foreach (Saxon.Hej.om.GroundedValue val in others) {
                chain = chain.addAll(val.asIterable());
            }
            return new Saxon.Hej.ma.zeno.ZenoSequence(chain);
        }

        /* Expanding default method makeRepeatable from interface Sequence */
        public virtual Saxon.Hej.om.Sequence makeRepeatable() {
            return this;
        }

        /* Expanding default method isSameNodeInfo from interface NodeInfo */
        public virtual bool isSameNodeInfo(Saxon.Hej.om.NodeInfo other) {
            return Equals(other);
        }

        /* Expanding default method getPublicId from interface NodeInfo */
        public virtual string getPublicId() {
            return null;
        }

        /* Expanding default method getLineNumber from interface NodeInfo */
        public virtual int getLineNumber() {
            return -1;
        }

        /* Expanding default method getColumnNumber from interface NodeInfo */
        public virtual int getColumnNumber() {
            return -1;
        }

        /* Expanding default method getURI from interface NodeInfo */
        public virtual string getURI() {
            return getNamespaceUri().ToString();
        }

        /* Expanding default method iterateAxis from interface NodeInfo */
        public virtual Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber) {
            return iterateAxis(axisNumber, Saxon.Hej.pattern.AnyNodeTest.getInstance());
        }

        /* Expanding default method getAttributeValue from interface NodeInfo */
        public virtual string getAttributeValue(string uri, string local) {
            return getAttributeValue((uri.Length==0 ? Saxon.Hej.om.NamespaceUri.NULL : Saxon.Hej.om.NamespaceUri.of(uri)), local);
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

        /* Expanding default method isId from interface NodeInfo */
        public virtual bool isId() {
            return false;
        }

        /* Expanding default method isIdref from interface NodeInfo */
        public virtual bool isIdref() {
            return false;
        }

        /* Expanding default method isNilled from interface NodeInfo */
        public virtual bool isNilled() {
            return false;
        }

        /* Expanding default method getGenre from interface NodeInfo */
        public virtual Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.NODE;
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
            return (Saxon.Hej.tree.iter.SingletonIterator)(Saxon.Hej.tree.iter.SingletonIterator.makeIterator(this));
        }

    }
}