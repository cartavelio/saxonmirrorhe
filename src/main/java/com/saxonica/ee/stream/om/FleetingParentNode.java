package com.saxonica.ee.stream.om;

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
}

// Copyright (c) 2012-2014 Saxonica.