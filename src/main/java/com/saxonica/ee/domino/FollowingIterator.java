package com.saxonica.ee.domino;

import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.z.IntPredicateProxy;

/**
 * Iterate over the following axis starting at a given node.
 * The start node must not be a namespace or attribute node.
 */

final class FollowingIterator implements AxisIterator {

    private final DominoTree tree;
    private final DominoNode startNode;
    private NodeInfo current;
    private final NodeTest test;
    private final boolean includeDescendants;
    int position = 0;
    private final IntPredicateProxy matcher;

    /**
     * Create an iterator over the following axis
     *
     * @param doc                the containing TinyTree
     * @param node               the start node. If the actual start was an attribute or namespace node, this will
     *                           be the parent element of that attribute or namespace
     * @param nodeTest           condition that all the returned nodes must satisfy
     * @param includeDescendants true if descendants of the start node are to be included. This will
     *                           be false if the actual start was an element node, true if it was an attribute or namespace node
     *                           (since the children of their parent follow the attribute or namespace in document order).
     */

    public FollowingIterator(DominoTree doc, DominoNode node,
                             NodeTest nodeTest, boolean includeDescendants) {
        tree = doc;
        test = nodeTest;
        startNode = node;
        this.includeDescendants = includeDescendants;
        this.matcher = nodeTest.getMatcher(doc);
    }

    /*@Nullable*/
    @Override
    public NodeInfo next() {
        int nodeNr;
        if (position <= 0) {
            if (position < 0) {
                // already at end
                return null;
            }
            // first time call
            nodeNr = startNode.getNodeNr();

            // skip the descendant nodes if any
            if (includeDescendants) {
                nodeNr++;
            } else {
                while (true) {
                    int nextSib = tree.next[nodeNr];
                    if (nextSib > nodeNr) {
                        nodeNr = nextSib;
                        break;
                    } else if (tree.depth[nextSib] == 0) {
                        current = null;
                        position = -1;
                        return null;
                    } else {
                        nodeNr = nextSib;
                    }
                }
            }
        } else {
            assert current != null;
            nodeNr = ((DominoNode) current).getNodeNr() + 1;
        }

        while (true) {
            if (tree.depth[nodeNr] == 0) {
                current = null;
                position = -1;
                return null;
            }
            if (matcher.test(nodeNr)) {
                position++;
                current = tree.getNode(nodeNr);
                return current;
            }
            nodeNr++;
        }
    }

}

// Copyright (c) 2017-2022 Saxonica Limited

