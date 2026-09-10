using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public sealed class TextFragmentValue : Saxon.Hej.om.NodeInfo, Saxon.Ejavax.xml.transform.SourceLocator {
        private readonly Saxon.Hej.str.UnicodeString text;
        private readonly string baseURI;
        private string documentURI;
        private readonly Saxon.Hej.om.GenericTreeInfo treeInfo;
        private Saxon.Hej.value.TextFragmentValue.TextFragmentTextNode textNode = null;
        public TextFragmentValue(Saxon.Hej.Configuration config, Saxon.Hej.str.UnicodeString value, string baseURI)  {
            this.text = value;
            this.baseURI = baseURI;
            this.treeInfo = new Saxon.Hej.om.GenericTreeInfo(config);
            this.treeInfo.setRootNode(this);
        }
        public static Saxon.Hej.om.NodeInfo makeTextFragment(Saxon.Hej.Configuration config, Saxon.Hej.str.UnicodeString value, string baseURI) {
            if ((value.length() == 0)) {
                Saxon.Hej.tree.linked.DocumentImpl doc = new Saxon.Hej.tree.linked.DocumentImpl();
                doc.setSystemId(baseURI);
                doc.setBaseURI(baseURI);
                doc.setConfiguration(config);
                return doc;
            } else {
                return new Saxon.Hej.value.TextFragmentValue(config, value, baseURI);
            }
        }
        public Saxon.Hej.om.TreeInfo getTreeInfo() {
            return treeInfo;
        }
        public Saxon.Hej.om.NodeInfo getRootNode() {
            return this;
        }
        public bool isTyped() {
            return false;
        }
        public int getNodeKind() {
            return Saxon.Hej.type.Type.DOCUMENT;
        }
        public Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return text;
        }
        public override bool Equals(System.Object other) {
            return (this == other);
        }
        public override int GetHashCode() {
            return Saxon.Ejava.lang.System.identityHashCode(this);
        }
        public bool hasFingerprint() {
            return true;
        }
        public void generateId(Saxon.Ejava.lang.StringBuilder buffer) {
            buffer.append("tt");
            buffer.append(treeInfo.getDocumentNumber());
        }
        public void setSystemId(string systemId) {
            documentURI = systemId;
        }
        public string getSystemId() {
            return documentURI;
        }
        public string getBaseURI() {
            return baseURI;
        }
        public int compareOrder(Saxon.Hej.om.NodeInfo other) {
            if ((this == other)) {
                return 0;
            }
            return -1;
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
            return (text.length() != 0);
        }
        public Saxon.Hej.s9api.Location saveLocation() {
            return this;
        }
        public Saxon.Hej.type.SchemaType getSchemaType() {
            return Saxon.Hej.type.Untyped.getInstance();
        }
        public Saxon.Hej.om.NamespaceBinding[] getDeclaredNamespaces(Saxon.Hej.om.NamespaceBinding[] buffer) {
            return null;
        }
        public Saxon.Hej.om.NamespaceMap getAllNamespaces() {
            return null;
        }
        public Saxon.Hej.om.AtomicSequence atomize() {
            return Saxon.Hej.value.StringValue.makeUntypedAtomic(text);
        }
        public string getAttributeValue(string uri, string local) {
            return null;
        }
        public Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber) {
            switch (axisNumber) {
              case Saxon.Hej.om.AxisInfo.ANCESTOR:
              case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
              case Saxon.Hej.om.AxisInfo.FOLLOWING:
              case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
              case Saxon.Hej.om.AxisInfo.NAMESPACE:
              case Saxon.Hej.om.AxisInfo.PARENT:
              case Saxon.Hej.om.AxisInfo.PRECEDING:
              case Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING:
              case Saxon.Hej.om.AxisInfo.PRECEDING_OR_ANCESTOR:
                return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
              case Saxon.Hej.om.AxisInfo.SELF:
              case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
                return Saxon.Hej.tree.iter.SingleNodeIterator.makeIterator(this);
              case Saxon.Hej.om.AxisInfo.CHILD:
              case Saxon.Hej.om.AxisInfo.DESCENDANT:
                return Saxon.Hej.tree.iter.SingleNodeIterator.makeIterator(getTextNode());
              case Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF:
                Saxon.Hej.om.NodeInfo[] nodes = {this, getTextNode()};
                return new Saxon.Hej.tree.iter.ArrayIterator.OfNodes<Saxon.Hej.om.NodeInfo>(nodes);
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown axis number "+axisNumber));
            }
        }
        public Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber, Saxon.Hej.pattern.NodePredicate nodeTest) {
            switch (axisNumber) {
              case Saxon.Hej.om.AxisInfo.ANCESTOR:
              case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
              case Saxon.Hej.om.AxisInfo.FOLLOWING:
              case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
              case Saxon.Hej.om.AxisInfo.NAMESPACE:
              case Saxon.Hej.om.AxisInfo.PARENT:
              case Saxon.Hej.om.AxisInfo.PRECEDING:
              case Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING:
              case Saxon.Hej.om.AxisInfo.PRECEDING_OR_ANCESTOR:
                return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
              case Saxon.Hej.om.AxisInfo.SELF:
              case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
                return Saxon.Hej.tree.util.Navigator.filteredSingleton(this, nodeTest);
              case Saxon.Hej.om.AxisInfo.CHILD:
              case Saxon.Hej.om.AxisInfo.DESCENDANT:
                return Saxon.Hej.tree.util.Navigator.filteredSingleton(getTextNode(), nodeTest);
              case Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF:
                bool b1 = nodeTest.test(this);
                Saxon.Hej.om.NodeInfo textNode2 = getTextNode();
                bool b2 = nodeTest.test(textNode2);
                if (b1) {
                    if (b2) {
                        Saxon.Hej.om.NodeInfo[] pair = {this, textNode2};
                        return new Saxon.Hej.tree.iter.ArrayIterator.OfNodes<Saxon.Hej.om.NodeInfo>(pair);
                    } else {
                        return Saxon.Hej.tree.iter.SingleNodeIterator.makeIterator(this);
                    }
                } else {
                    if (b2) {
                        return Saxon.Hej.tree.iter.SingleNodeIterator.makeIterator(textNode2);
                    } else {
                        return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                    }
                }
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown axis number "+axisNumber));
            }
        }
        public Saxon.Hej.om.NodeInfo getParent() {
            return null;
        }
        public Saxon.Hej.om.NodeInfo getRoot() {
            return this;
        }
        public void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            @out.characters(text, locationId, Saxon.Hej.@event.ReceiverOption.NONE);
        }
        public Saxon.Hej.om.NodeInfo selectID(string id, bool getParent) {
            return null;
        }
        public System.Collections.Generic.IEnumerator<string> getUnparsedEntityNames() {
            return new System.Collections.Generic.List<string>().GetEnumerator();;
        }
        public string[] getUnparsedEntity(string name) {
            return null;
        }
        private Saxon.Hej.value.TextFragmentValue.TextFragmentTextNode getTextNode() {
            if ((textNode == null)) {
                textNode = new Saxon.Hej.value.TextFragmentValue.TextFragmentTextNode(this);
            }
            return textNode;
        }
        private class TextFragmentTextNode : Saxon.Hej.om.NodeInfo, Saxon.Ejavax.xml.transform.SourceLocator {
            private readonly Saxon.Hej.value.TextFragmentValue fragment;
            public TextFragmentTextNode(Saxon.Hej.value.TextFragmentValue fragment)  {
                this.fragment = fragment;
            }
            public bool hasFingerprint() {
                return true;
            }
            public Saxon.Hej.om.TreeInfo getTreeInfo() {
                return fragment.treeInfo;
            }
            public void setSystemId(string systemId) {
            }
            public int getNodeKind() {
                return Saxon.Hej.type.Type.TEXT;
            }
            public Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
                return fragment.text;
            }
            public bool Equals(Saxon.Hej.om.NodeInfo other) {
                return (this == other);
            }
            public void generateId(Saxon.Ejava.lang.StringBuilder buffer) {
                buffer.append("tt");
                buffer.append(fragment.treeInfo.getDocumentNumber());
                buffer.append("t1");
            }
            public string getSystemId() {
                return null;
            }
            public string getBaseURI() {
                return fragment.baseURI;
            }
            public int compareOrder(Saxon.Hej.om.NodeInfo other) {
                if ((this == other)) {
                    return 0;
                }
                return +(1);
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
                return Saxon.Hej.value.StringValue.makeUntypedAtomic(fragment.text);
            }
            public Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber) {
                switch (axisNumber) {
                  case Saxon.Hej.om.AxisInfo.ANCESTOR:
                  case Saxon.Hej.om.AxisInfo.PARENT:
                  case Saxon.Hej.om.AxisInfo.PRECEDING_OR_ANCESTOR:
                    return Saxon.Hej.tree.iter.SingleNodeIterator.makeIterator(fragment);
                  case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
                    Saxon.Hej.om.NodeInfo[] nodes = {this, fragment};
                    return new Saxon.Hej.tree.iter.ArrayIterator.OfNodes<Saxon.Hej.om.NodeInfo>(nodes);
                  case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
                  case Saxon.Hej.om.AxisInfo.CHILD:
                  case Saxon.Hej.om.AxisInfo.DESCENDANT:
                  case Saxon.Hej.om.AxisInfo.FOLLOWING:
                  case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
                  case Saxon.Hej.om.AxisInfo.NAMESPACE:
                  case Saxon.Hej.om.AxisInfo.PRECEDING:
                  case Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING:
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                  case Saxon.Hej.om.AxisInfo.SELF:
                  case Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF:
                    return Saxon.Hej.tree.iter.SingleNodeIterator.makeIterator(this);
                  default:
                    throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown axis number "+axisNumber));
                }
            }
            public Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber, Saxon.Hej.pattern.NodePredicate nodeTest) {
                switch (axisNumber) {
                  case Saxon.Hej.om.AxisInfo.ANCESTOR:
                  case Saxon.Hej.om.AxisInfo.PARENT:
                  case Saxon.Hej.om.AxisInfo.PRECEDING_OR_ANCESTOR:
                    return Saxon.Hej.tree.util.Navigator.filteredSingleton(fragment, nodeTest);
                  case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
                    bool matchesDoc = nodeTest.test(fragment);
                    bool matchesText = nodeTest.test(this);
                    if ((matchesDoc && matchesText)) {
                        Saxon.Hej.om.NodeInfo[] nodes = {this, fragment};
                        return new Saxon.Hej.tree.iter.ArrayIterator.OfNodes<Saxon.Hej.om.NodeInfo>(nodes);
                    } else {
                        if (matchesDoc) {
                            return Saxon.Hej.tree.iter.SingleNodeIterator.makeIterator(fragment);
                        } else {
                            if (matchesText) {
                                return Saxon.Hej.tree.iter.SingleNodeIterator.makeIterator(this);
                            } else {
                                return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                            }
                        }
                    }
                  case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
                  case Saxon.Hej.om.AxisInfo.CHILD:
                  case Saxon.Hej.om.AxisInfo.DESCENDANT:
                  case Saxon.Hej.om.AxisInfo.FOLLOWING:
                  case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
                  case Saxon.Hej.om.AxisInfo.NAMESPACE:
                  case Saxon.Hej.om.AxisInfo.PRECEDING:
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
                return fragment;
            }
            public Saxon.Hej.om.NodeInfo getRoot() {
                return fragment;
            }
            public void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
                @out.characters(fragment.text, locationId, Saxon.Hej.@event.ReceiverOption.NONE);
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

            /* Expanding default method getLineNumber from interface NodeInfo */
            public virtual int getLineNumber() {
                return -1;
            }

            /* Expanding default method getColumnNumber from interface NodeInfo */
            public virtual int getColumnNumber() {
                return -1;
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
    
        /* Expanding default method effectiveBooleanValue from interface GroundedValue */
        public bool effectiveBooleanValue() {
            return Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(iterate());
        }

        /* Expanding default method reduce from interface Item */
        public Saxon.Hej.om.GroundedValue reduce() {
            return this;
        }

        /* Expanding default method materialize from interface Sequence */
        public Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.om.SequenceTool.toGroundedValue(iterate());
        }

        /* Expanding default method toShortString from interface Item */
        public string toShortString() {
            return ToString();
        }

        /* Expanding default method asIterable from interface GroundedValue */
        public System.Collections.Generic.IEnumerable<Saxon.Hej.om.Item> asIterable() {
            return new Saxon.Hej.om.GroundedValueAsIterable(this);
        }

        /* Expanding default method containsNode from interface GroundedValue */
        public bool containsNode(Saxon.Hej.om.NodeInfo sought) {
            return Saxon.Hej.expr.SingletonIntersectExpression.containsNode(iterate(), sought);
        }

        /* Expanding default method concatenate from interface GroundedValue */
        public Saxon.Hej.om.GroundedValue concatenate(params Saxon.Hej.om.GroundedValue[]  others) {
            Saxon.Hej.ma.zeno.ZenoSequence chain = new Saxon.Hej.ma.zeno.ZenoSequence();
            chain.appendSequence(this);
            foreach (Saxon.Hej.om.GroundedValue seq in others) {
                chain.appendSequence(seq);
            }
            return chain;
        }

        /* Expanding default method makeRepeatable from interface Sequence */
        public Saxon.Hej.om.Sequence makeRepeatable() {
            return this;
        }

        /* Expanding default method getConfiguration from interface NodeInfo */
        public Saxon.Hej.Configuration getConfiguration() {
            return getTreeInfo().getConfiguration();
        }

        /* Expanding default method isSameNodeInfo from interface NodeInfo */
        public bool isSameNodeInfo(Saxon.Hej.om.NodeInfo other) {
            return Equals(other);
        }

        /* Expanding default method getPublicId from interface NodeInfo */
        public string getPublicId() {
            return null;
        }

        /* Expanding default method getLineNumber from interface NodeInfo */
        public int getLineNumber() {
            return -1;
        }

        /* Expanding default method getColumnNumber from interface NodeInfo */
        public int getColumnNumber() {
            return -1;
        }

        /* Expanding default method children from interface NodeInfo */
        public System.Collections.Generic.IEnumerable<Saxon.Hej.om.NodeInfo> children() {
            return new Saxon.Hej.tree.util.Navigator.ChildrenAsIterable(this);
        }

        /* Expanding default method children from interface NodeInfo */
        public System.Collections.Generic.IEnumerable<Saxon.Hej.om.NodeInfo> children(Saxon.Hej.pattern.NodePredicate filter) {
            return new Saxon.Hej.tree.util.Navigator.ChildrenAsIterable(this, filter);
        }

        /* Expanding default method attributes from interface NodeInfo */
        public Saxon.Hej.om.AttributeMap attributes() {
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
        public Saxon.Hej.lib.ActiveSource asActiveSource() {
            return new Saxon.Hej.om.NodeSource(this);
        }

        /* Expanding default method isId from interface NodeInfo */
        public bool isId() {
            return false;
        }

        /* Expanding default method isIdref from interface NodeInfo */
        public bool isIdref() {
            return false;
        }

        /* Expanding default method isNilled from interface NodeInfo */
        public bool isNilled() {
            return false;
        }

        /* Expanding default method isStreamed from interface Item */
        public bool isStreamed() {
            return false;
        }

        /* Expanding default method getGenre from interface NodeInfo */
        public Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.NODE;
        }

        /* Expanding default method head from interface Item */
        public Saxon.Hej.om.Item head() {
            return this;
        }

        /* Expanding default method getStringValue from interface Item */
        public string getStringValue() {
            return getUnicodeStringValue().ToString();
        }

        /* Expanding default method itemAt from interface Item */
        public Saxon.Hej.om.Item itemAt(int n) {
            return ((n == 0) ? head() : null);
        }

        /* Expanding default method subsequence from interface Item */
        public Saxon.Hej.om.GroundedValue subsequence(int start, int length) {
            if (((start<=0) && (((start+length))>0))) {
                return this;
            } else {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        }

        /* Expanding default method getLength from interface Item */
        public int getLength() {
            return 1;
        }

        /* Expanding default method iterate from interface Item */
        public Saxon.Hej.om.SequenceIterator iterate() {
            return new Saxon.Hej.tree.iter.SingletonIterator(this);
        }

    }
}