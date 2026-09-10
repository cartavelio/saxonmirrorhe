package com.saxonica.serialize;

import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.lib.SaxonOutputKeys;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;

import java.util.*;

/**
 * AttributeSorter: This filter sorts attributes into the order requested using the
 * saxon:attribute-order serialization property
 */


public class AttributeSorter extends ProxyReceiver  {

    private Map<NodeName, Integer> knownAttributes;             // mapping of named attributes to relative position

    private final static int MIDDLE_GROUND = 10000000;

    /**
     * Create an attribute sorter
     *
     * @param next the next receiver in the pipeline
     */

    public AttributeSorter(Receiver next) {
        super(next);
    }

    /**
     * Set the properties for this attribute sorter
     *
     * @param details the output properties
     */

    public void setOutputProperties(/*@NotNull*/ Properties details) {
        String attOrder;
        if ("yes".equals(details.getProperty(SaxonOutputKeys.CANONICAL))) {
            attOrder = "";
        } else {
            attOrder = details.getProperty(SaxonOutputKeys.ATTRIBUTE_ORDER);
            if (attOrder == null) {
                attOrder = ""; // just in case
            }
        }
        knownAttributes = new HashMap<>();
        int pos = 0;
        StringTokenizer st2 = new StringTokenizer(attOrder, " \t\n\r", false);
        while (st2.hasMoreTokens()) {
            String expandedName = st2.nextToken();
            if (expandedName.equals("*")) {
                pos += MIDDLE_GROUND;
            } else {
                StructuredQName sq = StructuredQName.fromClarkName(expandedName);
                knownAttributes.put(new FingerprintedQName("", sq.getNamespaceUri(), sq.getLocalPart()), pos++);
            }
        }
    }

    /**
     * Notify the start of an element
     */
    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributesIn, NamespaceMap namespaces,
                             Location location, int properties)
            throws XPathException {
        if (attributesIn.size() <= 1) {
            nextReceiver.startElement(elemName, type, attributesIn, namespaces, location, properties);
        } else {
            ArrayList<AttributeInfo> attributes = attributesIn.asList();
            attributes.sort((a, b) -> {
                NodeName n0 = a.getNodeName();
                NodeName n1 = b.getNodeName();
                int r0 = knownAttributes.getOrDefault(n0, Integer.MIN_VALUE);
                int r1 = knownAttributes.getOrDefault(n1, Integer.MIN_VALUE);
                if (r0 != Integer.MIN_VALUE) {
                    if (r1 == Integer.MIN_VALUE) {
                        return Integer.compare(r0, MIDDLE_GROUND);
                    } else {
                        return Integer.compare(r0, r1);
                    }
                } else if (r1 != Integer.MIN_VALUE) {
                    return Integer.compare(MIDDLE_GROUND, r1);
                } else {
                    // both null: sort on URI/local-name
                    int x = n0.getNamespaceUri().toString().compareTo(n1.getNamespaceUri().toString());
                    if (x == 0) {
                        x = n0.getLocalPart().compareTo(n1.getLocalPart());
                    }
                    return x;
                }
            });

            AttributeMap sorted = SequenceTool.attributeMapFromList(attributes);
            nextReceiver.startElement(elemName, type, sorted, namespaces, location, properties);
        }


    }
}

// Copyright (c) 2017-2022 Saxonica Limited

