package com.saxonica.ee.stream.om;

import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.pattern.NameTest;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.iter.LookaheadIterator;
import net.sf.saxon.type.Type;


/**
 * AttributeIterator is an enumeration of all the attribute nodes of an Element. This
 * class is used only in conjunction with a FleetingElementNode, that is an element node
 * in a streamed document.
 */

final class AttributeIterator implements AxisIterator, LookaheadIterator<NodeInfo> {

    private FleetingElementNode element;
    private AttributeCollection attributes;
    private NodeTest nodeTest;
    private FleetingNode next;
    private int index;
    private int length;

    /**
     * Constructor
     *
     * @param element:  the element whose attributes are required. This may be any type of node,
     *                  but if it is not an element the enumeration will be empty
     * @param nodeTest: condition to be applied to the names of the attributes selected
     */

    public AttributeIterator(FleetingElementNode element, AttributeCollection attributes, NodeTest nodeTest) {

        this.nodeTest = nodeTest;

        if (element.getNodeKind() == Type.ELEMENT) {
            this.element = element;
            this.attributes = attributes;
            index = 0;

            if (nodeTest instanceof NameTest) {
                NameTest test = (NameTest) nodeTest;
                index = attributes.getIndexByFingerprint(test.getFingerprint());

                if (index < 0) {
                    next = null;
                } else {
                    next = makeAttributeNode(element, attributes, index);
                    index = 0;
                    length = 0; // force iteration to select one node only
                }

            } else {
                index = 0;
                length = attributes.getLength();
                advance();
            }
        } else {      // if it's not an element, or if we're not looking for attributes,
            // then there's nothing to find
            next = null;
            index = 0;
            length = 0;
        }
    }

    /**
     * Test if there are mode nodes still to come.
     * ("elements" is used here in the sense of the Java enumeration class, not in the XML sense)
     */

    public boolean hasNext() {
        return next != null;
    }

    /**
     * Get the next node in the iteration, or null if there are no more.
     */

    /*@Nullable*/
    public NodeInfo next() {
        if (next == null) {
            return null;
        } else {
            NodeInfo current = next;
            advance();
            return current;
        }
    }

    /**
     * Move to the next node in the enumeration.
     */

    private void advance() {
        while (true) {
            if (index >= length) {
                next = null;
                return;
            } else {
                next = makeAttributeNode(element, attributes, index);
                index++;
                if (nodeTest.matchesNode(next)) {
                    return;
                }
            }
        }
    }

    /**
     * Get properties of this iterator, as a bit-significant integer.
     *
     * @return the properties of this iterator. This will be some combination of
     *         properties such as {@link #GROUNDED}, {@link #LAST_POSITION_FINDER},
     *         and {@link #LOOKAHEAD}. It is always
     *         acceptable to return the value zero, indicating that there are no known special properties.
     *         It is acceptable for the properties of the iterator to change depending on its state.
     */

    public int getProperties() {
        return LOOKAHEAD;
    }

    /**
     * Make an attribute node
     *
     * @param element    the containing element
     * @param attributes
     * @param index
     * @return the attribute node
     */

    private static FleetingNode makeAttributeNode(FleetingElementNode element, AttributeCollection attributes, int index) {
        FleetingNode attr = new FleetingNode();
        attr.setParent(element);
        attr.setNodeKind(Type.ATTRIBUTE);
        attr.setNodeName(attributes.getNodeName(index));
        attr.setTypeAnnotation(attributes.getTypeAnnotation(index));
        attr.setStringValue(attributes.getValue(index));
        return attr;
    }
}

// Copyright (c) 2011 Saxonica Limited.