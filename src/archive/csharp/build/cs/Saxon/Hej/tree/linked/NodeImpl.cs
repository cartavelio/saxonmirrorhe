using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    public abstract class NodeImpl : Saxon.Hej.om.MutableNodeInfo, Saxon.Hej.tree.util.SteppingNode, Saxon.Hej.tree.wrapper.SiblingCountingNode, Saxon.Hej.s9api.Location {
        private Saxon.Hej.tree.linked.ParentNodeImpl parent;
        private int index;
        public static readonly char[] NODE_LETTER = {'x', 'e', 'a', 't', 'x', 'x', 'x', 'p', 'c', 'r', 'x', 'x', 'x', 'n'};
        public Saxon.Hej.om.Item head() {
            return this;
        }
        public Saxon.Hej.om.TreeInfo getTreeInfo() {
            return getPhysicalRoot();
        }
        public virtual Saxon.Hej.type.SchemaType getSchemaType() {
            return Saxon.Hej.type.Untyped.getInstance();
        }
        public virtual int getColumnNumber() {
            if ((parent == null)) {
                return -1;
            } else {
                return parent.getColumnNumber();
            }
        }
        public int getSiblingPosition() {
            return index;
        }
        protected internal void setSiblingPosition(int index) {
            this.index = index;
        }
        public virtual Saxon.Hej.om.AtomicSequence atomize() {
            Saxon.Hej.type.SchemaType stype = getSchemaType();
            if (((stype == Saxon.Hej.type.Untyped.getInstance()) || (stype == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))) {
                return Saxon.Hej.value.StringValue.makeUntypedAtomic(getUnicodeStringValue());
            } else {
                return stype.atomize(this);
            }
        }
        public virtual void setSystemId(string uri) {
            Saxon.Hej.om.NodeInfo p = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
            if ((p != null)) {
                p.setSystemId(uri);
            }
        }
        public override bool Equals(System.Object other) {
            return (this == other);
        }
        public override int GetHashCode() {
            return Saxon.Ejava.lang.System.identityHashCode(this);
        }
        public virtual Saxon.Hej.om.NodeName getNodeName() {
            return null;
        }
        public bool hasFingerprint() {
            return true;
        }
        public virtual int getFingerprint() {
            Saxon.Hej.om.NodeName name = getNodeName();
            if ((name == null)) {
                return -1;
            } else {
                return name.obtainFingerprint(getConfiguration().getNamePool());
            }
        }
        public virtual Saxon.Hej.om.AttributeMap attributes() {
            return Saxon.Hej.om.EmptyAttributeMap.getInstance();
        }
        public virtual void generateId(Saxon.Ejava.lang.StringBuilder buffer) {
            long seq = getSequenceNumber();
            if ((seq == -(1L))) {
                getPhysicalRoot().generateId(buffer);
                buffer.append(NODE_LETTER[getNodeKind()]);
                buffer.append(((seq+"h")+GetHashCode()));
            } else {
                parent.generateId(buffer);
                buffer.append(NODE_LETTER[getNodeKind()]);
                buffer.append(index);
            }
        }
        public virtual string getSystemId() {
            return parent.getSystemId();
        }
        public virtual string getBaseURI() {
            return parent.getBaseURI();
        }
        protected internal virtual long getSequenceNumber() {
            Saxon.Hej.tree.linked.NodeImpl prev = this;
            for (int i = 0; ; i++) {
                if ((prev is Saxon.Hej.tree.linked.ParentNodeImpl)) {
                    long prevseq = prev.getSequenceNumber();
                    return ((prevseq == -(1L)) ? prevseq : ((prevseq+0x10000)+i));
                }
                System.Diagnostics.Debug.Assert((prev != null));
                prev = prev.getPreviousInDocument();
            }
        }
        public int compareOrder(Saxon.Hej.om.NodeInfo other) {
            if ((other is Saxon.Hej.tree.NamespaceNode)) {
                return (0-other.compareOrder(this));
            }
            long a = getSequenceNumber();
            long b = ((Saxon.Hej.tree.linked.NodeImpl)(other)).getSequenceNumber();
            if (((a == -(1L)) || (b == -(1L)))) {
                return Saxon.Hej.tree.util.Navigator.compareOrder(this, (Saxon.Hej.tree.linked.NodeImpl)(other));
            }
            return a.CompareTo(b);
        }
        public virtual Saxon.Hej.Configuration getConfiguration() {
            return getPhysicalRoot().getConfiguration();
        }
        public virtual Saxon.Hej.om.NamePool getNamePool() {
            return getPhysicalRoot().getNamePool();
        }
        public string getPrefix() {
            Saxon.Hej.om.NodeName qName = getNodeName();
            return ((qName == null) ? "" : qName.getPrefix());
        }
        public string getURI() {
            Saxon.Hej.om.NodeName qName = getNodeName();
            return ((qName == null) ? "" : qName.getURI());
        }
        public string getDisplayName() {
            Saxon.Hej.om.NodeName qName = getNodeName();
            return ((qName == null) ? "" : qName.getDisplayName());
        }
        public string getLocalPart() {
            Saxon.Hej.om.NodeName qName = getNodeName();
            return ((qName == null) ? "" : qName.getLocalPart());
        }
        public virtual int getLineNumber() {
            return parent.getLineNumber();
        }
        public Saxon.Hej.s9api.Location saveLocation() {
            return this;
        }
        public Saxon.Hej.om.NodeInfo getParent() {
            if (((parent is Saxon.Hej.tree.linked.DocumentImpl) && ((Saxon.Hej.tree.linked.DocumentImpl)(parent)).isImaginary())) {
                return null;
            }
            return parent;
        }
        protected internal Saxon.Hej.tree.linked.ParentNodeImpl getRawParent() {
            return parent;
        }
        protected internal void setRawParent(Saxon.Hej.tree.linked.ParentNodeImpl parent) {
            this.parent = parent;
        }
        public virtual Saxon.Hej.tree.util.SteppingNode getPreviousSibling() {
            if ((parent == null)) {
                return null;
            }
            return parent.getNthChild((index-1));
        }
        public virtual Saxon.Hej.tree.util.SteppingNode getNextSibling() {
            if ((parent == null)) {
                return null;
            }
            return parent.getNthChild((index+1));
        }
        public virtual Saxon.Hej.tree.util.SteppingNode getFirstChild() {
            return null;
        }
        public virtual Saxon.Hej.om.NodeInfo getLastChild() {
            return null;
        }
        public Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber) {
            if ((axisNumber == Saxon.Hej.om.AxisInfo.CHILD)) {
                if ((this is Saxon.Hej.tree.linked.ParentNodeImpl)) {
                    return ((Saxon.Hej.tree.linked.ParentNodeImpl)(this)).iterateChildren(null);
                } else {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                }
            } else {
                return iterateAxis(axisNumber, Saxon.Hej.pattern.AnyNodeTest.getInstance());
            }
        }
        public Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber, Saxon.Hej.pattern.NodePredicate predicate) {
            Saxon.Hej.pattern.NodeTest nodeTest = Saxon.Hej.tree.util.Navigator.nodeTestFromPredicate(predicate);
            switch (axisNumber) {
              case Saxon.Hej.om.AxisInfo.ANCESTOR:
                return new Saxon.Hej.tree.linked.AncestorEnumeration(this, nodeTest, false);
              case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
                return new Saxon.Hej.tree.linked.AncestorEnumeration(this, nodeTest, true);
              case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
                if ((getNodeKind() != Saxon.Hej.type.Type.ELEMENT)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                } else {
                    return ((Saxon.Hej.tree.linked.ElementImpl)(this)).iterateAttributes(nodeTest);
                }
              case Saxon.Hej.om.AxisInfo.CHILD:
                if ((this is Saxon.Hej.tree.linked.ParentNodeImpl)) {
                    return ((Saxon.Hej.tree.linked.ParentNodeImpl)(this)).iterateChildren(nodeTest);
                } else {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                }
              case Saxon.Hej.om.AxisInfo.DESCENDANT:
                if ((((getNodeKind() == Saxon.Hej.type.Type.DOCUMENT) && (nodeTest is Saxon.Hej.pattern.NameTest)) && (((Saxon.Hej.pattern.NameTest)(nodeTest)).getPrimitiveType() == Saxon.Hej.type.Type.ELEMENT))) {
                    return ((Saxon.Hej.tree.linked.DocumentImpl)(this)).getAllElements(((Saxon.Hej.pattern.NameTest)(nodeTest)).getFingerprint());
                } else {
                    if (hasChildNodes()) {
                        return new Saxon.Hej.tree.util.SteppingNavigator.DescendantAxisIterator(this, false, nodeTest);
                    } else {
                        return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                    }
                }
              case Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF:
                return new Saxon.Hej.tree.util.SteppingNavigator.DescendantAxisIterator(this, true, nodeTest);
              case Saxon.Hej.om.AxisInfo.FOLLOWING:
                return new Saxon.Hej.tree.linked.FollowingEnumeration(this, nodeTest);
              case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
                return new Saxon.Hej.tree.linked.FollowingSiblingEnumeration(this, nodeTest);
              case Saxon.Hej.om.AxisInfo.NAMESPACE:
                if ((getNodeKind() != Saxon.Hej.type.Type.ELEMENT)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                }
                return Saxon.Hej.tree.NamespaceNode.makeIterator(this, nodeTest);
              case Saxon.Hej.om.AxisInfo.PARENT:
                Saxon.Hej.om.NodeInfo parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
                if ((parent == null)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                }
                return Saxon.Hej.tree.util.Navigator.filteredSingleton(parent, nodeTest);
              case Saxon.Hej.om.AxisInfo.PRECEDING:
                return new Saxon.Hej.tree.linked.PrecedingEnumeration(this, nodeTest);
              case Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING:
                return new Saxon.Hej.tree.linked.PrecedingSiblingEnumeration(this, nodeTest);
              case Saxon.Hej.om.AxisInfo.SELF:
                return Saxon.Hej.tree.util.Navigator.filteredSingleton(this, nodeTest);
              case Saxon.Hej.om.AxisInfo.PRECEDING_OR_ANCESTOR:
                return new Saxon.Hej.tree.linked.PrecedingOrAncestorEnumeration(this, nodeTest);
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown axis number "+axisNumber));
            }
        }
        public virtual string getAttributeValue(string uri, string localName) {
            return null;
        }
        public virtual Saxon.Hej.om.NodeInfo getRoot() {
            Saxon.Hej.om.NodeInfo parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
            if ((parent == null)) {
                return this;
            } else {
                return parent.getRoot();
            }
        }
        public virtual Saxon.Hej.tree.linked.DocumentImpl getPhysicalRoot() {
            Saxon.Hej.tree.linked.ParentNodeImpl up = parent;
            while (((up != null) && !(((up is Saxon.Hej.tree.linked.DocumentImpl))))) {
                up = up.getRawParent();
            }
            return (Saxon.Hej.tree.linked.DocumentImpl)(up);
        }
        public virtual Saxon.Hej.tree.linked.NodeImpl getNextInDocument(Saxon.Hej.tree.linked.NodeImpl anchor) {
            Saxon.Hej.tree.linked.NodeImpl next = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getFirstChild());
            if ((next != null)) {
                return next;
            }
            if ((this == anchor)) {
                return null;
            }
            next = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getNextSibling());
            if ((next != null)) {
                return next;
            }
            Saxon.Hej.tree.linked.NodeImpl parent = this;
            while (true) {
                parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)parent.getParent());
                if ((parent == null)) {
                    return null;
                }
                if ((parent == anchor)) {
                    return null;
                }
                next = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)parent.getNextSibling());
                if ((next != null)) {
                    return next;
                }
            }
        }
        public Saxon.Hej.tree.util.SteppingNode getSuccessorElement(Saxon.Hej.tree.util.SteppingNode anchor, string uri, string local) {
            Saxon.Hej.tree.linked.NodeImpl next = getNextInDocument((Saxon.Hej.tree.linked.NodeImpl)(anchor));
            while (((next != null) && !(((((next.getNodeKind() == Saxon.Hej.type.Type.ELEMENT) && (((uri == null) || uri.Equals(next.getURI())))) && (((local == null) || local.Equals(next.getLocalPart())))))))) {
                next = next.getNextInDocument((Saxon.Hej.tree.linked.NodeImpl)(anchor));
            }
            return next;
        }
        public virtual Saxon.Hej.tree.linked.NodeImpl getPreviousInDocument() {
            Saxon.Hej.tree.linked.NodeImpl prev = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getPreviousSibling());
            if ((prev != null)) {
                return prev.getLastDescendantOrSelf();
            }
            return ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
        }
        private Saxon.Hej.tree.linked.NodeImpl getLastDescendantOrSelf() {
            Saxon.Hej.tree.linked.NodeImpl last = (Saxon.Hej.tree.linked.NodeImpl)(getLastChild());
            if ((last == null)) {
                return this;
            }
            return last.getLastDescendantOrSelf();
        }
        public virtual Saxon.Hej.om.NamespaceBinding[] getDeclaredNamespaces(Saxon.Hej.om.NamespaceBinding[] buffer) {
            return null;
        }
        public virtual Saxon.Hej.om.NamespaceMap getAllNamespaces() {
            return null;
        }
        public virtual bool hasChildNodes() {
            return (((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getFirstChild()) != null);
        }
        public virtual void setTypeAnnotation(Saxon.Hej.type.SchemaType type) {
        }
        public virtual void delete() {
            if ((parent != null)) {
                parent.removeChild(this);
                Saxon.Hej.tree.linked.DocumentImpl newRoot = new Saxon.Hej.tree.linked.DocumentImpl();
                newRoot.setConfiguration(getConfiguration());
                newRoot.setImaginary(true);
                parent = newRoot;
            }
            index = -1;
        }
        public virtual bool isDeleted() {
            return ((index == -1) || (((parent != null) && parent.isDeleted())));
        }
        public virtual void setAttributes(Saxon.Hej.om.AttributeMap attributes) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("setAttributes() applies only to element nodes");
        }
        public virtual void removeAttribute(Saxon.Hej.om.NodeInfo attribute) {
        }
        public virtual void addAttribute(Saxon.Hej.om.NodeName name, Saxon.Hej.type.SimpleType attType, string value, int properties, bool inheritNamespaces) {
        }
        public virtual void rename(Saxon.Hej.om.NodeName newNameCode, bool inherit) {
        }
        public virtual void addNamespace(Saxon.Hej.om.NamespaceBinding nscode, bool inherit) {
        }
        public virtual void replace(Saxon.Hej.om.NodeInfo[] replacement, bool inherit) {
            if (isDeleted()) {
                throw new Saxon.Ejava.lang.IllegalStateException("Cannot replace a deleted node");
            }
            if ((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) == null)) {
                throw new Saxon.Ejava.lang.IllegalStateException("Cannot replace a parentless node");
            }
            System.Diagnostics.Debug.Assert((parent != null));
            parent.replaceChildrenAt(replacement, index, inherit);
            parent = null;
            index = -1;
        }
        public virtual void insertChildren(Saxon.Hej.om.NodeInfo[] source, bool atStart, bool inherit) {
        }
        public void insertSiblings(Saxon.Hej.om.NodeInfo[] source, bool before, bool inherit) {
            if ((parent == null)) {
                throw new Saxon.Ejava.lang.IllegalStateException("Cannot add siblings if there is no parent");
            }
            parent.insertChildrenAt(source, (before ? index : (index+1)), inherit);
        }
        public virtual void removeTypeAnnotation() {
        }
        public virtual Saxon.Hej.@event.Builder newBuilder() {
            return getPhysicalRoot().newBuilder();
        }
        public bool effectiveBooleanValue() {
            return true;
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
            Saxon.Hej.ma.zeno.ZenoSequence chain = new Saxon.Hej.ma.zeno.ZenoSequence();
            chain.appendSequence(this);
            foreach (Saxon.Hej.om.GroundedValue seq in others) {
                chain.appendSequence(seq);
            }
            return chain;
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

        /* Expanding default method children from interface NodeInfo */
        public virtual System.Collections.Generic.IEnumerable<Saxon.Hej.om.NodeInfo> children() {
            return new Saxon.Hej.tree.util.Navigator.ChildrenAsIterable(this);
        }

        /* Expanding default method children from interface NodeInfo */
        public virtual System.Collections.Generic.IEnumerable<Saxon.Hej.om.NodeInfo> children(Saxon.Hej.pattern.NodePredicate filter) {
            return new Saxon.Hej.tree.util.Navigator.ChildrenAsIterable(this, filter);
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

        /* Expanding default method isStreamed from interface Item */
        public virtual bool isStreamed() {
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
            return new Saxon.Hej.tree.iter.SingletonIterator(this);
        }

        /* Expanding default method removeNamespace from interface MutableNodeInfo */
        public virtual void removeNamespace(string prefix) {
        }

        /* Expanding default method addNamespace from interface MutableNodeInfo */
        public virtual void addNamespace(string prefix, string uri) {
        }

        /* Creating stub method getUnicodeStringValue to satisfy interface GroundedValue */
        /* (Also inherited from Item) */
        public virtual Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getNodeKind to satisfy interface NodeInfo */
        public virtual int getNodeKind() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method replaceStringValue to satisfy interface MutableNodeInfo */
        public virtual void replaceStringValue(Saxon.Hej.str.UnicodeString arg1) {
            throw new System.NotImplementedException();
        }

    }
}