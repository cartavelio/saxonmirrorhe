package net.sf.saxon.pattern.nodetest;

import net.sf.saxon.tree.tiny.NodeVectorTree;
import net.sf.saxon.z.IntPredicateProxy;

public interface NodeVectorMatchMaker extends NodePredicate {

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

    IntPredicateProxy getMatcher(final NodeVectorTree tree);
}

