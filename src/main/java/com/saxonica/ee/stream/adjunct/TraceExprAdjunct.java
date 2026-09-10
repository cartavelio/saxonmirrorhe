package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ComponentTracer;
import net.sf.saxon.expr.instruct.TraceExpression;
import net.sf.saxon.lib.TraceListener;
import net.sf.saxon.om.Item;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trace.Traceable;
import net.sf.saxon.trans.XPathException;

import java.util.Collections;

/**
 * Implement streaming of the trace instruction injected into the code when compiled with tracing enabled.
 * Not to be confused with the fn:trace function, though the implementation is similar.
 */
public class TraceExprAdjunct extends TransmissionAdjunct implements FeedMaker {

    @Override
    public FeedMaker getFeedMaker(int arg) throws XPathException {
        return arg == 0 ? this : super.getFeedMaker(arg);
    }

    @Override
    public ItemFeed makeItemFeed(final WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new TraceFeed(getExpression(), out, context);
    }

    /**
     * Streamed trace feed where the streamed argument is the first argument
     */

    private static class TraceFeed extends ItemFeed {

        Traceable info;
        TraceListener listener;

        TraceFeed(Expression exp, ItemFeed result, XPathContext context) {
            super(exp, result, context);
        }

        /**
         * Open the watch. This is done at the point where it first starts watching for nodes
         * that match the selection.
         *
         * @throws XPathException May be raised if a dynamic error occurs
         * @param terminator used to achieve early exit
         */
        @Override
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            Expression expr = getExpression();
            if (expr instanceof TraceExpression) {
                info = ((TraceExpression) expr).getChild();
            } else if (expr instanceof ComponentTracer) {
                info = ((ComponentTracer)expr).getChild();
            } else {
                throw new IllegalStateException();
            }
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
        @Override
        public void append(Item item) throws XPathException {
            if (listener != null) {
                listener.enter(info, Collections.emptyMap(), getContext());
            }
            getNextOutputter().append(item);
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
        @Override
        public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
            if (listener != null) {
                listener.enter(info, Collections.emptyMap(), getContext());
            }
            return getResultFeed().startSelectedParentNode(node, locationId);
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
        @Override
        public void endSelectedParentNode(Location locationId) throws XPathException {
            if (listener != null) {
                listener.leave(info);
            }
            getResultFeed().endSelectedParentNode(locationId);
        }
        
    }


}

// Copyright (c) 2018-2023 Saxonica Limited