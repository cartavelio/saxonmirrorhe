package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingDocumentNode;
import com.saxonica.ee.stream.om.FleetingElementNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NameOfNode;
import net.sf.saxon.trans.XPathException;

import java.util.Stack;

/**
 * Appends the output of an ItemFeed to an Outputter
 */
public class DecomposingFeed extends ItemFeed {

    private final Stack<Boolean> nodeKindStack = new Stack<>();

    public DecomposingFeed(Outputter receiver, XPathContext context) {
        // There is no next ItemFeed in the pipeline; all output goes to the supplied Outputter instead
        super(receiver, context, true);
    }

    /**
     * Signal the start of a sequence of items
     *
     * @throws net.sf.saxon.trans.XPathException
     *          to report any failure
     * @param terminator action to be taken on failure
     */

    @Override
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
    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        boolean isDocNode = node instanceof FleetingDocumentNode;
        nodeKindStack.push(isDocNode);
        if (isDocNode) {
            getNextOutputter().startDocument(ReceiverOption.NONE);
        } else {
            FleetingElementNode element = (FleetingElementNode)node;
            getNextOutputter().startElement(NameOfNode.makeName(element), node.getSchemaType(),
                                  locationId, ReceiverOption.NONE);
        }
        return null;
    }

    /**
     * Supply one item towards the streamed input of the expression
     *
     * @param item the item to be supplied
     */
    @Override
    public void append(Item item) throws XPathException {
        getNextOutputter().append(item);
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
    @Override
    public void endSelectedParentNode(Location locationId) throws XPathException {
        boolean isDocNode = nodeKindStack.pop();
        if (isDocNode) {
            getNextOutputter().endDocument();
        } else {
            getNextOutputter().endElement();
        }
    }

    /**
     * Signal the end of the sequence of items
     *
     * @throws net.sf.saxon.trans.XPathException
     *          to report any failure
     */

    @Override
    public void close() throws XPathException {
        //getNextOutputter().close();
    }

}

// Copyright (c) 2009-2023 Saxonica Limited


