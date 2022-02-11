using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    sealed class SiblingIterator : Saxon.Hej.tree.iter.AxisIterator, Saxon.Hej.tree.iter.LookaheadIterator, Saxon.Hej.om.AtomizedValueIterator {
        private readonly Saxon.Hej.tree.tiny.TinyTree tree;
        private int nextNodeNr;
        private readonly Saxon.Hej.pattern.NodeTest test;
        private readonly Saxon.Hej.tree.tiny.TinyNodeImpl startNode;
        private readonly Saxon.Hej.tree.tiny.TinyNodeImpl parentNode;
        private readonly bool getChildren;
        private bool needToAdvance = false;
        private readonly Saxon.Hej.z.IntPredicateProxy matcher;
        internal SiblingIterator(Saxon.Hej.tree.tiny.TinyTree tree, Saxon.Hej.tree.tiny.TinyNodeImpl node, Saxon.Hej.pattern.NodeTest nodeTest, bool getChildren)  {
            this.tree = tree;
            test = nodeTest;
            if ((nodeTest == null)) {
                matcher = Saxon.Hej.z.IntSetPredicate.ALWAYS_TRUE;
            } else {
                matcher = nodeTest.getMatcher(tree);
            }
            startNode = node;
            this.getChildren = getChildren;
            if (getChildren) {
                parentNode = node;
                nextNodeNr = (node.nodeNr+1);
            } else {
                parentNode = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)node.getParent());
                if ((parentNode == null)) {
                    nextNodeNr = -1;
                } else {
                    nextNodeNr = tree.next[node.nodeNr];
                    while ((tree.nodeKind[nextNodeNr] == Saxon.Hej.type.Type.PARENT_POINTER)) {
                        nextNodeNr = tree.next[nextNodeNr];
                    }
                    if ((nextNodeNr<node.nodeNr)) {
                        nextNodeNr = -1;
                    }
                }
            }
            if (((nextNodeNr>=0) && (nodeTest != null))) {
                if (!(matcher.test(nextNodeNr))) {
                    needToAdvance = true;
                }
            }
        }
        public Saxon.Hej.om.Item next() {
            if (needToAdvance) {
                int thisNode = nextNodeNr;
                int[] tNext = tree.next;
                if ((test == null)) {
                    do {
                        nextNodeNr = tNext[nextNodeNr];
                    } while ((tree.nodeKind[nextNodeNr] == Saxon.Hej.type.Type.PARENT_POINTER));
                } else {
                    do {
                        nextNodeNr = tNext[nextNodeNr];
                    } while (((nextNodeNr>=thisNode) && !(matcher.test(nextNodeNr))));
                }
                if ((nextNodeNr<thisNode)) {
                    nextNodeNr = -1;
                    needToAdvance = false;
                    return null;
                }
            }
            if ((nextNodeNr == -1)) {
                return null;
            }
            needToAdvance = true;
            Saxon.Hej.tree.tiny.TinyNodeImpl nextNode = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)tree.getNode(nextNodeNr));
            nextNode.setParentNode(parentNode);
            return nextNode;
        }
        public Saxon.Hej.om.AtomicSequence nextAtomizedValue() {
            if (needToAdvance) {
                int thisNode = nextNodeNr;
                int[] tNext = tree.next;
                if ((test == null)) {
                    do {
                        nextNodeNr = tNext[nextNodeNr];
                    } while ((tree.nodeKind[nextNodeNr] == Saxon.Hej.type.Type.PARENT_POINTER));
                } else {
                    do {
                        nextNodeNr = tNext[nextNodeNr];
                    } while (((nextNodeNr>=thisNode) && !(matcher.test(nextNodeNr))));
                }
                if ((nextNodeNr<thisNode)) {
                    nextNodeNr = -1;
                    needToAdvance = false;
                    return null;
                }
            }
            if ((nextNodeNr == -1)) {
                return null;
            }
            needToAdvance = true;
            int kind = tree.nodeKind[nextNodeNr];
            switch (kind) {
              case Saxon.Hej.type.Type.TEXT:
                {
                    return Saxon.Hej.value.StringValue.makeUntypedAtomic(Saxon.Hej.tree.tiny.TinyTextImpl.getStringValue(tree, nextNodeNr));
                }
              case Saxon.Hej.type.Type.WHITESPACE_TEXT:
                {
                    return Saxon.Hej.value.StringValue.makeUntypedAtomic(Saxon.Hej.tree.tiny.WhitespaceTextImpl.getStringValue(tree, nextNodeNr));
                }
              case Saxon.Hej.type.Type.ELEMENT:
              case Saxon.Hej.type.Type.TEXTUAL_ELEMENT:
                {
                    return tree.getTypedValueOfElement(nextNodeNr);
                }
              case Saxon.Hej.type.Type.COMMENT:
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                return tree.getAtomizedValueOfUntypedNode(nextNodeNr);
              default:
                throw new Saxon.Ejava.lang.AssertionError("Unknown node kind on child axis");
            }
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            int n = nextNodeNr;
            if (needToAdvance) {
                int[] tNext = tree.next;
                if ((test == null)) {
                    do {
                        n = tNext[n];
                    } while ((tree.nodeKind[n] == Saxon.Hej.type.Type.PARENT_POINTER));
                } else {
                    do {
                        n = tNext[n];
                    } while (((n>=nextNodeNr) && !(matcher.test(n))));
                }
                if ((n<nextNodeNr)) {
                    return false;
                }
            }
            return (n != -1);
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public void close() {
        }

        /* Expanding default method discharge from interface SequenceIterator */
        public void discharge() {
        }

    }
}