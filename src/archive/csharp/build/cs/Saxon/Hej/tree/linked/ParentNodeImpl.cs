using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    public abstract class ParentNodeImpl : Saxon.Hej.tree.linked.NodeImpl {
        private System.Object _children = null;
        private int sequence;
        protected internal override long getSequenceNumber() {
            return ((getRawSequenceNumber() == -1) ? -(1L) : ((long)(getRawSequenceNumber())<<32));
        }
        protected internal int getRawSequenceNumber() {
            return sequence;
        }
        protected internal void setRawSequenceNumber(int seq) {
            sequence = seq;
        }
        protected internal void setChildren(System.Object children) {
            this._children = children;
        }
        public override bool hasChildNodes() {
            return (_children != null);
        }
        public int getNumberOfChildren() {
            if ((_children == null)) {
                return 0;
            } else {
                if ((_children is Saxon.Hej.tree.linked.NodeImpl)) {
                    return 1;
                } else {
                    return ((Saxon.Hej.om.NodeInfo[])(_children)).Length;
                }
            }
        }
        protected internal Saxon.Hej.tree.iter.AxisIterator iterateChildren(Saxon.Hej.pattern.NodeTest test) {
            if ((_children == null)) {
                return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
            } else {
                if ((_children is Saxon.Hej.tree.linked.NodeImpl)) {
                    Saxon.Hej.tree.linked.NodeImpl child = (Saxon.Hej.tree.linked.NodeImpl)(_children);
                    if (((test == null) || (test == Saxon.Hej.pattern.AnyNodeTest.getInstance()))) {
                        return Saxon.Hej.tree.iter.SingleNodeIterator.makeIterator(child);
                    } else {
                        return Saxon.Hej.tree.util.Navigator.filteredSingleton(child, test);
                    }
                } else {
                    if (((test == null) || (test == Saxon.Hej.pattern.AnyNodeTest.getInstance()))) {
                        return new Saxon.Hej.tree.iter.ArrayIterator.OfNodes<Saxon.Hej.tree.linked.NodeImpl>((Saxon.Hej.tree.linked.NodeImpl[])(_children));
                    } else {
                        return new Saxon.Hej.tree.linked.ChildEnumeration(this, test);
                    }
                }
            }
        }
        public override Saxon.Hej.tree.linked.NodeImpl getFirstChild() {
            if ((_children == null)) {
                return null;
            } else {
                if ((_children is Saxon.Hej.tree.linked.NodeImpl)) {
                    return (Saxon.Hej.tree.linked.NodeImpl)(_children);
                } else {
                    return ((Saxon.Hej.tree.linked.NodeImpl[])(_children))[0];
                }
            }
        }
        public override Saxon.Hej.tree.linked.NodeImpl getLastChild() {
            if ((_children == null)) {
                return null;
            }
            if ((_children is Saxon.Hej.tree.linked.NodeImpl)) {
                return (Saxon.Hej.tree.linked.NodeImpl)(_children);
            }
            Saxon.Hej.tree.linked.NodeImpl[] n = (Saxon.Hej.tree.linked.NodeImpl[])(_children);
            return n[(n.Length-1)];
        }
        protected internal Saxon.Hej.tree.linked.NodeImpl getNthChild(int n) {
            if ((_children == null)) {
                return null;
            }
            if ((_children is Saxon.Hej.tree.linked.NodeImpl)) {
                return ((n == 0) ? (Saxon.Hej.tree.linked.NodeImpl)(_children) : null);
            }
            Saxon.Hej.tree.linked.NodeImpl[] nodes = (Saxon.Hej.tree.linked.NodeImpl[])(_children);
            if (((n<0) || (n>=nodes.Length))) {
                return null;
            }
            return nodes[n];
        }
        protected internal void removeChild(Saxon.Hej.tree.linked.NodeImpl child) {
            if ((_children == null)) {
                return ;
            }
            if ((_children == child)) {
                _children = null;
                return ;
            }
            Saxon.Hej.tree.linked.NodeImpl[] nodes = (Saxon.Hej.tree.linked.NodeImpl[])(_children);
            for (int i = 0; (i<nodes.Length); i++) {
                if ((nodes[i] == child)) {
                    if ((nodes.Length == 2)) {
                        _children = nodes[(1-i)];
                    } else {
                        Saxon.Hej.tree.linked.NodeImpl[] n2 = new Saxon.Hej.tree.linked.NodeImpl[(nodes.Length-1)];
                        if ((i>0)) {
                            Saxon.Ejava.lang.System.arraycopy(nodes, 0, n2, 0, i);
                        }
                        if ((i<(nodes.Length-1))) {
                            Saxon.Ejava.lang.System.arraycopy(nodes, (i+1), n2, i, ((nodes.Length-i)-1));
                        }
                        _children = cleanUpChildren(n2);
                    }
                    break ;
                }
            }
        }
        private Saxon.Hej.tree.linked.NodeImpl[] cleanUpChildren(Saxon.Hej.tree.linked.NodeImpl[] children) {
            bool prevText = false;
            int j = 0;
            Saxon.Hej.tree.linked.NodeImpl[] c2 = new Saxon.Hej.tree.linked.NodeImpl[children.Length];
            foreach (Saxon.Hej.tree.linked.NodeImpl node in children) {
                if ((node is Saxon.Hej.tree.linked.TextImpl)) {
                    if (prevText) {
                        Saxon.Hej.tree.linked.TextImpl prev = (Saxon.Hej.tree.linked.TextImpl)(c2[(j-1)]);
                        prev.replaceStringValue(prev.getUnicodeStringValue().concat(node.getUnicodeStringValue()));
                    } else {
                        if (!(node.getUnicodeStringValue().isEmpty())) {
                            prevText = true;
                            node.setSiblingPosition(j);
                            c2[j++] = node;
                        }
                    }
                } else {
                    node.setSiblingPosition(j);
                    c2[j++] = node;
                    prevText = false;
                }
            }
            if ((j == c2.Length)) {
                return c2;
            } else {
                return Saxon.Ejava.util.Arrays.copyOf(c2, j);
            }
        }
        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            Saxon.Hej.str.UnicodeBuilder sb = null;
            Saxon.Hej.tree.linked.NodeImpl next = getFirstChild();
            while ((next != null)) {
                if ((next is Saxon.Hej.tree.linked.TextImpl)) {
                    if ((sb == null)) {
                        sb = new Saxon.Hej.str.UnicodeBuilder();
                    }
                    sb.accept(next.getUnicodeStringValue());
                }
                next = next.getNextInDocument(this);
            }
            if ((sb == null)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
            return sb.toUnicodeString();
        }
        protected internal void addChild(Saxon.Hej.tree.linked.NodeImpl node, int index) { lock(this) {
            Saxon.Hej.tree.linked.NodeImpl[] c;
            if ((_children == null)) {
                c = new Saxon.Hej.tree.linked.NodeImpl[10];
            } else {
                if ((_children is Saxon.Hej.tree.linked.NodeImpl)) {
                    c = new Saxon.Hej.tree.linked.NodeImpl[10];
                    c[0] = (Saxon.Hej.tree.linked.NodeImpl)(_children);
                } else {
                    c = (Saxon.Hej.tree.linked.NodeImpl[])(_children);
                }
            }
            if ((index>=c.Length)) {
                c = Saxon.Ejava.util.Arrays.copyOf(c, (c.Length*2));
            }
            c[index] = node;
            node.setRawParent(this);
            node.setSiblingPosition(index);
            _children = c;
        }}
        public override void insertChildren(Saxon.Hej.om.NodeInfo[] source, bool atStart, bool inherit) {
            if (atStart) {
                insertChildrenAt(source, 0, inherit);
            } else {
                insertChildrenAt(source, getNumberOfChildren(), inherit);
            }
        }
        protected internal void insertChildrenAt(Saxon.Hej.om.NodeInfo[] source, int index, bool inherit) { lock(this) {
            if ((source.Length == 0)) {
                return ;
            }
            Saxon.Hej.tree.linked.NodeImpl[] source2 = adjustSuppliedNodeArray(source, inherit);
            if ((_children == null)) {
                if ((source2.Length == 1)) {
                    _children = source2[0];
                    ((Saxon.Hej.tree.linked.NodeImpl)(_children)).setSiblingPosition(0);
                } else {
                    _children = cleanUpChildren(source2);
                }
            } else {
                if ((_children is Saxon.Hej.tree.linked.NodeImpl)) {
                    int adjacent = ((index == 0) ? (source2.Length-1) : 0);
                    if (((_children is Saxon.Hej.tree.linked.TextImpl) && (source2[adjacent] is Saxon.Hej.tree.linked.TextImpl))) {
                        if ((index == 0)) {
                            source2[adjacent].replaceStringValue(source2[adjacent].getUnicodeStringValue().concat(((Saxon.Hej.tree.linked.TextImpl)(_children)).getUnicodeStringValue()));
                        } else {
                            source2[adjacent].replaceStringValue(((Saxon.Hej.tree.linked.TextImpl)(_children)).getUnicodeStringValue().concat(source2[adjacent].getUnicodeStringValue()));
                        }
                        _children = cleanUpChildren(source2);
                    } else {
                        Saxon.Hej.tree.linked.NodeImpl[] n2 = new Saxon.Hej.tree.linked.NodeImpl[(source2.Length+1)];
                        if ((index == 0)) {
                            Saxon.Ejava.lang.System.arraycopy(source2, 0, n2, 0, source2.Length);
                            n2[source2.Length] = (Saxon.Hej.tree.linked.NodeImpl)(_children);
                        } else {
                            n2[0] = (Saxon.Hej.tree.linked.NodeImpl)(_children);
                            Saxon.Ejava.lang.System.arraycopy(source2, 0, n2, 1, source2.Length);
                        }
                        _children = cleanUpChildren(n2);
                    }
                } else {
                    Saxon.Hej.tree.linked.NodeImpl[] n0 = (Saxon.Hej.tree.linked.NodeImpl[])(_children);
                    Saxon.Hej.tree.linked.NodeImpl[] n2 = new Saxon.Hej.tree.linked.NodeImpl[(n0.Length+source2.Length)];
                    Saxon.Ejava.lang.System.arraycopy(n0, 0, n2, 0, index);
                    Saxon.Ejava.lang.System.arraycopy(source2, 0, n2, index, source2.Length);
                    Saxon.Ejava.lang.System.arraycopy(n0, index, n2, (index+source2.Length), (n0.Length-index));
                    _children = cleanUpChildren(n2);
                }
            }
        }}
        private Saxon.Hej.tree.linked.NodeImpl convertForeignNode(Saxon.Hej.om.NodeInfo source) {
            if (!(((source is Saxon.Hej.tree.linked.NodeImpl)))) {
                int kind = source.getNodeKind();
                switch (kind) {
                  case Saxon.Hej.type.Type.TEXT:
                    return new Saxon.Hej.tree.linked.TextImpl(source.getUnicodeStringValue());
                  case Saxon.Hej.type.Type.COMMENT:
                    return new Saxon.Hej.tree.linked.CommentImpl(source.getUnicodeStringValue());
                  case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                    return new Saxon.Hej.tree.linked.ProcInstImpl(source.getLocalPart(), source.getUnicodeStringValue());
                  case Saxon.Hej.type.Type.ELEMENT:
                    Saxon.Hej.@event.Builder builder = null;
                    try {
                        builder = new Saxon.Hej.tree.linked.LinkedTreeBuilder(getConfiguration().makePipelineConfiguration(), Saxon.Hej.om.Durability.MUTABLE);
                        builder.open();
                        source.copy(builder, Saxon.Hej.om.CopyOptions.ALL_NAMESPACES, Saxon.Hej.expr.parser.Loc.NONE);
                        builder.close();
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Ejava.lang.IllegalArgumentException("Failed to convert inserted element node to an instance of net.sf.saxon.om.tree.ElementImpl");
                    }
                    return (Saxon.Hej.tree.linked.NodeImpl)(builder.getCurrentRoot());
                  default:
                    throw new Saxon.Ejava.lang.IllegalArgumentException("Cannot insert a node unless it is an element, comment, text node, or processing instruction");
                }
            }
            return (Saxon.Hej.tree.linked.NodeImpl)(source);
        }
        protected internal void replaceChildrenAt(Saxon.Hej.om.NodeInfo[] source, int index, bool inherit) { lock(this) {
            if ((_children == null)) {
                return ;
            }
            Saxon.Hej.tree.linked.NodeImpl[] source2 = adjustSuppliedNodeArray(source, inherit);
            if ((_children is Saxon.Hej.tree.linked.NodeImpl)) {
                if ((source2.Length == 0)) {
                    _children = null;
                } else {
                    if ((source2.Length == 1)) {
                        _children = source2[0];
                    } else {
                        Saxon.Hej.tree.linked.NodeImpl[] n2 = new Saxon.Hej.tree.linked.NodeImpl[source2.Length];
                        Saxon.Ejava.lang.System.arraycopy(source2, 0, n2, 0, source.Length);
                        _children = cleanUpChildren(n2);
                    }
                }
            } else {
                Saxon.Hej.tree.linked.NodeImpl[] n0 = (Saxon.Hej.tree.linked.NodeImpl[])(_children);
                Saxon.Hej.tree.linked.NodeImpl[] n2 = new Saxon.Hej.tree.linked.NodeImpl[((n0.Length+source2.Length)-1)];
                Saxon.Ejava.lang.System.arraycopy(n0, 0, n2, 0, index);
                Saxon.Ejava.lang.System.arraycopy(source2, 0, n2, index, source2.Length);
                Saxon.Ejava.lang.System.arraycopy(n0, (index+1), n2, (index+source2.Length), ((n0.Length-index)-1));
                _children = cleanUpChildren(n2);
            }
        }}
        private Saxon.Hej.tree.linked.NodeImpl[] adjustSuppliedNodeArray(Saxon.Hej.om.NodeInfo[] source, bool inherit) {
            Saxon.Hej.tree.linked.NodeImpl[] source2 = new Saxon.Hej.tree.linked.NodeImpl[source.Length];
            for (int i = 0; (i<source.Length); i++) {
                source2[i] = convertForeignNode(source[i]);
                Saxon.Hej.tree.linked.NodeImpl child = source2[i];
                child.setRawParent(this);
                if ((child is Saxon.Hej.tree.linked.ElementImpl)) {
                    ((Saxon.Hej.tree.linked.ElementImpl)(child)).fixupInsertedNamespaces(inherit);
                }
            }
            return source2;
        }
        public void compact(int size) { lock(this) {
            if ((size == 0)) {
                _children = null;
            } else {
                if ((size == 1)) {
                    if ((_children is Saxon.Hej.tree.linked.NodeImpl[])) {
                        _children = ((Saxon.Hej.tree.linked.NodeImpl[])(_children))[0];
                    }
                } else {
                    _children = Saxon.Ejava.util.Arrays.copyOf((Saxon.Hej.tree.linked.NodeImpl[])(_children), size);
                }
            }
        }}
    
    }
}