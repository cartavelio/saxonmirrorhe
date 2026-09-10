using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    sealed class PrecedingSiblingIterator : Saxon.Hej.tree.iter.AxisIterator {
        private readonly Saxon.Hej.tree.tiny.TinyTree document;
        private readonly Saxon.Hej.tree.tiny.TinyNodeImpl startNode;
        private int nextNodeNr;
        private readonly Saxon.Hej.pattern.NodeTest test;
        private readonly Saxon.Hej.tree.tiny.TinyNodeImpl parentNode;
        private readonly Saxon.Hej.z.IntPredicateProxy matcher;
        internal PrecedingSiblingIterator(Saxon.Hej.tree.tiny.TinyTree doc, Saxon.Hej.tree.tiny.TinyNodeImpl node, Saxon.Hej.pattern.NodeTest nodeTest)  {
            document = doc;
            document.ensurePriorIndex();
            test = nodeTest;
            startNode = node;
            nextNodeNr = node.nodeNr;
            parentNode = node.parent;
            this.matcher = nodeTest.getMatcher(doc);
        }
        public Saxon.Hej.om.Item next() {
            if ((nextNodeNr<0)) {
                return null;
            }
            while (true) {
                nextNodeNr = document.prior[nextNodeNr];
                if ((nextNodeNr<0)) {
                    return null;
                }
                if (matcher.test(nextNodeNr)) {
                    Saxon.Hej.tree.tiny.TinyNodeImpl next = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)document.getNode(nextNodeNr));
                    next.setParentNode(parentNode);
                    return next;
                }
            }
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public void close() {
        }

    }
}