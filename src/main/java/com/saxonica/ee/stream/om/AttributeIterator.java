package com.saxonica.ee.stream.om;

import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.iter.LookaheadIterator;
import net.sf.saxon.type.Type;

import java.util.EnumSet;
import java.util.Iterator;
import java.util.function.BiFunction;


/**
 * AttributeIterator is an enumeration of all the attribute nodes of an Element. This
 * class is used only in conjunction with a FleetingElementNode, that is an element node
 * in a streamed document.
 */

final public class AttributeIterator implements AxisIterator, LookaheadIterator {

    private final NodeInfo element;
    private final Iterator<AttributeInfo> attributeIter;

    private final BiFunction<NodeInfo, AttributeInfo, NodeInfo> attributeMaker;

    /**
     * Constructor
     *
     * @param element:  the element whose attributes are required. This may be any type of node,
     *                  but if it is not an element the enumeration will be empty
     */

    public AttributeIterator(NodeInfo element, AttributeMap attributes, BiFunction<NodeInfo, AttributeInfo, NodeInfo> attributeMaker) {
        if (element.getNodeKind() != Type.ELEMENT) {
            throw new IllegalArgumentException("Not an element node");
        }
        this.element = element;
        this.attributeMaker = attributeMaker;
        attributeIter = attributes.iterator();
    }

    /**
     * Test if there are mode nodes still to come.
     * ("elements" is used here in the sense of the Java enumeration class, not in the XML sense)
     */

    @Override
    public boolean hasNext() {
        return attributeIter.hasNext();
    }

    /**
     * Get the next node in the iteration, or null if there are no more.
     */

    /*@Nullable*/
    @Override
    public NodeInfo next() {
        if (attributeIter.hasNext()) {
            AttributeInfo info = attributeIter.next();
            return attributeMaker.apply(element, info);
        } else {
            return null;
        }
    }

    /**
     * Get properties of this iterator, as a bit-significant integer.
     *
     * @return the properties of this iterator. This will be some combination of
     *         properties such as {@link Property#GROUNDED}, {@link Property#LAST_POSITION_FINDER},
     *         and {@link Property#LOOKAHEAD}. It is always
     *         acceptable to return the value zero, indicating that there are no known special properties.
     *         It is acceptable for the properties of the iterator to change depending on its state.
     */

    @Override
    public EnumSet<Property> getProperties() {
        return EnumSet.of(Property.LOOKAHEAD);
    }


}

// Copyright (c) 2011-2020 Saxonica Limited