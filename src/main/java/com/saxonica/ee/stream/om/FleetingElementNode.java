package com.saxonica.ee.stream.om;

import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NameTest;
import net.sf.saxon.pattern.NodePredicate;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.StringView;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.NamespaceNode;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.iter.SingleNodeIterator;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.Type;

import java.util.Objects;

/**
 * Represents an element node available transiently within a streamed document
 */
public class FleetingElementNode extends FleetingParentNode {

    private NamespaceMap inScopeNamespaces;
    private AttributeMap attributeMap;

    public FleetingElementNode(NodeName name, SchemaType type) {
        setNodeKind(Type.ELEMENT);
        setNodeName(name);
        setTypeAnnotation(type);
    }

    public FleetingElementNode(NodeName name, SchemaType type, AttributeMap attributes, NamespaceMap namespaces) {
        setNodeKind(Type.ELEMENT);
        setNodeName(name);
        setTypeAnnotation(type);
        setAttributes(attributes);
        setNamespaceMap(namespaces);
    }

    /**
     * Set the in-scope namespaces for this element
     * @param namespaces the in-scope namespaces. This includes all namespace bindings that are
     *                   in scope, not just the local namespace declarations
     */

    public void setNamespaceMap(NamespaceMap namespaces) {
        this.inScopeNamespaces = namespaces;
    }


    @Override
    public NamespaceMap getAllNamespaces() {
        return inScopeNamespaces;
    }

    /**
     * Set the attributes that appear on this element
     * @param attributes the element's attributes
     */

    public void setAttributes(AttributeMap attributes) {
        this.attributeMap = attributes;
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

    @Override
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
     *                   defined in class {@link AxisInfo}
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

    @Override
    public AxisIterator iterateAxis(int axisNumber, NodePredicate nodeTest) {
        switch (axisNumber) {
            case AxisInfo.ATTRIBUTE:
                if (attributeMap.size() == 0) {
                    return EmptyIterator.ofNodes();
                } else if (nodeTest instanceof NameTest) {
                    AttributeInfo att = attributeMap.getByFingerprint(
                            ((NameTest)nodeTest).getFingerprint(), getConfiguration().getNamePool());
                    return att==null ? EmptyIterator.ofNodes() : SingleNodeIterator.makeIterator(makeAttributeNode(att));
                } else {
                    final AttributeIterator base = new AttributeIterator(this, attributeMap);
                    return new Navigator.AxisFilter(base, nodeTest);
                }

            case AxisInfo.NAMESPACE:
                return NamespaceNode.makeIterator(this, nodeTest);

            default:
                return super.iterateAxis(axisNumber, nodeTest);
        }

    }

    /**
     * Make an attribute node
     *
     * @param attInfo information about the attribute
     * @return the attribute node.
     */

    public FleetingNode makeAttributeNode(AttributeInfo attInfo) {
        Objects.requireNonNull(attInfo);
        FleetingNode attr = new FleetingNode();
        attr.setParent(this);
        attr.setNodeKind(Type.ATTRIBUTE);
        attr.setNodeName(attInfo.getNodeName());
        attr.setTypeAnnotation(attInfo.getType());
        attr.setStringValue(StringView.tidy(attInfo.getValue()));
        return attr;
    }

    /**
     * Get the attributes defined on this element
     * @return the attributes in the form of an AttributeCollection
     */

    public AttributeMap getAttributes() {
        return attributeMap;
    }

    /**
     * Get all in-scope namespaces defined on this element.
     *
     * @return a {@code NamespaceMap} containing all the in-scope namespaces for an element (not just
     * those declared locally on the element itself). Returns null if the node is
     * not an element.
     */

    /*@Nullable*/
    @Override
    public NamespaceMap getInScopeNamespaces() {
        return inScopeNamespaces;
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
        return attributeMap.getValue(uri, local);
    }

    /**
     * Get the index position of an attribute with given local name and fingerprint
     */

    public int getAttributeIndex(String uri, String local) {
        int i=0;
        for (AttributeInfo att : attributeMap) {
            final NodeName nn = att.getNodeName();
            if (nn.hasURI(uri) && nn.getLocalPart().equals(local)) {
                return i;
            } else {
                i++;
            }
        }
        return Integer.MAX_VALUE;
    }

    public int compareAttributeOrder(NodeName one, NodeName two) {
        if (one.equals(two)) {
            return 0;
        } else {
            for (AttributeInfo att : attributeMap) {
                final NodeName nn = att.getNodeName();
                if (nn.equals(one)) {
                    return -1;
                } else if (nn.equals(two)) {
                    return +1;
                }
            }
            throw new IllegalStateException("Attribute not found in attribute list");
        }
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

    /**
     * Notify the event to a Receiver
     *
     * @param out the receiver to be notified
     */
    @Override
    public void notify(Receiver out) throws XPathException {
        out.startElement(getNodeName(), getSchemaType(), getAttributes(), getAllNamespaces(), this, ReceiverOption.NONE);
    }
}

// Copyright (c) 2012-2022 Saxonica Limited

