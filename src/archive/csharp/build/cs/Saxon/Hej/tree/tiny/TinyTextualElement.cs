using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    public class TinyTextualElement : Saxon.Hej.tree.tiny.TinyElementImpl {
        private Saxon.Hej.tree.tiny.TinyTextualElement.TinyTextualElementText textNode = null;
        public TinyTextualElement(Saxon.Hej.tree.tiny.TinyTree tree, int nodeNr) : base (tree, nodeNr) {
        }
        public override Saxon.Hej.om.NamespaceBinding[] getDeclaredNamespaces(Saxon.Hej.om.NamespaceBinding[] buffer) {
            return Saxon.Hej.om.NamespaceBinding.EMPTY_ARRAY;
        }
        public override Saxon.Hej.om.NamespaceMap getAllNamespaces() {
            Saxon.Hej.tree.tiny.TinyNodeImpl parent = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)getParent());
            if ((parent is Saxon.Hej.tree.tiny.TinyElementImpl)) {
                return parent.getAllNamespaces();
            } else {
                return Saxon.Hej.om.NamespaceMap.emptyMap();
            }
        }
        public override string getAttributeValue(string uri, string local) {
            return null;
        }
        public override string getAttributeValue(int fp) {
            return null;
        }
        public override void copy(Saxon.Hej.@event.Receiver receiver, int copyOptions, Saxon.Hej.s9api.Location location) {
            bool typed = Saxon.Hej.om.CopyOptions.includes(copyOptions, Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS);
            Saxon.Hej.type.SchemaType type = (typed ? getSchemaType() : Saxon.Hej.type.Untyped.getInstance());
            bool disallowNamespaceSensitiveContent = (((((copyOptions&Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS)) != 0)) && ((((copyOptions&Saxon.Hej.om.CopyOptions.ALL_NAMESPACES)) == 0)));
            if (disallowNamespaceSensitiveContent) {
                try {
                    checkNotNamespaceSensitiveElement(type, nodeNr);
                } catch (Saxon.Hej.@event.CopyNamespaceSensitiveException e) {
                    e.setErrorCode((receiver.getPipelineConfiguration().isXSLT() ? "XTTE0950" : "XQTY0086"));
                    throw;
                }
            }
            System.Func<Saxon.Hej.om.NodeInfo,System.Object> informee = receiver.getPipelineConfiguration().getCopyInformee();
            if ((informee != null)) {
                System.Object o = informee(this);
                if ((o is Saxon.Hej.s9api.Location)) {
                    location = (Saxon.Hej.s9api.Location)(o);
                }
            }
            Saxon.Hej.om.NamespaceMap namespaces;
            if ((((copyOptions&Saxon.Hej.om.CopyOptions.ALL_NAMESPACES)) != 0)) {
                namespaces = getAllNamespaces();
            } else {
                namespaces = Saxon.Hej.om.NamespaceMap.emptyMap();
            }
            receiver.startElement(Saxon.Hej.om.NameOfNode.makeName(this), type, Saxon.Hej.om.EmptyAttributeMap.getInstance(), namespaces, location, Saxon.Hej.@event.ReceiverOption.NONE);
            receiver.characters(getUnicodeStringValue(), location, Saxon.Hej.@event.ReceiverOption.NONE);
            receiver.endElement();
        }
        public override bool hasChildNodes() {
            return true;
        }
        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return Saxon.Hej.tree.tiny.TinyTextImpl.getStringValue(tree, nodeNr);
        }
        public override Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber) {
            switch (axisNumber) {
              case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
                return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
              case Saxon.Hej.om.AxisInfo.CHILD:
              case Saxon.Hej.om.AxisInfo.DESCENDANT:
                return Saxon.Hej.tree.iter.SingleNodeIterator.makeIterator(getTextNode());
              case Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF:
                return new Saxon.Hej.tree.iter.ArrayIterator.OfNodes<Saxon.Hej.om.NodeInfo>(new Saxon.Hej.om.NodeInfo[]{this, getTextNode()});
              default:
                return base.iterateAxis(axisNumber);
            }
        }
        public override Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber, Saxon.Hej.pattern.NodePredicate nodeTest) {
            switch (axisNumber) {
              case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
                return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
              case Saxon.Hej.om.AxisInfo.CHILD:
              case Saxon.Hej.om.AxisInfo.DESCENDANT:
                return Saxon.Hej.tree.util.Navigator.filteredSingleton(getTextNode(), nodeTest);
              case Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF:
                System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> list = new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>(2);
                if (nodeTest.test(this)) {
                    list.Add(this);
                }
                if (nodeTest.test(getTextNode())) {
                    list.Add(getTextNode());
                }
                return new Saxon.Hej.tree.iter.NodeListIterator(list);
              default:
                return base.iterateAxis(axisNumber, nodeTest);
            }
        }
        public override bool isAncestorOrSelf(Saxon.Hej.tree.tiny.TinyNodeImpl d) {
            return this.Equals(d);
        }
        public Saxon.Hej.tree.tiny.TinyTextualElement.TinyTextualElementText getTextNode() {
            if ((textNode == null)) {
                textNode = new Saxon.Hej.tree.tiny.TinyTextualElement.TinyTextualElementText(this);
            }
            return textNode;
        }
        public class TinyTextualElementText : Saxon.Hej.om.NodeInfo, Saxon.Ejavax.xml.transform.SourceLocator {
            private readonly Saxon.Hej.tree.tiny.TinyTextualElement element;
            public TinyTextualElementText(Saxon.Hej.tree.tiny.TinyTextualElement element)  {
                this.element = element;
            }
            public bool hasFingerprint() {
                return true;
            }
            public Saxon.Hej.om.TreeInfo getTreeInfo() {
                return element.getTreeInfo();
            }
            public void setSystemId(string systemId) {
            }
            public int getNodeKind() {
                return Saxon.Hej.type.Type.TEXT;
            }
            public Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
                return element.getUnicodeStringValue();
            }
            public override bool Equals(System.Object other) {
                return ((other is Saxon.Hej.tree.tiny.TinyTextualElement.TinyTextualElementText) && getParent().Equals(((Saxon.Hej.tree.tiny.TinyTextualElement.TinyTextualElementText)(other)).getParent()));
            }
            public override int GetHashCode() {
                return (getParent().GetHashCode()^0x01010101);
            }
            public void generateId(Saxon.Ejava.lang.StringBuilder buffer) {
                element.generateId(buffer);
                buffer.append("T");
            }
            public string getSystemId() {
                return element.getSystemId();
            }
            public string getBaseURI() {
                return element.getBaseURI();
            }
            public int compareOrder(Saxon.Hej.om.NodeInfo other) {
                if (other.Equals(this)) {
                    return 0;
                } else {
                    if (other.Equals(getParent())) {
                        return 1;
                    } else {
                        return getParent().compareOrder(other);
                    }
                }
            }
            public int getFingerprint() {
                return -1;
            }
            public string getPrefix() {
                return "";
            }
            public string getURI() {
                return "";
            }
            public string getDisplayName() {
                return "";
            }
            public string getLocalPart() {
                return "";
            }
            public bool hasChildNodes() {
                return false;
            }
            public string getAttributeValue(string uri, string local) {
                return null;
            }
            public int getLineNumber() {
                return getParent().getLineNumber();
            }
            public int getColumnNumber() {
                return getParent().getColumnNumber();
            }
            public Saxon.Hej.s9api.Location saveLocation() {
                return this;
            }
            public Saxon.Hej.type.SchemaType getSchemaType() {
                return null;
            }
            public Saxon.Hej.om.NamespaceBinding[] getDeclaredNamespaces(Saxon.Hej.om.NamespaceBinding[] buffer) {
                return null;
            }
            public Saxon.Hej.om.NamespaceMap getAllNamespaces() {
                return null;
            }
            public Saxon.Hej.om.AtomicSequence atomize() {
                return Saxon.Hej.value.StringValue.makeUntypedAtomic(getUnicodeStringValue());
            }
            public Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber) {
                switch (axisNumber) {
                  case Saxon.Hej.om.AxisInfo.ANCESTOR:
                    return element.iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF);
                  case Saxon.Hej.om.AxisInfo.PRECEDING_OR_ANCESTOR:
                    return new Saxon.Hej.tree.util.Navigator.PrecedingEnumeration(this, true);
                  case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
                    return new Saxon.Hej.tree.iter.PrependAxisIterator(this, getParent().iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF));
                  case Saxon.Hej.om.AxisInfo.FOLLOWING:
                    return new Saxon.Hej.tree.util.Navigator.FollowingEnumeration(this);
                  case Saxon.Hej.om.AxisInfo.PRECEDING:
                    return new Saxon.Hej.tree.util.Navigator.PrecedingEnumeration(this, false);
                  case Saxon.Hej.om.AxisInfo.PARENT:
                    return Saxon.Hej.tree.iter.SingleNodeIterator.makeIterator(getParent());
                  case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
                  case Saxon.Hej.om.AxisInfo.CHILD:
                  case Saxon.Hej.om.AxisInfo.DESCENDANT:
                  case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
                  case Saxon.Hej.om.AxisInfo.NAMESPACE:
                  case Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING:
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                  case Saxon.Hej.om.AxisInfo.SELF:
                  case Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF:
                    return Saxon.Hej.tree.iter.SingleNodeIterator.makeIterator(this);
                  default:
                    throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown axis number "+axisNumber));
                }
            }
            public Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber, Saxon.Hej.pattern.NodePredicate predicate) {
                Saxon.Hej.pattern.NodeTest nodeTest = Saxon.Hej.tree.util.Navigator.nodeTestFromPredicate(predicate);
                switch (axisNumber) {
                  case Saxon.Hej.om.AxisInfo.ANCESTOR:
                    return getParent().iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF, nodeTest);
                  case Saxon.Hej.om.AxisInfo.PRECEDING_OR_ANCESTOR:
                    return new Saxon.Hej.tree.util.Navigator.AxisFilter(new Saxon.Hej.tree.util.Navigator.PrecedingEnumeration(this, true), nodeTest);
                  case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
                    return new Saxon.Hej.tree.util.Navigator.AxisFilter(new Saxon.Hej.tree.iter.PrependAxisIterator(this, getParent().iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF)), nodeTest);
                  case Saxon.Hej.om.AxisInfo.FOLLOWING:
                    return new Saxon.Hej.tree.util.Navigator.AxisFilter(new Saxon.Hej.tree.util.Navigator.FollowingEnumeration(this), nodeTest);
                  case Saxon.Hej.om.AxisInfo.PRECEDING:
                    return new Saxon.Hej.tree.util.Navigator.AxisFilter(new Saxon.Hej.tree.util.Navigator.PrecedingEnumeration(this, false), nodeTest);
                  case Saxon.Hej.om.AxisInfo.PARENT:
                    return Saxon.Hej.tree.util.Navigator.filteredSingleton(getParent(), nodeTest);
                  case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
                  case Saxon.Hej.om.AxisInfo.CHILD:
                  case Saxon.Hej.om.AxisInfo.DESCENDANT:
                  case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
                  case Saxon.Hej.om.AxisInfo.NAMESPACE:
                  case Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING:
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                  case Saxon.Hej.om.AxisInfo.SELF:
                  case Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF:
                    return Saxon.Hej.tree.util.Navigator.filteredSingleton(this, nodeTest);
                  default:
                    throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown axis number "+axisNumber));
                }
            }
            public Saxon.Hej.om.NodeInfo getParent() {
                return element;
            }
            public Saxon.Hej.om.NodeInfo getRoot() {
                return element.getRoot();
            }
            public void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
                @out.characters(getUnicodeStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
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
                return new Saxon.Hej.tree.iter.SingletonIterator(this);
            }

        }
    
    }
}