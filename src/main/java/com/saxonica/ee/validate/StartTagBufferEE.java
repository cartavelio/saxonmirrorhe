package com.saxonica.ee.validate;

import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.StartTagBuffer;
import net.sf.saxon.expr.parser.ExplicitLocation;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.tiny.Statistics;
import net.sf.saxon.tree.tiny.TinyBuilder;
import net.sf.saxon.tree.util.AttributeCollectionImpl;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.Untyped;

import java.util.Stack;

/**
 * This class handles attribute inheritance. It's a ProxyReceiver inserted into the validation pipeline
 * that received notification of all events.
 */
public class StartTagBufferEE extends StartTagBuffer {

    private Stack<AttributeCollection> stack = new Stack<AttributeCollection>();
    private boolean empty = true;

    // We can make an element node representing this start tag for use in evaluation xs:alternative
    // (conditional type assignment) expressions. Once this is done, we keep it.

    private NodeInfo elementNode;

    public StartTagBufferEE(Receiver next) {
        super(next);
    }

    /**
     * Notify the start of an element
     *  @param nameCode   integer code identifying the name of the element within the name pool.
     * @param typeCode   integer code identifying the element's type within the name pool.
     * @param location
     * @param properties properties of the element node
     */
    @Override
    public void startElement(NodeName nameCode, SchemaType typeCode, Location location, int properties) throws XPathException {
        stack.push(AttributeCollectionImpl.EMPTY_ATTRIBUTE_COLLECTION);
        elementNode = null;
        super.startElement(nameCode, typeCode, location, properties);
    }

    /**
     * End of element
     */
    @Override
    public void endElement() throws XPathException {
        stack.pop();
        super.endElement();
    }

    /**
     * Notify an inheritable attribute
     *
     * @param nameCode the name of the attribute
     * @param value    the value of the attribute
     */

    public void notifyInheritableAttribute(NodeName nameCode, String value, Location location, int properties) {
        AttributeCollection atts = stack.peek();
        if (atts == AttributeCollectionImpl.EMPTY_ATTRIBUTE_COLLECTION) {
            stack.pop();
            atts = new AttributeCollectionImpl(getConfiguration());
            stack.push(atts);
        }
        ((AttributeCollectionImpl) atts).addAttribute(nameCode, BuiltInAtomicType.UNTYPED_ATOMIC, value, location, properties);
        empty = false;
    }

    /**
     * Get all inherited attributes
     *
     * @param atts a collection of attributes; inherited attributes will be added to this collection unless there
     *             is already an attribute in the collection with the same name
     */

    public void gatherInheritedAttributes(AttributeCollectionImpl atts) {
        for (int i = stack.size() - 1; i >= 0; i--) {
            AttributeCollection ac = stack.get(i);
            for (int j = 0; j < ac.getLength(); j++) {
                NodeName nc = ac.getNodeName(j);
                if (atts.findByNodeName(nc) == -1) {
                    atts.addAttribute(nc, BuiltInAtomicType.UNTYPED_ATOMIC, ac.getValue(j), ac.getLocation(j), ac.getProperties(j));
                }
            }
        }
    }

    /**
     * Ask whether there are any attributes to inherit.
     * (This is an optimization, returning true when there are none does not matter)
     */

    public boolean hasInheritedAttributes() {
        return !empty;
    }

    /**
     * Get an element node representing the element whose start tag this is, as required
     * for implementing conditional type assignment
     *
     * @return an element node. This contains all the required namespaces and attributes, and has no children;
     *         it is untyped, as are the attributes.
     */

    /*@Nullable*/
    public NodeInfo createSkeletonElementNode() throws XPathException {
        if (elementNode == null) {
            int len = bufferedAttributes.getLength();
            TinyBuilder builder = new TinyBuilder(getPipelineConfiguration());
            builder.setSystemId(systemId);
            Statistics stats = new Statistics(2, len + 2, namespacesSize + 2, 16);
            builder.setStatistics(stats);
            builder.open();
            builder.startElement(elementNameCode, Untyped.getInstance(), ExplicitLocation.UNKNOWN_LOCATION, 0);
            for (int i = 0; i < namespacesSize; i++) {
                builder.namespace(namespaces[i], 0);
            }

            AttributeCollectionImpl allAttributes = bufferedAttributes;
            if (hasInheritedAttributes()) {
                if (bufferedAttributes.getLength() == 0) {
                    allAttributes = new AttributeCollectionImpl(getConfiguration());
                } else {
                    allAttributes = AttributeCollectionImpl.copy(bufferedAttributes);
                }
                gatherInheritedAttributes(allAttributes);
            }

            for (int i = 0; i < allAttributes.getLength(); i++) {
                builder.attribute(allAttributes.getNodeName(i), BuiltInAtomicType.UNTYPED_ATOMIC,
                        allAttributes.getValue(i), ExplicitLocation.UNKNOWN_LOCATION, 0);
            }

            builder.startContent();
            builder.endElement();
            elementNode = builder.getCurrentRoot();
        }
        return elementNode;
    }

}

// Copyright (c) 2010 Saxonica Limited.



