package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.SequenceReceiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;

/**
 * Appends the output of an ItemFeed to a SequenceReceiver.
 *
 * This works as follows. When the startElement event for a matched node is reported, this feed simply returns
 * a Receiver to the WatchManager. The WatchManager then notifies all events from this startElement to the
 * matching endElement (inclusive) to that Receiver. So all the events for the matched node and its descendants
 * are passed on unchanged.
 *
 * The Receiver that is actually returned to the WatchManager is the Receiver obtained from the next expression
 * in the push pipeline, but with a simple filter attached that suppresses open() and close() events. Without
 * this, the WatchManager would close the receiver when the endElement event occurs, which is inappropriate
 * when the expression selects a sequence of elements.
 */
public class FeedToReceiver extends ItemFeed {

    private Receiver receiver;
    private NoOpenCloseReceiver innerReceiver;

    public FeedToReceiver(Feed out, XPathContext context) {
        super(out, context);
    }

    /**
     * Get the pipeline configuration
     *
     * @return the pipeline configuration
     */

    public PipelineConfiguration getPipelineConfiguration() {
        return getResult().getPipelineConfiguration();
    }

    /**
     * Get the destination receiver
     * @return the receiver to which events are being written to represent the results of this instruction
     */

    public SequenceReceiver getReceiver() {
        return innerReceiver;
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
     * @throws XPathException
     *          to report any failure
     * @param terminator
     */

    public void open(Terminator terminator) throws XPathException {
        getResult().open(terminator);
        receiver = getResult().getReceiver();
        innerReceiver = new NoOpenCloseReceiver(receiver);
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
     * @throws XPathException
     *          May be raised if a dynamic error occurs
     */
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        return innerReceiver;
    }

    /**
     * Signal that the endElement event has occurred for the element whose startElement
     * event caused the Watch to be activated.
     *
     * @param locationId the location of the element
     * @throws XPathException
     *          May be raised if a constraint implemented by this
     *          Watch is violated
     */
    public void endSelectedParentNode(Location locationId) throws XPathException {
        // No action: the WatchManager will notify the endElement event to the Receiver
    }

    /**
     * Supply an item to the feed
     *
     * @param item the item being fed
     * @throws XPathException
     *          to report any failure
     */

    public void processItem(Item<?> item) throws XPathException {
        receiver.append(item);
    }

    /**
     * Signal the end of the sequence of items
     *
     * @throws XPathException
     *          to report any failure
     */

    public void close() throws XPathException {
        getResult().close();
    }

    /**
     * A filter on the Receiver pipeline to suppress open() and close() events
     */

    private static class NoOpenCloseReceiver extends ProxyReceiver {
        public NoOpenCloseReceiver(Receiver nextReceiver) {
            super(nextReceiver);
        }

        @Override
        public void open() throws XPathException {
            //
        }

        @Override
        public void close() throws XPathException {
            //
        }
    }

}

// Copyright (c) 2018 Saxonica Limited.


