package com.saxonica.serialize;

import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.expr.sort.GenericSorter;
import net.sf.saxon.expr.sort.Sortable;
import net.sf.saxon.lib.SaxonOutputKeys;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.AttributeCollectionImpl;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;

import java.util.HashMap;
import java.util.Map;
import java.util.Properties;
import java.util.StringTokenizer;

/**
 * AttributeSorter: This filter sorts attributes into the order requested using the
 * saxon:attribute-order serialization property
 *
 * @author Michael Kay
 */


public class AttributeSorter extends ProxyReceiver implements Sortable {

    private Map<NodeName, Integer> knownAttributes;             // mapping of named attributes to relative position
    protected AttributeCollectionImpl attributes;

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
        if ("yes".equals(details.get(SaxonOutputKeys.CANONICAL))) {
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
                knownAttributes.put(new FingerprintedQName("", sq.getURI(), sq.getLocalPart()), pos++);
            }
        }
    }

    /**
     * Notify the start of an element
     * @param elemName   integer code identifying the name of the element within the name pool.
     * @param typeCode   integer code identifying the element's type within the name pool.
     * @param location   the element's line &amp; column number
     * @param properties properties of the element node
     */
    @Override
    public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
        attributes = new AttributeCollectionImpl(getConfiguration());
        super.startElement(elemName, typeCode, location, properties);
    }

    /**
     * Notify an attribute. Attributes are notified after the startElement event, and before any
     * children. Namespaces and attributes may be intermingled.
     *
     * @param nameCode   The name of the attribute
     * @param typeCode   The type of the attribute
     * @param locationId  the attribute's line &amp; column number
     * @param properties Bit significant value. The following bits are defined:
     *                   <dl>
     *                   <dd>DISABLE_ESCAPING</dd>    <dt>Disable escaping for this attribute</dt>
     *                   <dd>NO_SPECIAL_CHARACTERS</dd>      <dt>Attribute value contains no special characters</dt>
     *                   </dl>
     * @throws IllegalStateException: attempt to output an attribute when there is no open element
     *                                start tag
     */
    @Override
    public void attribute(NodeName nameCode, SimpleType typeCode, CharSequence value, Location locationId, int properties) {
        attributes.addAttribute(nameCode, typeCode, value.toString(), locationId, properties);
    }

    /**
     * Notify the start of the content, that is, the completion of all attributes and namespaces.
     * Note that the initial receiver of output from XSLT instructions will not receive this event,
     * it has to detect it itself. Note that this event is reported for every element even if it has
     * no attributes, no namespaces, and no content.
     */
    @Override
    public void startContent() throws XPathException {
        if (attributes.getLength() > 1) {
            GenericSorter.quickSort(0, attributes.getLength(), this);
        }
        for (int i = 0; i < attributes.getLength(); i++) {
            nextReceiver.attribute(
                    attributes.getNodeName(i),
                    attributes.getTypeAnnotation(i),
                    attributes.getValue(i),
                    attributes.getLocation(i),
                    attributes.getProperties(i));
        }
        super.startContent();
    }

    /**
     * Compare two objects within this Sortable, identified by their position.
     *
     * @return &lt;0 if obj[a]&lt;obj[b], 0 if obj[a]=obj[b], &gt;0 if obj[a]&gt;obj[b]
     */
    public int compare(int a, int b) {
        NodeName n0 = attributes.getNodeName(a);
        NodeName n1 = attributes.getNodeName(b);
        Integer r0 = knownAttributes.get(n0);
        Integer r1 = knownAttributes.get(n1);
        if (r0 != null) {
            if (r1 == null) {
                return r0.compareTo(MIDDLE_GROUND);
            } else {
                return r0.compareTo(r1);
            }
        } else if (r1 != null) {
            return Integer.compare(MIDDLE_GROUND, r1);
        } else {
            // both null: sort on URI/local-name
            int x = n0.getURI().compareTo(n1.getURI());
            if (x == 0) {
                x = n0.getLocalPart().compareTo(n1.getLocalPart());
            }
            return x;
        }
    }

    /**
     * Swap two objects within this Sortable, identified by their position.
     */
    public void swap(int a, int b) {
        attributes.swap(a, b);
    }
}

// Copyright (c) 2017-2018 Saxonica Limited

