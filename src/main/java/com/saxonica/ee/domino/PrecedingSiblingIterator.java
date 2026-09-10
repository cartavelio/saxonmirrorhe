package com.saxonica.ee.domino;

import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.tree.iter.AxisIterator;

import java.util.function.IntPredicate;

/**
 * This class supports the preceding-sibling axis.
 * The starting node must be an element, text node, comment, or processing instruction:
 * to ensure this, construct the enumeration using NodeInfo#getEnumeration()
 */

final class PrecedingSiblingIterator implements AxisIterator {

    private DominoTree document;
    private DominoNode startNode;
    private int nextNodeNr;
    private NodeTest test;
    private DominoNode parentNode;
    private final IntPredicate matcher;

    PrecedingSiblingIterator(DominoTree doc, /*@NotNull*/ DominoNode node, NodeTest nodeTest) {
        document = doc;
        document.ensurePriorIndex();
        test = nodeTest;
        matcher = nodeTest.getMatcher(doc);
        startNode = node;
        nextNodeNr = node.getNodeNr();
        parentNode = node.getParent();   // doesn't matter if this is null (unknown)
    }

    /*@Nullable*/
    public NodeInfo next() {
        if (nextNodeNr < 0) {
            // This check is needed because an errant caller can call next() again after hitting the end of sequence
            return null;
        }
        while (true) {
            nextNodeNr = document.prior[nextNodeNr];
            if (nextNodeNr < 0) {
                return null;
            }
            if (matcher.test(nextNodeNr)) {
                DominoNode next = document.getNode(nextNodeNr);
                next.setParentNode(parentNode);
                return next;
            }
        }
    }

}

// Copyright (c) 2017 Saxonica Ltd

