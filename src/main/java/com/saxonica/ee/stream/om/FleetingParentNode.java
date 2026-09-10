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

}

// Copyright (c) 2012-2022 Saxonica Limited.