package com.saxonica.ee.stream.om;

import net.sf.saxon.event.Receiver;
import net.sf.saxon.trans.XPathException;


/**
 * A streamed document or element node.
 */
public abstract class FleetingParentNode extends FleetingNode {

    private boolean hasChildren = false;

    /**
     * Ask whether the node has any children.
     * @return True if the node has one or more children
     */
    @Override
    public boolean hasChildNodes() {
        return hasChildren;
    }

    /**
     * Say whether the node has any children.
     * @param has true if the node has children
     */

    public void setHasChildren(boolean has) {
        hasChildren = has;
    }

    /**
     * Notify the event to a Receiver
     * @param out the receiver to be notified
     */

    public abstract void notify(Receiver out) throws XPathException;

    /**
     * Determine whether this is the same node as another node.
     * <p>Note that two different NodeInfo instances can represent the same conceptual node.
     * Therefore the "==" operator should not be used to test node identity. The equals()
     * method should give the same result as isSameNodeInfo(), but since this rule was introduced
     * late it might not apply to all implementations.</p>
     * <p>Note: a.isSameNodeInfo(b) if and only if generateId(a)==generateId(b).</p>
     * <p>This method has the same semantics as isSameNode() in DOM Level 3, but
     * works on Saxon NodeInfo objects rather than DOM Node objects.</p>
     *
     * @param other the node to be compared with this node
     * @return true if this NodeInfo object and the supplied NodeInfo object represent
     * the same node in the tree.
     */
    @Override
    public boolean equals(Object other) {
        return this == other;
    }

    @Override
    public int hashCode() {
        return System.identityHashCode(this);
    }
}

// Copyright (c) 2012-2023 Saxonica Limited.