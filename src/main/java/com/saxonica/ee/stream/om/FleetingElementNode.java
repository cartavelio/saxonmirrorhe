package com.saxonica.ee.stream.om;

import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.NamespaceNode;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.Type;

/**
 * Represents an element node available transiently within a streamed document
 */
public class FleetingElementNode extends FleetingParentNode {

    private NamespaceBinding[] localNamespaces;
    private AttributeCollection attributes;

    public FleetingElementNode(NodeName name, SchemaType type) {
        setNodeKind(Type.ELEMENT);
        setNodeName(name);
        setTypeAnnotation(type);
    }

    /**
     * Set the local namespace declarations for this element, that is, the namespace declarations
     * (and undeclarations) that actually appear on this element
     * @param namespaces the local namespace declarations
     */

    public void setLocalNamespaces(NamespaceBinding[] namespaces) {
        this.localNamespaces = namespaces;
    }

    /**
     * Set the attributes that appear on this element
     * @param attributes the element's attributes
     */

    public void setAttributes(AttributeCollection attributes) {
        this.attributes = attributes;
    }

    /**
     * Copy this node to a given Receiver.
     * <p>This method is primarily for internal use. It should not be considered a stable
     * part of the Saxon API.</p>
     *
     * @param out         the Receiver to which the node should be copied. It is the caller's
     *                    responsibility to ensure that this Receiver is open before the method is called
     *                    (or that it is self-opening), and that it is closed after use.
     * @param copyOptions a selection of the options defined in {@link CopyOptions}
     * @param locationId  If non-zero, identifies the location of the instruction
     *                    that requested this copy. If zero, indicates that the location information
     *                    for the original node is to be copied; in this case the Receiver must be
     * @throws UnsupportedOperationException (always)
     */

    public void copy(Receiver out, int copyOptions, Location locationId) throws XPathException {
        throw new UnsupportedOperationException("copy() is not supported for streamed " +
                                                        Err.wrap(getDisplayName(), Err.ELEMENT) +
                                                        " element node");
    }

    /**
     * Return an iteration over all the nodes reached by the given axis from this node
     * that match a given NodeTest
     *
     * @param axisNumber an integer identifying the axis; one of the constants
     *                   defined in class {@link net.sf.saxon.om.AxisInfo}
     * @param nodeTest   A condition to be satisfied by the returned nodes; nodes
     *                   that do not satisfy this condition are not included in the result
     * @return an AxisIterator that delivers the nodes reached by the axis in
     *         turn.  The nodes are returned in axis order (document order for a forwards
     *         axis, reverse document order for a reverse axis).
     * @throws UnsupportedOperationException if the namespace axis is
     *                                       requested and this axis is not supported for this implementation.
     * @see net.sf.saxon.om.AxisInfo
     * @since 8.4
     */

    public AxisIterator iterateAxis(byte axisNumber, NodeTest nodeTest) {
        switch (axisNumber) {
            case AxisInfo.ATTRIBUTE:
                if (attributes.getLength() == 0) {
                    return EmptyIterator.OfNodes.THE_INSTANCE;
                }
                return new AttributeIterator(this, attributes, nodeTest);

            case AxisInfo.NAMESPACE:
                return NamespaceNode.makeIterator(this, nodeTest);

            default:
                return super.iterateAxis(axisNumber, nodeTest);
        }

    }

    /**
     * Get the attributes defined on this element
     * @return the attributes in the form of an AttributeCollection
     */

    public AttributeCollection getAttributes() {
        return attributes;
    }

    /**
     * Get all namespace declarations and undeclarations defined on this element.
     * <p>This method is intended primarily for internal use. User applications needing
     * information about the namespace context of a node should use <code>iterateAxis(Axis.NAMESPACE)</code>.
     * (However, not all implementations support the namespace axis, whereas all implementations are
     * required to support this method.)</p>
     *
     * @param buffer If this is non-null, and the result array fits in this buffer, then the result
     *               may overwrite the contents of this array, to avoid the cost of allocating a new array on the heap.
     * @return An array of integers representing the namespace declarations and undeclarations present on
     *         this element. For a node other than an element, return null. Otherwise, the returned array is a
     *         sequence of namespace codes, whose meaning may be interpreted by reference to the name pool. The
     *         top half word of each namespace code represents the prefix, the bottom half represents the URI.
     *         If the bottom half is zero, then this is a namespace undeclaration rather than a declaration.
     *         The XML namespace is never included in the list. If the supplied array is larger than required,
     *         then the first unused entry will be set to -1.
     *         <p>For a node other than an element, the method returns null.</p>
     */

    public NamespaceBinding[] getDeclaredNamespaces(NamespaceBinding[] buffer) {
        return localNamespaces;
    }

    /**
     * Get the string value of a given attribute of this node
     *
     * @param uri   the namespace URI of the attribute name. Supply the empty string for an attribute
     *              that is in no namespace
     * @param local the local part of the attribute name.
     * @return the attribute value if it exists, or null if it does not exist. Always returns null
     *         if this node is not an element.
     * @since 9.4
     */
    @Override
    public String getAttributeValue(/*@NotNull*/ String uri, /*@NotNull*/ String local) {
        return attributes.getValue(uri, local);
    }

    /**
     * Get the index position of an attribute with given local name and fingerprint
     */

    public int getAttributeIndex(String uri, String local) {
        return attributes.getIndex(uri, local);
    }

    /**
     * Get the index position of a namespace node with given local name (=prefix
     */

    public int getNamespaceNodeIndex(String prefix) {
        int index=0;
        AxisIterator iter = iterateAxis(AxisInfo.NAMESPACE);
        NodeInfo node;
        while ((node = iter.next()) != null) {
            index++;
            if (node.getLocalPart().equals(prefix)) {
                return index;
            }
        }
        return -1;
    }

}

// Copyright (c) 2012 Saxonica Limited.

