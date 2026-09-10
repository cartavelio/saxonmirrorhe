using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree {
    public class NamespaceNode : Saxon.Hej.om.NodeInfo {
        internal Saxon.Hej.om.NodeInfo element;
        internal Saxon.Hej.om.NamespaceBinding nsBinding;
        internal int position;
        internal int fingerprint;
        public NamespaceNode(Saxon.Hej.om.NodeInfo element, Saxon.Hej.om.NamespaceBinding nscode, int position)  {
            this.element = element;
            this.nsBinding = nscode;
            this.position = position;
            fingerprint = -1;
        }
        public Saxon.Hej.om.TreeInfo getTreeInfo() {
            return element.getTreeInfo();
        }
        public Saxon.Hej.om.Item head() {
            return this;
        }
        public int getNodeKind() {
            return Saxon.Hej.type.Type.NAMESPACE;
        }
        public override bool Equals(System.Object other) {
            return (((other is Saxon.Hej.tree.NamespaceNode) && element.Equals(((Saxon.Hej.tree.NamespaceNode)(other)).element)) && nsBinding.Equals(((Saxon.Hej.tree.NamespaceNode)(other)).nsBinding));
        }
        public override int GetHashCode() {
            return (element.GetHashCode()^((position<<13)));
        }
        public string getSystemId() {
            return element.getSystemId();
        }
        public string getPublicId() {
            return element.getPublicId();
        }
        public string getBaseURI() {
            return null;
        }
        public int getLineNumber() {
            return element.getLineNumber();
        }
        public int getColumnNumber() {
            return element.getColumnNumber();
        }
        public Saxon.Hej.s9api.Location saveLocation() {
            return this;
        }
        public int compareOrder(Saxon.Hej.om.NodeInfo other) {
            if (((other is Saxon.Hej.tree.NamespaceNode) && element.Equals(((Saxon.Hej.tree.NamespaceNode)(other)).element))) {
                int c = (position-((Saxon.Hej.tree.NamespaceNode)(other)).position);
                return c.CompareTo(0);
            } else {
                if (element.Equals(other)) {
                    return +(1);
                } else {
                    return element.compareOrder(other);
                }
            }
        }
        public Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return nsBinding.getNamespaceUri().toUnicodeString();
        }
        public string getStringValue() {
            return nsBinding.getNamespaceUri().ToString();
        }
        public bool hasFingerprint() {
            return true;
        }
        public int getFingerprint() {
            if ((fingerprint == -1)) {
                if (nsBinding.getPrefix().Length==0) {
                    return -1;
                } else {
                    fingerprint = element.getConfiguration().getNamePool().allocateFingerprint(Saxon.Hej.om.NamespaceUri.NULL, nsBinding.getPrefix());
                }
            }
            return fingerprint;
        }
        public string getLocalPart() {
            return nsBinding.getPrefix();
        }
        public Saxon.Hej.om.NamespaceUri getNamespaceUri() {
            return Saxon.Hej.om.NamespaceUri.NULL;
        }
        public string getDisplayName() {
            return getLocalPart();
        }
        public string getPrefix() {
            return "";
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return element.getConfiguration();
        }
        public Saxon.Hej.om.NamePool getNamePool() {
            return getConfiguration().getNamePool();
        }
        public Saxon.Hej.type.SchemaType getSchemaType() {
            return Saxon.Hej.type.BuiltInAtomicType.STRING;
        }
        public Saxon.Hej.om.NodeInfo getParent() {
            return element;
        }
        public Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber, Saxon.Hej.pattern.NodePredicate predicate) {
            Saxon.Hej.pattern.NodeTest nodeTest = Saxon.Hej.tree.util.Navigator.nodeTestFromPredicate(predicate);
            switch (axisNumber) {
              case Saxon.Hej.om.AxisInfo.ANCESTOR:
                return element.iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF, nodeTest);
              case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
                if (nodeTest.test(this)) {
                    return new Saxon.Hej.tree.iter.PrependAxisIterator(this, element.iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF, nodeTest));
                } else {
                    return element.iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF, nodeTest);
                }
              case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
              case Saxon.Hej.om.AxisInfo.CHILD:
              case Saxon.Hej.om.AxisInfo.DESCENDANT:
              case Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF:
              case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
              case Saxon.Hej.om.AxisInfo.NAMESPACE:
              case Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING:
                return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
              case Saxon.Hej.om.AxisInfo.FOLLOWING:
                return new Saxon.Hej.tree.util.Navigator.AxisFilter(new Saxon.Hej.tree.util.Navigator.FollowingEnumeration(this), nodeTest);
              case Saxon.Hej.om.AxisInfo.PARENT:
                return Saxon.Hej.tree.util.Navigator.filteredSingleton(element, nodeTest);
              case Saxon.Hej.om.AxisInfo.PRECEDING:
                return new Saxon.Hej.tree.util.Navigator.AxisFilter(new Saxon.Hej.tree.util.Navigator.PrecedingEnumeration(this, false), nodeTest);
              case Saxon.Hej.om.AxisInfo.SELF:
                return Saxon.Hej.tree.util.Navigator.filteredSingleton(this, nodeTest);
              case Saxon.Hej.om.AxisInfo.PRECEDING_OR_ANCESTOR:
                return new Saxon.Hej.tree.util.Navigator.AxisFilter(new Saxon.Hej.tree.util.Navigator.PrecedingEnumeration(this, true), nodeTest);
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown axis number "+axisNumber));
            }
        }
        public string getAttributeValue(Saxon.Hej.om.NamespaceUri uri, string local) {
            return null;
        }
        public Saxon.Hej.om.NodeInfo getRoot() {
            return element.getRoot();
        }
        public bool hasChildNodes() {
            return false;
        }
        public void generateId(Saxon.Ejava.lang.StringBuilder buffer) {
            element.generateId(buffer);
            buffer.append("n");
            buffer.append(position);
        }
        public void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            @out.append(this);
        }
        public Saxon.Hej.om.NamespaceBinding[] getDeclaredNamespaces(Saxon.Hej.om.NamespaceBinding[] buffer) {
            return null;
        }
        public Saxon.Hej.om.NamespaceMap getAllNamespaces() {
            return null;
        }
        public void setSystemId(string systemId) {
        }
        public Saxon.Hej.om.AtomicSequence atomize() {
            return new Saxon.Hej.value.StringValue(getStringValue());
        }
        public bool isStreamed() {
            return element.isStreamed();
        }
        public static Saxon.Hej.tree.iter.AxisIterator makeIterator(Saxon.Hej.om.NodeInfo element, Saxon.Hej.pattern.NodePredicate test) {
            System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> nodes = new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>();
            int position = 0;
            bool foundXML = false;
            foreach (Saxon.Hej.om.NamespaceBinding binding in element.getAllNamespaces()) {
                if (binding.getPrefix().Equals("xml")) {
                    foundXML = true;
                }
                Saxon.Hej.tree.NamespaceNode node = new Saxon.Hej.tree.NamespaceNode(element, binding, position++);
                if (test.test(node)) {
                    nodes.Add(node);
                }
            }
            if (!(foundXML)) {
                Saxon.Hej.tree.NamespaceNode node = new Saxon.Hej.tree.NamespaceNode(element, Saxon.Hej.om.NamespaceBinding.XML, position);
                if (test.test(node)) {
                    nodes.Add(node);
                }
            }
            return new Saxon.Hej.tree.iter.NodeListIterator(nodes);
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