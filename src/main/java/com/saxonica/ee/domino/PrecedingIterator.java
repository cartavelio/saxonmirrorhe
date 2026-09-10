package com.saxonica.ee.domino;

import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.z.IntPredicateProxy;

/**
 * Enumerate all the nodes on the preceding axis from a given start node.
 * The calling code ensures that the start node is not a root, attribute,
 * or namespace node. As well as the standard XPath preceding axis, this
 * class also implements a Saxon-specific "preceding-or-ancestor" axis
 * which returns ancestor nodes as well as preceding nodes. This is used
 * when performing xsl:number level="any".
 */

final class PrecedingIterator implements AxisIterator {

    private final DominoTree tree;
    private final DominoNode startNode;
    private DominoNode current;
    private final NodeTest test;
    private int nextAncestorDepth;
    private final boolean includeAncestors;
    private final IntPredicateProxy matcher;

    public PrecedingIterator(/*@NotNull*/ DominoTree doc, /*@NotNull*/ DominoNode node,
                             NodeTest nodeTest, boolean includeAncestors) {

        this.includeAncestors = includeAncestors;
        test = nodeTest;
        tree = doc;
        startNode = node;
        current = startNode;
        nextAncestorDepth = doc.depth[node.getNodeNr()] - 1;
        matcher = nodeTest.getMatcher(doc);
    }

    /*@Nullable*/
    @Override
    public NodeInfo next() {
        if (current == null) {
            return null;
        }
        int nextNodeNr = current.getNodeNr();
        while (true) {
            if (!includeAncestors) {
                nextNodeNr--;
                // skip over ancestor elements
                while (nextAncestorDepth >= 0 && tree.depth[nextNodeNr] == nextAncestorDepth) {
                    if (nextAncestorDepth-- <= 0) {  // bug 1121528
                        current = null;
                        return null;
                    }
                    nextNodeNr--;
                }
            } else {
                if (tree.depth[nextNodeNr] == 0) {
                    current = null;
                    return null;
                } else {
                    nextNodeNr--;
                }
            }
            if (matcher.test(nextNodeNr)) {
                current = tree.getNode(nextNodeNr);
                return current;
            }
            if (tree.depth[nextNodeNr] == 0) {
                current = null;
                return null;
            }
        }
    }

}

// Copyright (c) 2017-2022 Saxonica Limited

