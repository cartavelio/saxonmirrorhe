package com.saxonica.ee.domino;

import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.iter.LookaheadIterator;
import net.sf.saxon.type.Type;
import net.sf.saxon.z.IntPredicateProxy;

/**
 * This class supports both the child:: and following-sibling:: axes, which are
 * identical except for the route to the first candidate node.
 * It enumerates either the children or the following siblings of the specified node.
 * In the case of children, the specified node must always
 * be a node that has children: to ensure this, construct the iterator
 * using NodeInfo#iterateAxis()
 */

final class SiblingIterator implements AxisIterator, LookaheadIterator {

    // NOTE: have experimented with a dedicated iterator for the child axis matching against
    // elements only, by fingerprint - no measurable improvement obtained.

    private final DominoTree tree;
    private int nextNodeNr;
    /*@Nullable*/ private final NodeTest test;
    private final DominoNode startNode;
    private final DominoNode parentNode;
    private final boolean getChildren;
    private boolean needToAdvance = false;
    private final IntPredicateProxy matcher;

    /**
     * Return an enumeration over children or siblings of the context node
     *
     * @param tree        The TinyTree containing the context node
     * @param node        The context node, the start point for the iteration
     * @param nodeTest    Test that the selected nodes must satisfy, or null indicating
     *                    that all nodes are selected
     * @param getChildren True if children of the context node are to be returned, false
     *                    if following siblings are required
     */

    SiblingIterator(/*@NotNull*/ DominoTree tree, /*@NotNull*/ DominoNode node,
                    /*@Nullable*/ NodeTest nodeTest, boolean getChildren) {
        this.tree = tree;
        test = nodeTest;
        matcher = nodeTest.getMatcher(tree);
        startNode = node;
        this.getChildren = getChildren;
        if (getChildren) {          // child:: axis
            parentNode = node;
            // move to first child
            // ASSERT: we don't invoke this code unless the node has children
            nextNodeNr = node.getNodeNr() + 1;

        } else {                    // following-sibling:: axis
            parentNode = node.getParent();
            if (parentNode == null) {
                nextNodeNr = -1;
            } else {
                // move to next sibling
                nextNodeNr = tree.next[node.getNodeNr()];
                if (nextNodeNr < node.getNodeNr()) {
                    // if "next" pointer goes backwards, it's really an owner pointer from the last sibling
                    nextNodeNr = -1;
                }
            }
        }

        // check if this matches the conditions
        if (nextNodeNr >= 0 && nodeTest != null) {
            if (!matcher.test(nextNodeNr)) {
                needToAdvance = true;
            }
        }
    }

    /**
     * Return the next node in the sequence
     *
     * @return the next node, or null if the end of the sequence is reached
     */

    /*@Nullable*/
    @Override
    public NodeInfo next() {
        if (needToAdvance) {
            final int thisNode = nextNodeNr;
            final int[] tNext = tree.next;
            final NodeTest nTest = test;
            if (nTest == null) {
                do {
                    nextNodeNr = tNext[nextNodeNr];
                } while (tree.getNodeKind(nextNodeNr) == Type.PARENT_POINTER);
            } else {
                do {
                    nextNodeNr = tNext[nextNodeNr];
                } while (nextNodeNr >= thisNode && !matcher.test(nextNodeNr));
            }

            if (nextNodeNr < thisNode) {    // indicates we've got to the last sibling
                nextNodeNr = -1;
                needToAdvance = false;
                return null;
            }
        }

        if (nextNodeNr == -1) {
            return null;
        }
        needToAdvance = true;
        DominoNode nextNode = tree.getNode(nextNodeNr);
        nextNode.setParentNode(parentNode);
        return nextNode;
    }

    @Override
    public boolean supportsHasNext() {
        return true;
    }

    /**
     * Test whether there are any more nodes to come. This method is used only when testing whether the
     * current item is the last in the sequence. It's not especially efficient, but is more efficient than
     * the alternative strategy which involves counting how many nodes there are in the sequence.
     *
     * @return true if there are more items in the sequence
     */

    @Override
    public boolean hasNext() {
        int n = nextNodeNr;
        if (needToAdvance) {
            final NodeTest nTest = test;
            final int[] tNext = tree.next;
            if (nTest == null) {
                do {
                    n = tNext[n];
                } while (tree.nodeKind[n] == Type.PARENT_POINTER);
            } else {
                do {
                    n = tNext[n];
                } while (n >= nextNodeNr && !matcher.test(n));
            }

            if (n < nextNodeNr) {    // indicates we've got to the last sibling
                return false;
            }
        }

        return n != -1;
    }

}

// Copyright (c) 2017-2023 Saxonica Limited

