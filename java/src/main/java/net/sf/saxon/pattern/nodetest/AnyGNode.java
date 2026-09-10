package net.sf.saxon.pattern.nodetest;

import net.sf.saxon.om.GNode;
import net.sf.saxon.tree.tiny.NodeVectorTree;
import net.sf.saxon.z.IntPredicateProxy;
import net.sf.saxon.z.IntSetPredicate;

public class AnyGNode implements NodePredicate, NodeVectorMatchMaker {

    private AnyGNode(){};

    public static AnyGNode TEST = new AnyGNode();

    @Override
    public boolean test(GNode node) {
        return true;
    }

    /**
     * Get a matching function that can be used to test whether numbered nodes in a TinyTree
     * or DominoTree satisfy the node test. (Calling this matcher must give the same result
     * as calling <code>matchesNode(tree.getNode(nodeNr))</code>, but it may well be faster).
     *
     * @param tree the tree against which the returned function will operate
     * @return an IntPredicate; the matches() method of this predicate takes a node number
     * as input, and returns true if and only if the node identified by this node number
     * matches the node predicate.
     */
    @Override
    public IntPredicateProxy getMatcher(NodeVectorTree tree) {
        return IntSetPredicate.ALWAYS_TRUE;
    }
}

