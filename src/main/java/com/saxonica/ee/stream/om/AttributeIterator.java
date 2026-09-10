package com.saxonica.ee.stream.om;

import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.iter.LookaheadIterator;

import java.util.List;


/**
 * AttributeIterator iterates over all the attribute nodes of a streamed Element node.
 */

final public class AttributeIterator implements AxisIterator, LookaheadIterator {

    private final FleetingElementNode element;
    private final List<AttributeInfo> attributeList;
    private int position;

    /**
     * Constructor
     *
     * @param element   the element whose attributes are required.
     */

    public AttributeIterator(FleetingElementNode element, AttributeMap attributes) {
        this.element = element;
        this.attributeList = attributes.asList();
        this.position = 0;
    }

    @Override
    public boolean supportsHasNext() {
        return true;
    }


    /**
     * Test if there are more nodes still to come.
     * ("elements" is used here in the sense of the Java enumeration class, not in the XML sense)
     */

    @Override
    public boolean hasNext() {
        return position < attributeList.size();
    }

    /**
     * Get the next node in the iteration, or null if there are no more.
     */

    /*@Nullable*/
    @Override
    public NodeInfo next() {
        if (hasNext()) {
            AttributeInfo info = attributeList.get(position++);
            return element.makeAttributeNode(info);
        } else {
            return null;
        }
    }


}

// Copyright (c) 2011-2023 Saxonica Limited