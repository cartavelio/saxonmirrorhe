using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    sealed class NamedChildIterator : Saxon.Hej.tree.iter.AxisIterator, Saxon.Hej.tree.iter.LookaheadIterator, Saxon.Hej.om.AtomizedValueIterator {
        private readonly Saxon.Hej.tree.tiny.TinyTree tree;
        private int nextNodeNr;
        private readonly int fingerprint;
        private Saxon.Hej.tree.tiny.TinyNodeImpl startNode;
        private bool needToAdvance = false;
        internal NamedChildIterator(Saxon.Hej.tree.tiny.TinyTree tree, Saxon.Hej.tree.tiny.TinyNodeImpl node, int fingerprint)  {
            this.tree = tree;
            this.fingerprint = fingerprint;
            this.startNode = node;
            startNode = node;
            nextNodeNr = (node.nodeNr+1);
            if ((((((tree.nodeKind[nextNodeNr]&unchecked((int)0xf))) != Saxon.Hej.type.Type.ELEMENT)) || (((tree.nameCode[nextNodeNr]&unchecked((int)0xfffff))) != fingerprint))) {
                needToAdvance = true;
            }
        }
        public Saxon.Hej.om.Item next() {
            if (needToAdvance) {
                int thisNode = nextNodeNr;
                do {
                    nextNodeNr = tree.next[nextNodeNr];
                    if ((nextNodeNr<thisNode)) {
                        nextNodeNr = -1;
                        needToAdvance = false;
                        return null;
                    }
                } while ((((((tree.nameCode[nextNodeNr]&unchecked((int)0xfffff))) != fingerprint)) || ((((tree.nodeKind[nextNodeNr]&unchecked((int)0xf))) != Saxon.Hej.type.Type.ELEMENT))));
            } else {
                if ((nextNodeNr == -1)) {
                    return null;
                }
            }
            needToAdvance = true;
            Saxon.Hej.tree.tiny.TinyNodeImpl nextNode = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)tree.getNode(nextNodeNr));
            nextNode.setParentNode(startNode);
            return nextNode;
        }
        public Saxon.Hej.om.AtomicSequence nextAtomizedValue() {
            if (needToAdvance) {
                int thisNode = nextNodeNr;
                do {
                    nextNodeNr = tree.next[nextNodeNr];
                    if ((nextNodeNr<thisNode)) {
                        nextNodeNr = -1;
                        needToAdvance = false;
                        return null;
                    }
                } while ((((((tree.nameCode[nextNodeNr]&unchecked((int)0xfffff))) != fingerprint)) || (((tree.nodeKind[nextNodeNr]&unchecked((int)0xf))) != Saxon.Hej.type.Type.ELEMENT)));
            } else {
                if ((nextNodeNr == -1)) {
                    return null;
                }
            }
            needToAdvance = true;
            return tree.getTypedValueOfElement(nextNodeNr);
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            int n = nextNodeNr;
            if (needToAdvance) {
                int thisNode = n;
                do {
                    n = tree.next[n];
                    if ((n<thisNode)) {
                        return false;
                    }
                } while (((((tree.nodeKind[n]&unchecked((int)0xf))) != Saxon.Hej.type.Type.ELEMENT) || (((tree.nameCode[n]&unchecked((int)0xfffff))) != fingerprint)));
                return true;
            } else {
                return (n != -1);
            }
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public void close() {
        }

        /* Expanding default method discharge from interface SequenceIterator */
        public void discharge() {
        }

    }
}