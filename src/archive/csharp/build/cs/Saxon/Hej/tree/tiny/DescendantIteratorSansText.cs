using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    sealed class DescendantIteratorSansText : Saxon.Hej.tree.iter.AxisIterator {
        private readonly Saxon.Hej.tree.tiny.TinyTree tree;
        private int nextNodeNr;
        private readonly int startDepth;
        private readonly Saxon.Hej.z.IntPredicateProxy matcher;
        internal DescendantIteratorSansText(Saxon.Hej.tree.tiny.TinyTree doc, Saxon.Hej.tree.tiny.TinyNodeImpl node, Saxon.Hej.pattern.NodeTest nodeTest)  {
            tree = doc;
            nextNodeNr = node.nodeNr;
            startDepth = doc.depth[nextNodeNr];
            matcher = nodeTest.getMatcher(doc);
        }
        public Saxon.Hej.om.Item next() {
            do {
                nextNodeNr++;
                try {
                    if ((tree.depth[nextNodeNr]<=startDepth)) {
                        nextNodeNr = -1;
                        return null;
                    }
                } catch (System.IndexOutOfRangeException e) {
                    nextNodeNr = -1;
                    return null;
                }
            } while (!(matcher.test(nextNodeNr)));
            return ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)tree.getNode(nextNodeNr));
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public void close() {
        }

    }
}