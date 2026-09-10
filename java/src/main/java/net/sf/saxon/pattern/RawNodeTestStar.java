package net.sf.saxon.pattern;

import net.sf.saxon.ma.jnode.JNode;
import net.sf.saxon.ma.jnode.JNodeType;
import net.sf.saxon.om.GNode;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.Type;

/**
 * A raw node test representing the syntax {@code *} - meaning either all
 * children of a JNode, or all children of the principal node kind of the axis
 * in the case of an XNode
 */

public class RawNodeTestStar extends RawNodeTest {

    private final int defaultNodeKind;

    public RawNodeTestStar(int defaultNodeKind) {
        this.defaultNodeKind = defaultNodeKind;
    }

    /**
     * Convert this RawNodeTest to an executable node test using type information
     * about the origin node of the axis step.
     *
     * @param origin the type of the origin node
     * @return a refined NodeTest.
     */
    @Override
    public NodeTest cook(ItemType origin) {
        if (origin instanceof JNodeType) {
            return JNodeType.ANY_JNODE;
        }
        if (origin instanceof XNodeTest) {
            switch (defaultNodeKind) {
                case Type.ATTRIBUTE:
                    return NodeKindTest.ATTRIBUTE;
                case Type.NAMESPACE:
                    return NodeKindTest.NAMESPACE;
                default:
                    return NodeKindTest.ELEMENT;
            }
        }
        return this;
    }

    /**
     * Convert this RawNodeTest to an executable node test knowing the
     * actual origin node. This method can take advantage of the actual
     * tree model used, e.g. if the origin is a node in a TinyTree.
     *
     * @param origin the origin node
     * @return a refined NodeTest.
     */
    @Override
    public NodePredicate cook(GNode origin) {
        if (origin instanceof JNode) {
            return (node) -> true;
        }
        if (origin instanceof NodeInfo) {
            switch (defaultNodeKind) {
                case Type.ATTRIBUTE:
                    return NodeKindTest.ATTRIBUTE;
                case Type.NAMESPACE:
                    return NodeKindTest.NAMESPACE;
                default:
                    return NodeKindTest.ELEMENT;
            }
        }
        return this;
    }
    
    @Override
    public String toString() {
        return "*";
    }
}


