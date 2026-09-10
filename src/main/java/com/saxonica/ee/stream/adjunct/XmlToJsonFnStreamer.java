package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.StreamingPatternMaker;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.CardinalityChecker;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.ma.json.JsonReceiver;
import net.sf.saxon.ma.json.XMLToJsonFn;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;

import java.util.Map;

/**
 * Streaming code for the xml-to-json() function
 */
public class XmlToJsonFnStreamer extends StreamingAdjunct implements FeedMaker {

    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        final SystemFunctionCall instruction = (SystemFunctionCall) getExpression();
        Expression select = instruction.getArg(0);

        // Because the function requires a singleton node as the argument, the select expression will often be
        // a CardinalityChecker, which doesn't provide a streaming pattern. We bypass the cardinality checker, and
        // take responsibility for this checking ourselves.

        if (select instanceof CardinalityChecker) {
            select = ((CardinalityChecker)select).getBaseExpression();
        }

        final Pattern selection = StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);

        if (selection != null) {
            return (watchManager, out, context) -> {
                ToJsonAction action = new ToJsonAction(getExpression(), out, context);
                return new Trigger(selection, action, context);
            };
        }
        return null;
    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new ToJsonAction(getExpression(), out, context);
    }

    private static class ToJsonAction extends ItemFeed {

        private JsonReceiver receiver;
        private boolean indenting;
        private boolean foundOneNode;

        public ToJsonAction(Expression functionCall, ItemFeed out, XPathContext context) {
            super(functionCall, out, context);
        }

        @Override
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            SystemFunctionCall exp = (SystemFunctionCall) getExpression();
            XMLToJsonFn target = (XMLToJsonFn)exp.getTargetFunction(getContext());

            if (exp.getArity() > 1) {
                MapItem suppliedOptions = (MapItem) exp.getArg(1).evaluateItem(getContext());
                Map<String, Sequence> options = target.getDetails().optionDetails.processSuppliedOptions(suppliedOptions, getContext());
                indenting = ((BooleanValue) options.get("indent").head()).getBooleanValue();
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
         * for the matched element, and ending with the endElement event for that element; or null if this
         * feature is not required.
         * @throws XPathException May be raised if a dynamic error occurs
         */

        @Override
        public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
            if (foundOneNode) {
                XPathException err = new XPathException("Supplied argument to xml-to-json contains more than one node", "XPTY0004");
                dynamicError(err);
                return null;
            } else {
                foundOneNode = true;
                receiver = new JsonReceiver(getPipelineConfiguration(),
                                            getContext(),
                                            getNextOutputter().getStringReceiver(false, locationId));
                receiver.setIndenting(indenting);
                receiver.open();
                return receiver;
            }
        }

        /**
         * Signal that a item (other than a streamed element) that matches the selection
         * has been found.
         *
         * @param item the item that was matched.
         */

        @Override
        public void append(Item item) throws XPathException {
            throw new AssertionError();
        }

        /**
         * Signal that the endElement event has occurred for the element whose startElement
         * event caused the Watch to be activated.
         *
         * @param locationId the location of the node in the source, or of the instruction that created it
         * @throws net.sf.saxon.type.ValidationException May be raised if the constraint implemented by this
         *                                               Watch is violated @param locationId the location of the node in the source, or of the instruction that created it
         */

        @Override
        public void endSelectedParentNode(Location locationId) throws XPathException {
            //
        }

        @Override
        public void close() throws XPathException {
            receiver.close();
//            String json = receiver.getJsonString();
//            getResult().append(StringValue.makeStringValue(json));
            super.close();
        }


    }


}

// Copyright (c) 2018-2022 Saxonica Limited