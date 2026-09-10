package net.sf.saxon.pattern.nodetest;

import net.sf.saxon.ma.jnode.AnyJNodeType;
import net.sf.saxon.ma.jnode.JNode;
import net.sf.saxon.om.GNode;
import net.sf.saxon.pattern.qname.AnyQNameTest;
import net.sf.saxon.pattern.qname.QNameTest;
import net.sf.saxon.type.ChoiceItemType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.type.gnode.NodeKindType;

/**
 * A node test representing the syntax {@code *} - meaning either all
 * children of a JNode, or all children of the principal node kind of the axis
 * in the case of an XNode
 */

public class NodeTestStar extends NodeTest {

    private final TypeHierarchy th;
    private final int defaultNodeKind;

    public NodeTestStar(int defaultNodeKind, TypeHierarchy th) {
        this.defaultNodeKind = defaultNodeKind;
        this.th = th;
    }

    public int getDefaultNodeKind() {
        return defaultNodeKind;
    }

    /**
     * Determine the default priority to use if this node-test appears as a match pattern
     * for a template with no explicit priority attribute.
     *
     * @return the default priority for the pattern
     */
    @Override
    public double getDefaultPriority() {
        return 0;
    }

    /**
     * Extract a QNameTest (the strongest one possible) that must be satisfied by a node
     * if it is to satisfy this NodeTest
     *
     * @return the strongest possible QNameTest
     */
    @Override
    public QNameTest getQNameTest() {
        return AnyQNameTest.getInstance();
    }

    /**
     * Test whether this node test is satisfied by a given node. 
     *
     * @param node the node to be matched
     * @return true if the node test is satisfied by the supplied node, false otherwise
     */
    @Override
    public boolean test(GNode node) {
        return node instanceof JNode || node.getNodeKind() == defaultNodeKind;
    }

    /**
     * Get an item type that all matching nodes must satisfy
     *
     * @return an item type
     */
    @Override
    public ItemType getItemType() {
        return ChoiceItemType.of(AnyJNodeType.getInstance(), NodeKindType.of(defaultNodeKind));
    }

    @Override
    public String toString() {
        return "*";
    }
}


