package com.saxonica.ee.validate;

import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.tiny.Statistics;
import net.sf.saxon.tree.tiny.TinyBuilder;
import net.sf.saxon.tree.util.IndexedStack;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.Untyped;

import java.util.Stack;

/**
 * This class handles attribute inheritance. It's a ProxyReceiver inserted into the validation pipeline
 * that received notification of all events.
 */
public class AttributeInheritor extends ProxyReceiver {

    private AttributeMap bufferedAttributes;
    private final IndexedStack<AttributeMap> inheritedAttributeStack = new IndexedStack<>();
    private final Stack<NodeName> elementNameStack = new Stack<>();
    private final Stack<NamespaceMap> namespaceMapStack = new Stack<>();
    private boolean empty = true;

    // We can make an element node representing this start tag for use in evaluation xs:alternative
    // (conditional type assignment) expressions. Once this is done, we keep it.

    private NodeInfo elementNode;

    public AttributeInheritor(Receiver next) {
        super(next);
    }

    /**
     * Notify the start of an element
     */
    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) throws XPathException {
        bufferedAttributes = attributes;
        inheritedAttributeStack.push(EmptyAttributeMap.getInstance());
        elementNameStack.push(elemName);
        namespaceMapStack.push(namespaces);
        elementNode = null;
        super.startElement(elemName, type, attributes, namespaces, location, properties);
    }

    /**
     * End of element
     */
    @Override
    public void endElement() throws XPathException {
        inheritedAttributeStack.pop();
        elementNameStack.pop();
        namespaceMapStack.pop();
        super.endElement();
    }

    /**
     * Notify an inheritable attribute
     *
     * @param nameCode the name of the attribute
     * @param value    the value of the attribute
     */

    public void notifyInheritableAttribute(NodeName nameCode, String value, Location location, int properties) {
        AttributeMap atts = inheritedAttributeStack.pop();
        atts = atts.put(new AttributeInfo(nameCode, BuiltInAtomicType.UNTYPED_ATOMIC, value, location, properties));
        inheritedAttributeStack.push(atts);
        empty = false;
    }

    /**
     * Get all inherited attributes
     *
     * @param initialAtts a collection of attributes; inherited attributes will be added to this collection unless there
     *             is already an attribute in the collection with the same name
     * @return an expanded collection of attributes that also contains inherited attributes
     */

    public AttributeMap gatherInheritedAttributes(AttributeMap initialAtts) {
        for (int i = inheritedAttributeStack.size() - 1; i >= 0; i--) {
            AttributeMap ac = inheritedAttributeStack.get(i);
            for (AttributeInfo att : ac) {
                NodeName nc = att.getNodeName();
                if (initialAtts.get(nc) == null) {
                    AttributeInfo newAtt = new AttributeInfo(
                            nc, BuiltInAtomicType.UNTYPED_ATOMIC, att.getValue(), att.getLocation(), att.getProperties());
                    initialAtts = initialAtts.put(newAtt);
                }
            }
        }
        return initialAtts;
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
    NodeInfo createSkeletonElementNode() throws XPathException {
        if (elementNode == null) {
            int len = bufferedAttributes.size();
            TinyBuilder builder = new TinyBuilder(getPipelineConfiguration());
            builder.setSystemId(systemId);
            Statistics stats = new Statistics(2, len + 2, 20, 16);
            builder.setStatistics(stats);
            builder.open();

            AttributeMap allAttributes = bufferedAttributes;
            if (hasInheritedAttributes()) {
                allAttributes = gatherInheritedAttributes(allAttributes);
            }

            builder.startElement(elementNameStack.peek(), Untyped.getInstance(),
                                 allAttributes, namespaceMapStack.peek(),
                                 Loc.NONE, ReceiverOption.NONE);
            builder.endElement();
            elementNode = builder.getCurrentRoot();
        }
        return elementNode;
    }

}

// Copyright (c) 2010-2022 Saxonica Limited



