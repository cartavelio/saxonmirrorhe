using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    sealed class FollowingIterator : Saxon.Hej.tree.iter.AxisIterator {
        private readonly Saxon.Hej.tree.tiny.TinyTree tree;
        private readonly Saxon.Hej.tree.tiny.TinyNodeImpl startNode;
        private Saxon.Hej.om.NodeInfo current;
        private readonly Saxon.Hej.pattern.NodeTest test;
        private readonly bool includeDescendants;
        internal int position = 0;
        private readonly Saxon.Hej.z.IntPredicateProxy matcher;
        private Saxon.Hej.om.NodeInfo pending;
        public FollowingIterator(Saxon.Hej.tree.tiny.TinyTree doc, Saxon.Hej.tree.tiny.TinyNodeImpl node, Saxon.Hej.pattern.NodeTest nodeTest, bool includeDescendants)  {
            tree = doc;
            test = nodeTest;
            startNode = node;
            this.includeDescendants = includeDescendants;
            this.matcher = nodeTest.getMatcher(doc);
        }
        public Saxon.Hej.om.Item next() {
            if ((pending != null)) {
                Saxon.Hej.om.NodeInfo p = pending;
                pending = null;
                return p;
            }
            int nodeNr;
            if ((position<=0)) {
                if ((position<0)) {
                    return null;
                }
                nodeNr = startNode.nodeNr;
                if (includeDescendants) {
                    nodeNr++;
                } else {
                    while (true) {
                        int nextSib = tree.next[nodeNr];
                        if ((nextSib>nodeNr)) {
                            nodeNr = nextSib;
                            break ;
                        } else {
                            if ((tree.depth[nextSib] == 0)) {
                                current = null;
                                position = -1;
                                return null;
                            } else {
                                nodeNr = nextSib;
                            }
                        }
                    }
                }
            } else {
                System.Diagnostics.Debug.Assert((current != null));
                Saxon.Hej.tree.tiny.TinyNodeImpl here;
                if ((current is Saxon.Hej.tree.tiny.TinyTextualElement.TinyTextualElementText)) {
                    here = (Saxon.Hej.tree.tiny.TinyNodeImpl)(current.getParent());
                } else {
                    here = (Saxon.Hej.tree.tiny.TinyNodeImpl)(current);
                }
                nodeNr = (here.nodeNr+1);
            }
            while (true) {
                if ((tree.depth[nodeNr] == 0)) {
                    current = null;
                    position = -1;
                    return null;
                }
                if ((tree.nodeKind[nodeNr] == Saxon.Hej.type.Type.TEXTUAL_ELEMENT)) {
                    Saxon.Hej.tree.tiny.TinyTextualElement e = (Saxon.Hej.tree.tiny.TinyTextualElement)(((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)tree.getNode(nodeNr)));
                    Saxon.Hej.om.NodeInfo t = e.getTextNode();
                    if (matcher.test(nodeNr)) {
                        if (test.test(t)) {
                            pending = t;
                        }
                        position++;
                        return current = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)tree.getNode(nodeNr));
                    } else {
                        if (test.test(t)) {
                            position++;
                            return current = t;
                        }
                    }
                } else {
                    if (matcher.test(nodeNr)) {
                        position++;
                        current = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)tree.getNode(nodeNr));
                        return current;
                    }
                }
                nodeNr++;
            }
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public void close() {
        }

    }
}