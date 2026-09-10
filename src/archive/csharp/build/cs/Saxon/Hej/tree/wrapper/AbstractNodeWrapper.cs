using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.wrapper {
    public abstract class AbstractNodeWrapper : Saxon.Hej.om.NodeInfo, Saxon.Hej.tree.wrapper.VirtualNode {
        protected internal Saxon.Hej.om.TreeInfo treeInfo;
        public Saxon.Hej.om.TreeInfo getTreeInfo() {
            return treeInfo;
        }
        public System.Object getRealNode() {
            return getUnderlyingNode();
        }
        public Saxon.Hej.om.NamePool getNamePool() {
            return getConfiguration().getNamePool();
        }
        public Saxon.Hej.om.AtomicSequence atomize() {
            switch (getNodeKind()) {
              case Saxon.Hej.type.Type.COMMENT:
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                return new Saxon.Hej.value.StringValue(getUnicodeStringValue());
              default:
                return Saxon.Hej.value.StringValue.makeUntypedAtomic(getUnicodeStringValue());
            }
        }
        public override bool Equals(System.Object other) {
            if (!(((other is Saxon.Hej.tree.wrapper.AbstractNodeWrapper)))) {
                return false;
            }
            Saxon.Hej.tree.wrapper.AbstractNodeWrapper ow = (Saxon.Hej.tree.wrapper.AbstractNodeWrapper)(other);
            return getUnderlyingNode().Equals(ow.getUnderlyingNode());
        }
        public override int GetHashCode() {
            return getUnderlyingNode().GetHashCode();
        }
        public virtual  /* REPLACING public */ string getSystemId() {
            if ((treeInfo is Saxon.Hej.om.GenericTreeInfo)) {
                return ((Saxon.Hej.om.GenericTreeInfo)(treeInfo)).getSystemId();
            } else {
                throw new Saxon.Ejava.lang.UnsupportedOperationException();
            }
        }
        public void setSystemId(string uri) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public virtual  /* REPLACING public */ string getBaseURI() {
            if ((getNodeKind() == Saxon.Hej.type.Type.NAMESPACE)) {
                return null;
            }
            Saxon.Hej.om.NodeInfo n = this;
            if ((getNodeKind() != Saxon.Hej.type.Type.ELEMENT)) {
                n = getParent();
            }
            while ((n != null)) {
                string xmlbase = n.getAttributeValue(Saxon.Hej.om.NamespaceUri.XML, "base");
                if ((xmlbase != null)) {
                    return xmlbase;
                }
                n = n.getParent();
            }
            return getRoot().getSystemId();
        }
        public int getLineNumber() {
            return -1;
        }
        public int getColumnNumber() {
            return -1;
        }
        public Saxon.Hej.s9api.Location saveLocation() {
            return this;
        }
        public virtual  /* REPLACING public */ string getDisplayName() {
            string prefix = getPrefix();
            string local = getLocalPart();
            if (prefix.Length==0) {
                return local;
            } else {
                return ((prefix+":")+local);
            }
        }
        public virtual  /* REPLACING public */ string getAttributeValue(Saxon.Hej.om.NamespaceUri uri, string local) {
            return null;
        }
        public virtual  /* REPLACING public */ Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber) {
            return iterateAxis(axisNumber, Saxon.Hej.pattern.AnyNodeTest.getInstance());
        }
        public virtual  /* REPLACING public */ Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber, Saxon.Hej.pattern.NodePredicate predicate) {
            Saxon.Hej.pattern.NodeTest nodeTest = Saxon.Hej.tree.util.Navigator.nodeTestFromPredicate(predicate);
            int nodeKind = getNodeKind();
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
              case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
                if ((nodeKind != Saxon.Hej.type.Type.ELEMENT)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                }
                return iterateAttributes(nodeTest);
              case Saxon.Hej.om.AxisInfo.CHILD:
                if (((nodeKind == Saxon.Hej.type.Type.ELEMENT) || (nodeKind == Saxon.Hej.type.Type.DOCUMENT))) {
                    return iterateChildren(nodeTest);
                } else {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                }
              case Saxon.Hej.om.AxisInfo.DESCENDANT:
                if (((nodeKind == Saxon.Hej.type.Type.ELEMENT) || (nodeKind == Saxon.Hej.type.Type.DOCUMENT))) {
                    return iterateDescendants(nodeTest, false);
                } else {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                }
              case Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF:
                if (((nodeKind == Saxon.Hej.type.Type.ELEMENT) || (nodeKind == Saxon.Hej.type.Type.DOCUMENT))) {
                    return iterateDescendants(nodeTest, true);
                } else {
                    return Saxon.Hej.tree.util.Navigator.filteredSingleton(this, nodeTest);
                }
              case Saxon.Hej.om.AxisInfo.FOLLOWING:
                return new Saxon.Hej.tree.util.Navigator.AxisFilter(new Saxon.Hej.tree.util.Navigator.FollowingEnumeration(this), nodeTest);
              case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
                switch (nodeKind) {
                  case Saxon.Hej.type.Type.DOCUMENT:
                  case Saxon.Hej.type.Type.ATTRIBUTE:
                  case Saxon.Hej.type.Type.NAMESPACE:
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                  default:
                    return iterateSiblings(nodeTest, true);
                }
              case Saxon.Hej.om.AxisInfo.NAMESPACE:
                if ((nodeKind != Saxon.Hej.type.Type.ELEMENT)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                }
                return Saxon.Hej.tree.NamespaceNode.makeIterator(this, nodeTest);
              case Saxon.Hej.om.AxisInfo.PARENT:
                return Saxon.Hej.tree.util.Navigator.filteredSingleton(getParent(), nodeTest);
              case Saxon.Hej.om.AxisInfo.PRECEDING:
                return new Saxon.Hej.tree.util.Navigator.AxisFilter(new Saxon.Hej.tree.util.Navigator.PrecedingEnumeration(this, false), nodeTest);
              case Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING:
                switch (nodeKind) {
                  case Saxon.Hej.type.Type.DOCUMENT:
                  case Saxon.Hej.type.Type.ATTRIBUTE:
                  case Saxon.Hej.type.Type.NAMESPACE:
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                  default:
                    return iterateSiblings(nodeTest, false);
                }
              case Saxon.Hej.om.AxisInfo.SELF:
                return Saxon.Hej.tree.util.Navigator.filteredSingleton(this, nodeTest);
              case Saxon.Hej.om.AxisInfo.PRECEDING_OR_ANCESTOR:
                return new Saxon.Hej.tree.util.Navigator.AxisFilter(new Saxon.Hej.tree.util.Navigator.PrecedingEnumeration(this, true), nodeTest);
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown axis number "+axisNumber));
            }
        }
        protected internal abstract Saxon.Hej.tree.iter.AxisIterator iterateAttributes(Saxon.Hej.pattern.NodeTest nodeTest);
        protected internal abstract Saxon.Hej.tree.iter.AxisIterator iterateChildren(Saxon.Hej.pattern.NodeTest nodeTest);
        protected internal abstract Saxon.Hej.tree.iter.AxisIterator iterateSiblings(Saxon.Hej.pattern.NodeTest nodeTest, bool forwards);
        protected virtual  /* REPLACING protected internal */ Saxon.Hej.tree.iter.AxisIterator iterateDescendants(Saxon.Hej.pattern.NodeTest nodeTest, bool includeSelf) {
            Saxon.Hej.tree.iter.AxisIterator iter = new Saxon.Hej.tree.util.Navigator.DescendantEnumeration(this, includeSelf, true);
            if (!(((nodeTest is Saxon.Hej.pattern.AnyNodeTest)))) {
                iter = new Saxon.Hej.tree.util.Navigator.AxisFilter(iter, nodeTest);
            }
            return iter;
        }
        public virtual  /* REPLACING public */ Saxon.Hej.om.NamespaceBinding[] getDeclaredNamespaces(Saxon.Hej.om.NamespaceBinding[] buffer) {
            return new Saxon.Hej.om.NamespaceBinding[0];
        }
        public virtual  /* REPLACING public */ Saxon.Hej.om.NamespaceMap getAllNamespaces() {
            if ((getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                throw new Saxon.Ejava.lang.AssertionError(("getAllNamespaces() not implemented for "+this.GetType()));
            }
            return null;
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
        public virtual  /* REPLACING public */ void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            Saxon.Hej.tree.util.Navigator.copy(this, @out, copyOptions, locationId);
        }
        public virtual  /* REPLACING public */ bool hasChildNodes() {
            switch (getNodeKind()) {
              case Saxon.Hej.type.Type.DOCUMENT:
              case Saxon.Hej.type.Type.ELEMENT:
                return (((/*1430*/Saxon.Hej.om.NodeInfo)iterateAxis(Saxon.Hej.om.AxisInfo.CHILD).next()) != null);
              default:
                return false;
            }
        }
        public virtual  /* REPLACING public */ int getFingerprint() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public virtual  /* REPLACING public */ bool hasFingerprint() {
            return false;
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

        /* Expanding default method getURI from interface NodeInfo */
        public virtual string getURI() {
            return getNamespaceUri().ToString();
        }

        /* Expanding default method getSchemaType from interface NodeInfo */
        public virtual Saxon.Hej.type.SchemaType getSchemaType() {
            switch (getNodeKind()) {
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
              case Saxon.Hej.type.Type.DOCUMENT:
              case Saxon.Hej.type.Type.ELEMENT:
                return Saxon.Hej.type.Untyped.getInstance();
              default:
                return null;
            }
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
            return (Saxon.Hej.tree.iter.SingletonIterator)(Saxon.Hej.tree.iter.SingletonIterator.makeIterator(this));
        }

        /* Creating stub method getUnderlyingNode to satisfy interface VirtualNode */
        public virtual System.Object getUnderlyingNode() {
            throw new System.NotImplementedException();
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

        /* Creating stub method compareOrder to satisfy interface NodeInfo */
        public virtual int compareOrder(Saxon.Hej.om.NodeInfo arg1) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getLocalPart to satisfy interface NodeInfo */
        public virtual string getLocalPart() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getNamespaceUri to satisfy interface NodeInfo */
        public virtual Saxon.Hej.om.NamespaceUri getNamespaceUri() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getPrefix to satisfy interface NodeInfo */
        public virtual string getPrefix() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getParent to satisfy interface NodeInfo */
        public virtual Saxon.Hej.om.NodeInfo getParent() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method generateId to satisfy interface NodeInfo */
        public virtual void generateId(Saxon.Ejava.lang.StringBuilder arg1) {
            throw new System.NotImplementedException();
        }

    }
}