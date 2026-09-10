package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingDocumentNode;
import com.saxonica.ee.stream.om.FleetingElementNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.SequenceReceiver;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NameOfNode;
import net.sf.saxon.om.NamespaceBinding;
import net.sf.saxon.trans.XPathException;

import java.util.Stack;

/**
 * Appends the output of an ItemFeed to a SequenceReceiver
 */
public class DecomposingFeed implements Feed {

    private Receiver receiver;
    private Stack<Boolean> nodeKindStack = new Stack<Boolean>();

    public DecomposingFeed(Receiver receiver) {
        this.receiver = receiver;
    }

    /**
     * Get the pipeline configuration
     *
     * @return the pipeline configuration
     */

    public PipelineConfiguration getPipelineConfiguration() {
        return receiver.getPipelineConfiguration();
    }

    /**
     * Get the destination receiver
     * @return the receiver to which events are being written to represent the results of this instruction
     */

    public Receiver getReceiver() {
        return receiver;
    }

    /**
     * Set the destination receiver
     * @param out the receiver to which events should henceforth be written
     */

    public void setReceiver(SequenceReceiver out) {
        receiver = out;
    }

    /**
     * Signal the start of a sequence of items
     *
     * @throws net.sf.saxon.trans.XPathException
     *          to report any failure
     * @param terminator
     */

    public void open(Terminator terminator) throws XPathException {

    }

    /**
     * Signal that an element or document node has been found that matches the selection that this
     * Watch is looking for. This method is called by the WatchManager while
     * processing the startElement or startDocument event that matches the selection.
     *
     * @param node       the element or document node whose start event has been matched
     * @param locationId the location associated with the element or document node (may be
     *                   the location of the instruction that created it)
     * @return a Receiver to be notified of all events starting with the startElement/startDocument event
     *         for the matched element, and ending with the endElement event for that element; or null if this
     *         feature is not required.
     * @throws net.sf.saxon.trans.XPathException
     *          May be raised if a dynamic error occurs
     */
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        boolean isDocNode = node instanceof FleetingDocumentNode;
        nodeKindStack.push(isDocNode);
        if (isDocNode) {
            receiver.startDocument(0);
        } else {
            FleetingElementNode element = (FleetingElementNode)node;
            receiver.startElement(NameOfNode.makeName(element), node.getSchemaType(), locationId, 0);
            AttributeCollection atts = element.getAttributes();
            for (int i=0; i<atts.getLength(); i++) {
                receiver.attribute(atts.getNodeName(i), atts.getTypeAnnotation(i), atts.getValue(i), locationId, 0);
            }
            NamespaceBinding[] namespaces = element.getDeclaredNamespaces(null);
            for (NamespaceBinding nb : namespaces) {
                receiver.namespace(nb, 0);
            }
        }
        return null;
    }

    /**
     * Signal that the endElement event has occurred for the element whose startElement
     * event caused the Watch to be activated.
     *
     * @param locationId the location of the element
     * @throws net.sf.saxon.trans.XPathException
     *          May be raised if a constraint implemented by this
     *          Watch is violated
     */
    public void endSelectedParentNode(Location locationId) throws XPathException {
        boolean isDocNode = nodeKindStack.pop();
        if (isDocNode) {
            receiver.endDocument();
        } else {
            receiver.endElement();
        }
    }

    /**
     * Supply an item to the feed
     *
     * @param item the item being fed
     * @throws net.sf.saxon.trans.XPathException
     *          to report any failure
     */

    public void processItem(Item<?> item) throws XPathException {
        receiver.append(item);
    }

    /**
     * Signal the end of the sequence of items
     *
     * @throws net.sf.saxon.trans.XPathException
     *          to report any failure
     */

    public void close() throws XPathException {

    }

}

// Copyright (c) 2009 Saxonica Limited.


