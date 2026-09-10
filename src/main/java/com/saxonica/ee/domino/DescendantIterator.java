package com.saxonica.ee.domino;

import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.z.IntPredicateProxy;

/**
 * This class supports both the descendant:: and descendant-or-self:: axes, which are
 * identical except for the route to the first candidate node.
 * It enumerates descendants of the specified node.
 * The calling code must ensure that the start node is not an attribute or namespace node.
 */

final class DescendantIterator implements AxisIterator {

    private final DominoTree tree;
    private final DominoNode startNode;
    private int nextNodeNr;
    private final int startDepth;
    private final NodeTest test;
    private final IntPredicateProxy matcher;

    /**
     * Create an iterator over the descendant axis
     *
     * @param doc         the containing TinyTree
     * @param node        the node whose descendants are required
     * @param nodeTest    test to be satisfied by each returned node
     */

    DescendantIterator(DominoTree doc, DominoNode node, NodeTest nodeTest) {
        tree = doc;
        startNode = node;
        test = nodeTest;
        nextNodeNr = node.getNodeNr();
        startDepth = doc.depth[nextNodeNr];
        matcher = test.getMatcher(doc);
    }

    /*@Nullable*/
    @Override
    public NodeInfo next() {
        do {
            nextNodeNr++;
            try {
                if (tree.depth[nextNodeNr] <= startDepth) {
                    nextNodeNr = -1;
                    return null;
                }
            } catch (ArrayIndexOutOfBoundsException e) {
                // this shouldn't happen. If it does happen, it means the tree wasn't properly closed
                // during construction (there is no stopper node at the end). In this case, we'll recover
                // by returning end-of sequence
                //System.err.println("********* no stopper node **********");
                nextNodeNr = -1;
                return null;
            }
        } while (!matcher.test(nextNodeNr));

        return tree.getNode(nextNodeNr);
    }

}

// Copyright (c) 2017-2022 Saxonica Limited