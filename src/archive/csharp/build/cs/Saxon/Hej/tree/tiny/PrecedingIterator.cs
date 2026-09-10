using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    sealed class PrecedingIterator : Saxon.Hej.tree.iter.AxisIterator {
        private readonly Saxon.Hej.tree.tiny.TinyTree tree;
        private Saxon.Hej.om.NodeInfo current;
        private int nextAncestorDepth;
        private readonly bool includeAncestors;
        private readonly Saxon.Hej.z.IntPredicateProxy matcher;
        private Saxon.Hej.om.NodeInfo pending = null;
        private readonly Saxon.Hej.pattern.NodeTest nodeTest;
        private readonly bool matchesTextNodes;
        public PrecedingIterator(Saxon.Hej.tree.tiny.TinyTree doc, Saxon.Hej.tree.tiny.TinyNodeImpl node, Saxon.Hej.pattern.NodeTest nodeTest, bool includeAncestors)  {
            this.includeAncestors = includeAncestors;
            tree = doc;
            current = node;
            nextAncestorDepth = (doc.depth[node.nodeNr]-1);
            this.nodeTest = nodeTest;
            this.matcher = nodeTest.getMatcher(doc);
            matchesTextNodes = nodeTest.getUType().overlaps(Saxon.Hej.type.UType.TEXT);
        }
        public Saxon.Hej.om.Item next() {
            if ((pending != null)) {
                current = pending;
                pending = null;
                return current;
            }
            if ((current == null)) {
                return null;
            }
            if ((current is Saxon.Hej.tree.tiny.TinyTextualElement.TinyTextualElementText)) {
                current = current.getParent();
            }
            int nextNodeNr = ((Saxon.Hej.tree.tiny.TinyNodeImpl)(current)).nodeNr;
            while (true) {
                if (!(includeAncestors)) {
                    nextNodeNr--;
                    while (((nextAncestorDepth>=0) && (tree.depth[nextNodeNr] == nextAncestorDepth))) {
                        if ((nextAncestorDepth--<=0)) {
                            current = null;
                            return null;
                        }
                        nextNodeNr--;
                    }
                } else {
                    if ((tree.depth[nextNodeNr] == 0)) {
                        current = null;
                        return null;
                    } else {
                        nextNodeNr--;
                    }
                }
                if ((matchesTextNodes && (tree.nodeKind[nextNodeNr] == Saxon.Hej.type.Type.TEXTUAL_ELEMENT))) {
                    Saxon.Hej.tree.tiny.TinyTextualElement element = (Saxon.Hej.tree.tiny.TinyTextualElement)(((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)tree.getNode(nextNodeNr)));
                    Saxon.Hej.tree.tiny.TinyTextualElement.TinyTextualElementText text = element.getTextNode();
                    if (nodeTest.test(text)) {
                        if (nodeTest.test(element)) {
                            pending = element;
                        }
                        return current = text;
                    } else {
                        if (nodeTest.test(element)) {
                            return current = element;
                        }
                    }
                } else {
                    if (matcher.test(nextNodeNr)) {
                        current = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)tree.getNode(nextNodeNr));
                        return current;
                    }
                    if ((tree.depth[nextNodeNr] == 0)) {
                        current = null;
                        return null;
                    }
                }
            }
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public void close() {
        }

    }
}