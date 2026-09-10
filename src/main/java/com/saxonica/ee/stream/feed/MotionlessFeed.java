package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;

/**
 * A feed for an expression that typically accesses the context item but does not consume the input stream.
 * This class only ever appears as the first thing triggered by a watch. It evaluates the non-consuming
 * expression immediately when the start event is reported, supplying the items that result from the
 * evaluation to the next feed in the pipeline.
 */
public class MotionlessFeed extends ItemFeed {

    private final Expression expression;
    private boolean selected;

    public MotionlessFeed(Expression expression, ItemFeed result, XPathContext context) {
        super(expression, result, context);
        this.expression = expression;
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
     * for the matched element, and ending with the endElement event for that element; or null if this
     * feature is not required.
     * @throws net.sf.saxon.trans.XPathException May be raised if a dynamic error occurs
     */
    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        //System.err.println("Motionless start " + node.toShortString());
        SequenceIterator iter = expression.iterate(getContext());
        for (Item item; (item = iter.next()) != null; ) {
            if (item instanceof FleetingParentNode) {
                // We're assuming that if we process the streaming node, we only do it once.
                // Test case su-shallow-descent-004
                selected = true;
                return getResultFeed().startSelectedParentNode((FleetingParentNode) item, locationId);
            } else {
                // See bug 4128
                getResultFeed().append(item);
            }
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
        // This path is probably unused - MHK 2019-02-02
        SequenceIterator iter = expression.iterate(getContext());
        processItems(iter, getNextOutputter());
    }

    @Override
    public void endSelectedParentNode(Location locationId) throws XPathException {
        if (selected) {
            getResultFeed().endSelectedParentNode(locationId);
            selected = false;
        }
    }
}

// Copyright (c) 2018-2023 Saxonica Limited