using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    public abstract class TinyNodeImpl : Saxon.Hej.om.NodeInfo {
        protected internal Saxon.Hej.tree.tiny.TinyTree tree;
        protected internal int nodeNr;
        protected internal Saxon.Hej.tree.tiny.TinyNodeImpl parent = null;
        public Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.NODE;
        }
        public Saxon.Hej.om.TreeInfo getTreeInfo() {
            return tree;
        }
        public static readonly char[] NODE_LETTER = {'x', 'e', 'a', 't', 'x', 'x', 'x', 'p', 'c', 'r', 'x', 'x', 'x', 'n'};
        public Saxon.Hej.om.Item head() {
            return this;
        }
        public virtual Saxon.Hej.type.SchemaType getSchemaType() {
            return null;
        }
        public virtual int getColumnNumber() {
            return tree.getColumnNumber(nodeNr);
        }
        public virtual void setSystemId(string uri) {
            tree.setSystemId(nodeNr, uri);
        }
        protected internal void setParentNode(Saxon.Hej.tree.tiny.TinyNodeImpl parent) {
            this.parent = parent;
        }
        public bool isSameNodeInfo(Saxon.Hej.om.NodeInfo other) {
            return ((this == other) || (((((other is Saxon.Hej.tree.tiny.TinyNodeImpl) && (tree == ((Saxon.Hej.tree.tiny.TinyNodeImpl)(other)).tree)) && (nodeNr == ((Saxon.Hej.tree.tiny.TinyNodeImpl)(other)).nodeNr)) && (getNodeKind() == other.getNodeKind()))));
        }
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Hej.om.NodeInfo) && isSameNodeInfo((Saxon.Hej.om.NodeInfo)(other)));
        }
        public override int GetHashCode() {
            return (((((int)(((tree.getDocumentNumber()&0x3ff)))<<20))^nodeNr)^((getNodeKind()<<14)));
        }
        public virtual string getSystemId() {
            return tree.getSystemId(nodeNr);
        }
        public virtual string getBaseURI() {
            return ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)getParent()).getBaseURI();
        }
        public virtual int getLineNumber() {
            return tree.getLineNumber(nodeNr);
        }
        public Saxon.Hej.s9api.Location saveLocation() {
            return this;
        }
        protected internal virtual long getSequenceNumber() {
            return ((long)(nodeNr)<<32);
        }
        public int compareOrder(Saxon.Hej.om.NodeInfo other) {
            long a = getSequenceNumber();
            if ((other is Saxon.Hej.tree.tiny.TinyNodeImpl)) {
                long b = ((Saxon.Hej.tree.tiny.TinyNodeImpl)(other)).getSequenceNumber();
                return a.CompareTo(b);
            } else {
                return (0-other.compareOrder(this));
            }
        }
        public bool hasFingerprint() {
            return true;
        }
        public virtual int getFingerprint() {
            int nc = tree.nameCode[nodeNr];
            if ((nc == -1)) {
                return -1;
            }
            return (nc&Saxon.Hej.om.NamePool.FP_MASK);
        }
        public virtual string getPrefix() {
            int code = tree.nameCode[nodeNr];
            if ((code<0)) {
                return "";
            }
            if (!(Saxon.Hej.om.NamePool.isPrefixed(code))) {
                return "";
            }
            return tree.prefixPool.getPrefix((code>>20));
        }
        public virtual string getURI() {
            int code = tree.nameCode[nodeNr];
            if ((code<0)) {
                return "";
            }
            return tree.getNamePool().getURI((code&Saxon.Hej.om.NamePool.FP_MASK));
        }
        public virtual string getDisplayName() {
            int code = tree.nameCode[nodeNr];
            if ((code<0)) {
                return "";
            }
            if (Saxon.Hej.om.NamePool.isPrefixed(code)) {
                return ((getPrefix()+":")+getLocalPart());
            } else {
                return getLocalPart();
            }
        }
        public virtual string getLocalPart() {
            int code = tree.nameCode[nodeNr];
            if ((code<0)) {
                return "";
            }
            return tree.getNamePool().getLocalName(code);
        }
        public virtual Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber) {
            if ((axisNumber == Saxon.Hej.om.AxisInfo.CHILD)) {
                if (hasChildNodes()) {
                    return new Saxon.Hej.tree.tiny.SiblingIterator(tree, this, null, true);
                } else {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                }
            } else {
                return iterateAxis(axisNumber, Saxon.Hej.pattern.AnyNodeTest.getInstance());
            }
        }
        public virtual Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber, Saxon.Hej.pattern.NodePredicate predicate) {
            Saxon.Hej.pattern.NodeTest nodeTest = Saxon.Hej.tree.util.Navigator.nodeTestFromPredicate(predicate);
            int type = getNodeKind();
            switch (axisNumber) {
              case Saxon.Hej.om.AxisInfo.ANCESTOR:
                return new Saxon.Hej.tree.tiny.AncestorIterator(this, nodeTest);
              case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
                Saxon.Hej.tree.iter.AxisIterator ancestors = new Saxon.Hej.tree.tiny.AncestorIterator(this, nodeTest);
                if (nodeTest.test(this)) {
                    return new Saxon.Hej.tree.iter.PrependAxisIterator(this, ancestors);
                } else {
                    return ancestors;
                }
              case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
                if ((type != Saxon.Hej.type.Type.ELEMENT)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                }
                if ((tree.alpha[nodeNr]<0)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                }
                return new Saxon.Hej.tree.tiny.AttributeIterator(tree, nodeNr, nodeTest);
              case Saxon.Hej.om.AxisInfo.CHILD:
                if (hasChildNodes()) {
                    if (((nodeTest is Saxon.Hej.pattern.NameTest) && (((Saxon.Hej.pattern.NameTest)(nodeTest)).getNodeKind() == Saxon.Hej.type.Type.ELEMENT))) {
                        return new Saxon.Hej.tree.tiny.NamedChildIterator(tree, this, ((Saxon.Hej.pattern.NameTest)(nodeTest)).getFingerprint());
                    } else {
                        return new Saxon.Hej.tree.tiny.SiblingIterator(tree, this, nodeTest, true);
                    }
                } else {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                }
              case Saxon.Hej.om.AxisInfo.DESCENDANT:
                if ((((type == Saxon.Hej.type.Type.DOCUMENT) && (nodeTest is Saxon.Hej.pattern.NameTest)) && (nodeTest.getPrimitiveType() == Saxon.Hej.type.Type.ELEMENT))) {
                    return ((Saxon.Hej.tree.tiny.TinyDocumentImpl)(this)).getAllElements(nodeTest.getFingerprint());
                } else {
                    if (hasChildNodes()) {
                        if (nodeTest.getUType().overlaps(Saxon.Hej.type.UType.TEXT)) {
                            return new Saxon.Hej.tree.tiny.DescendantIterator(tree, this, nodeTest);
                        } else {
                            return new Saxon.Hej.tree.tiny.DescendantIteratorSansText(tree, this, nodeTest);
                        }
                    } else {
                        return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                    }
                }
              case Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF:
                Saxon.Hej.tree.iter.AxisIterator descendants = iterateAxis(Saxon.Hej.om.AxisInfo.DESCENDANT, nodeTest);
                if (nodeTest.test(this)) {
                    return new Saxon.Hej.tree.iter.PrependAxisIterator(this, descendants);
                } else {
                    return descendants;
                }
              case Saxon.Hej.om.AxisInfo.FOLLOWING:
                if (((type == Saxon.Hej.type.Type.ATTRIBUTE) || (type == Saxon.Hej.type.Type.NAMESPACE))) {
                    return new Saxon.Hej.tree.tiny.FollowingIterator(tree, ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)getParent()), nodeTest, true);
                } else {
                    if ((tree.depth[nodeNr] == 0)) {
                        return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                    } else {
                        return new Saxon.Hej.tree.tiny.FollowingIterator(tree, this, nodeTest, false);
                    }
                }
              case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
                if ((((type == Saxon.Hej.type.Type.ATTRIBUTE) || (type == Saxon.Hej.type.Type.NAMESPACE)) || (tree.depth[nodeNr] == 0))) {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                } else {
                    return new Saxon.Hej.tree.tiny.SiblingIterator(tree, this, nodeTest, false);
                }
              case Saxon.Hej.om.AxisInfo.NAMESPACE:
                if ((type != Saxon.Hej.type.Type.ELEMENT)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                }
                return Saxon.Hej.tree.NamespaceNode.makeIterator(this, nodeTest);
              case Saxon.Hej.om.AxisInfo.PARENT:
                Saxon.Hej.om.NodeInfo parent = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)getParent());
                return Saxon.Hej.tree.util.Navigator.filteredSingleton(parent, nodeTest);
              case Saxon.Hej.om.AxisInfo.PRECEDING:
                if (((type == Saxon.Hej.type.Type.ATTRIBUTE) || (type == Saxon.Hej.type.Type.NAMESPACE))) {
                    return ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)getParent()).iterateAxis(axisNumber, nodeTest);
                } else {
                    if ((tree.depth[nodeNr] == 0)) {
                        return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                    } else {
                        return new Saxon.Hej.tree.tiny.PrecedingIterator(tree, this, nodeTest, false);
                    }
                }
              case Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING:
                if ((((type == Saxon.Hej.type.Type.ATTRIBUTE) || (type == Saxon.Hej.type.Type.NAMESPACE)) || (tree.depth[nodeNr] == 0))) {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                } else {
                    return new Saxon.Hej.tree.tiny.PrecedingSiblingIterator(tree, this, nodeTest);
                }
              case Saxon.Hej.om.AxisInfo.SELF:
                return Saxon.Hej.tree.util.Navigator.filteredSingleton(this, nodeTest);
              case Saxon.Hej.om.AxisInfo.PRECEDING_OR_ANCESTOR:
                if ((type == Saxon.Hej.type.Type.DOCUMENT)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                } else {
                    if (((type == Saxon.Hej.type.Type.ATTRIBUTE) || (type == Saxon.Hej.type.Type.NAMESPACE))) {
                        Saxon.Hej.tree.tiny.TinyNodeImpl el = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)getParent());
                        return new Saxon.Hej.tree.iter.PrependAxisIterator(el, new Saxon.Hej.tree.tiny.PrecedingIterator(tree, el, nodeTest, true));
                    } else {
                        return new Saxon.Hej.tree.tiny.PrecedingIterator(tree, this, nodeTest, true);
                    }
                }
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown axis number "+axisNumber));
            }
        }
        public virtual Saxon.Hej.om.NodeInfo getParent() {
            if ((parent != null)) {
                return parent;
            }
            int p = getParentNodeNr(tree, nodeNr);
            if ((p == -1)) {
                return null;
            } else {
                return parent = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)tree.getNode(p));
            }
        }
        protected internal static int getParentNodeNr(Saxon.Hej.tree.tiny.TinyTree tree, int nodeNr) {
            if ((tree.depth[nodeNr] == 0)) {
                return -1;
            }
            int p = tree.next[nodeNr];
            while ((p>nodeNr)) {
                if ((tree.nodeKind[p] == Saxon.Hej.type.Type.PARENT_POINTER)) {
                    return tree.alpha[p];
                }
                p = tree.next[p];
            }
            return p;
        }
        public virtual bool hasChildNodes() {
            return false;
        }
        public virtual string getAttributeValue(string uri, string local) {
            return null;
        }
        public virtual Saxon.Hej.om.NodeInfo getRoot() {
            return ((nodeNr == 0) ? this : tree.getRootNode());
        }
        public virtual Saxon.Hej.Configuration getConfiguration() {
            return tree.getConfiguration();
        }
        public Saxon.Hej.om.NamePool getNamePool() {
            return tree.getNamePool();
        }
        public virtual Saxon.Hej.om.NamespaceBinding[] getDeclaredNamespaces(Saxon.Hej.om.NamespaceBinding[] buffer) {
            return null;
        }
        public virtual Saxon.Hej.om.NamespaceMap getAllNamespaces() {
            return null;
        }
        public virtual void generateId(Saxon.Ejava.lang.StringBuilder buffer) {
            buffer.append("d");
            buffer.append(tree.getDocumentNumber());
            buffer.append(NODE_LETTER[getNodeKind()]);
            buffer.append(nodeNr);
        }
        public virtual bool isAncestorOrSelf(Saxon.Hej.tree.tiny.TinyNodeImpl d) {
            if ((tree != d.tree)) {
                return false;
            }
            int dn = d.nodeNr;
            if ((d is Saxon.Hej.tree.tiny.TinyAttributeImpl)) {
                if ((this is Saxon.Hej.tree.tiny.TinyAttributeImpl)) {
                    return (nodeNr == dn);
                } else {
                    dn = tree.attParent[dn];
                }
            }
            if ((this is Saxon.Hej.tree.tiny.TinyAttributeImpl)) {
                return false;
            }
            if ((nodeNr>dn)) {
                return false;
            }
            if ((nodeNr == dn)) {
                return true;
            }
            if (!(((this is Saxon.Hej.tree.tiny.TinyParentNodeImpl)))) {
                return false;
            }
            if ((tree.depth[nodeNr]>=tree.depth[dn])) {
                return false;
            }
            int n = nodeNr;
            while (true) {
                int nextSib = tree.next[n];
                if (((nextSib<0) || (nextSib>dn))) {
                    return true;
                } else {
                    if ((tree.depth[nextSib] == 0)) {
                        return true;
                    } else {
                        if ((nextSib<n)) {
                            n = nextSib;
                        } else {
                            return false;
                        }
                    }
                }
            }
        }
        public virtual bool isId() {
            return false;
        }
        public virtual bool isIdref() {
            return false;
        }
        public bool isNilled() {
            return tree.isNilled(nodeNr);
        }
        public bool isStreamed() {
            return false;
        }
        public virtual Saxon.Hej.tree.tiny.TinyTree getTree() {
            return tree;
        }
        public int getNodeNumber() {
            return nodeNr;
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

        /* Expanding default method copy from interface NodeInfo */
        public virtual void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            Saxon.Hej.tree.util.Navigator.copy(this, @out, copyOptions, locationId);
        }

        /* Expanding default method asActiveSource from interface NodeInfo */
        public virtual Saxon.Hej.lib.ActiveSource asActiveSource() {
            return new Saxon.Hej.om.NodeSource(this);
        }

        /* Expanding default method makeRepeatable from interface Sequence */
        public virtual Saxon.Hej.om.Sequence makeRepeatable() {
            return this;
        }

        /* Creating stub method getUnicodeStringValue to satisfy interface GroundedValue */
        /* (Also inherited from Item) */
        public virtual Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method atomize to satisfy interface Item */
        /* (Also inherited from NodeInfo) */
        public virtual Saxon.Hej.om.AtomicSequence atomize() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getNodeKind to satisfy interface NodeInfo */
        public virtual int getNodeKind() {
            throw new System.NotImplementedException();
        }

    }
}