package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.TraceExpression;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.lib.TraceListener;
import net.sf.saxon.om.Item;
import net.sf.saxon.trace.InstructionInfo;
import net.sf.saxon.trans.XPathException;

/**
 * Implement streaming of the trace instruction injected into the code when compiled with tracing enabled.
 * Not to be confused with the fn:trace function, though the implementation is similar.
 */
public class TraceExprAdjunct extends TransmissionAdjunct implements FeedMaker {

    public FeedMaker getFeedMaker(int arg) throws XPathException {
        return arg == 0 ? this : super.getFeedMaker(arg);
    }

    public Feed makeItemFeed(final WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
        return new TraceFeed((TraceExpression)getExpression(), out, context);
    }

    public static int sequenceNr = 0;

    /**
     * Streamed trace feed where the streamed argument is the first argument
     */

    private static class TraceFeed extends ItemFeed {

        InstructionInfo info;
        TraceListener listener;

        public TraceFeed(TraceExpression exp, Feed result, XPathContext context) {
            super(exp, result, context);
        }

        /**
         * Open the watch. This is done at the point where it first starts watching for nodes
         * that match the selection.
         *
         * @throws XPathException
         *          May be raised if a dynamic error occurs
         * @param terminator
         */
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            TraceExpression expr = (TraceExpression)getExpression();
            info = expr.getInstructionInfo();
            if (getContext().getController().isTracing()) {
                listener = getContext().getController().getTraceListener();
            }
        }

        /**
         * Signal that a node (other than a document or element node) that matches the selection
         * has been found, or that a grounded item is being passed to the next expression in the
         * evaluation pipeline.
         *
         * @param item the node that was matched.
         * @throws XPathException
         *          May be raised if a dynamic error occurs
         */
        public void processItem(Item<?> item) throws XPathException {
            if (listener != null) {
                listener.enter(info, getContext());
            }
            getResult().processItem(item);
            if (listener != null) {
                listener.leave(info);
            }
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
            if (listener != null) {
                listener.enter(info, getContext());
            }
            return getResult().startSelectedParentNode(node, locationId);
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
            if (listener != null) {
                listener.leave(info);
            }
            getResult().endSelectedParentNode(locationId);
        }
        
    }


}

// Copyright (c) 2018 Saxonica Ltd