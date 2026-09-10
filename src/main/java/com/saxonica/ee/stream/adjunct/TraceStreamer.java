package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.Controller;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.functions.Trace;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Type;

import java.util.Stack;


/**
 * Implement streaming of fn:trace instruction.
 */
public class TraceStreamer extends TransmissionAdjunct implements FeedMaker {

    public FeedMaker getFeedMaker(int arg) throws XPathException {
        return arg == 0 ? this : super.getFeedMaker(arg);
    }

    public Feed makeItemFeed(final WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
        return new TraceFeed((SystemFunctionCall)getExpression(), out, context);
    }

    public static int sequenceNr = 0;

    /**
     * Streamed trace feed where the streamed argument is the first argument
     */

    private static class TraceFeed extends ItemFeed {

        String label;
        String seq = "trace " + sequenceNr++ + ": ";
        Stack<FleetingParentNode> stack = new Stack<FleetingParentNode>();

        public TraceFeed(SystemFunctionCall exp, Feed result, XPathContext context) {
            super(exp, result, context);
        }

        /**
         * Open the watch. This is done at the point where it first starts watching for nodes
         * that match the selection.
         *
         * @throws net.sf.saxon.trans.XPathException
         *          May be raised if a dynamic error occurs
         * @param terminator
         */
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
            Trace function = (Trace)fnc.getTargetFunction();
            this.label = fnc.getArg(1).evaluateItem(getContext()).getStringValue();
            emit("open", null);
        }

        /**
         * Signal that a node (other than a document or element node) that matches the selection
         * has been found, or that a grounded item is being passed to the next expression in the
         * evaluation pipeline.
         *
         * @param item the node that was matched.
         * @throws net.sf.saxon.trans.XPathException
         *          May be raised if a dynamic error occurs
         */
        public void processItem(Item<?> item) throws XPathException {
            emit("item", item);
            getResult().processItem(item);
        }

        /**
         * The close() method is called immediately before the Watch is destroyed, that is,
         * when the element whose declaration scopes the constraint implemented by this
         * Watch goes out of scope. The default implementation does nothing.
         */

        public void close() throws XPathException {
            emit("close", null);
            getResult().close();
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
            if (node.getNodeKind() == Type.DOCUMENT) {
                emit("startDocument", node);
            } else {
                emit("startElement", node);
            }
            stack.push(node);
            return getResult().startSelectedParentNode(node, locationId);
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
            FleetingParentNode node = stack.pop();
            if (node.getNodeKind() == Type.DOCUMENT) {
                emit("endDocument", node);
            } else {
                emit("endElement", node);
            }
            getResult().endSelectedParentNode(locationId);
        }

        private void emit(String phase, Item val) {
            String message = seq + label + ": " + phase;
            Controller controller = getContext().getController();
            if (controller.isTracing()) {
                SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
                Trace function = (Trace) fnc.getTargetFunction();
                function.notifyListener(message, val, getExpression().getLocation(), getContext());
            } else {
                Logger out = controller.getTraceFunctionDestination();
                if (out != null) {
                    Trace.traceItem(val, message, out);
                }
            }
        }
    }


}

// Copyright (c) 2010 Saxonica Limited - 2013.