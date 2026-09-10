package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;


/**
 * Abstract superclass for constructs that have operand usage "inspection" on their primary (sequence-valued) operand
 */
public abstract class InspectionAdjunct extends StreamingAdjunct {

    /*@Nullable*/
    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        Operand consumer = null;
        for (Operand o : getExpression().operands()) {
            if (Streamability.getSweep(o.getChildExpression()) == Sweep.CONSUMING) {
                consumer = o;
                break;
            }
        }
        if (consumer == null) {
            throw new XPathException("No consuming operand found");
        }
        final Expression select = consumer.getChildExpression();
        if (Streamability.isIncrementalPosture(Streamability.getPosture(select))) {
            return getInspectionWatchMaker(getExpression(), select, forGrouping);
        } else {
            return null;
        }
    }
    
    /**
     * This is a utility method for subclasses;
     * it provides a WatchMaker suitable for all constructs that perform inspection on a sequence,
     * for example exists(), count(), empty(), boolean(), not(), instanceOf.
     *
     * @param parent the expression, typically one of the above function calls
     * @param select the argument to the inspection construct, typically the first argument of one of the above
     * function calls
     * @param forGrouping
     * @return a WatchMaker that feeds the selected nodes (as FleetingNode instances) to the Feed provided
     * by this construct. Because this is an inspection construct, it is assumed able to compute the result
     * by looking at the FleetingNode objects alone.
     * @throws XPathException in the event of a dynamic error.
     */

    protected final WatchMaker getInspectionWatchMaker(Expression parent, Expression select, boolean forGrouping) throws XPathException {
        final Pattern selection = Streamability.toStreamingPattern(select, getConfiguration());
        final FeedMaker feedMaker = getFeedMaker(0);
        if (selection != null) {
            return (watchManager, out, context) -> {
                ItemFeed feed = feedMaker.makeItemFeed(watchManager, out, context);
                return new Trigger(selection, feed, context);
            };
        } else {
            StreamingAdjunct adjunct = StreamingAdjunct.makeStreamingAdjunct(getConfiguration(), select);
            final WatchMaker selectWM = adjunct.getWatchMaker(forGrouping);
            if (selectWM == null) {
                throw new XPathException("No watch found for " + select.toShortString());
            }
            return (watchManager, out, context) -> {
                ItemFeed feed = feedMaker.makeItemFeed(watchManager, out, context);
                return selectWM.makeWatch(watchManager, feed, context);
            };
        }
    }

//    public static class InspectionTrigger extends Trigger {
//
//        private Stack<Integer> nodeKindStack = new Stack<>();
//
//        InspectionTrigger(Pattern selection, ItemFeed action, XPathContext selectionContext) {
//            super(selection, action, selectionContext);
//        }
//
//        /**
//         * Signal that an element or document node has been found that matches the selection that this
//         * Watch is looking for. This method is called by the WatchManager while
//         * processing the startElement or startDocument event that matches the selection.
//         *
//         * @param node       the element or document node whose start event has been matched
//         * @param locationId the location associated with the element or document node (may be
//         *                   the location of the instruction that created it)
//         * @return a Receiver to be notified of all events starting with the startElement/startDocument event
//         * for the matched element, and ending with the endElement event for that element; or null if this
//         * feature is not required.
//         * @throws XPathException May be raised if a dynamic error occurs
//         */
//        @Override
//        public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
//            nodeKindStack.push(node.getNodeKind());
//            node.notify(getAction());
//            return null;
//        }
//
//        /**
//         * Signal that the endElement event has occurred for the element whose startElement
//         * event caused the Watch to be activated.
//         *
//         * @param locationId the location of the element
//         * @throws XPathException May be raised if a constraint implemented by this
//         *                        Watch is violated
//         */
//        @Override
//        public void endSelectedParentNode(Location locationId) throws XPathException {
//            int kind = nodeKindStack.pop();
//            if (kind == Type.DOCUMENT) {
//                getAction().endDocument();
//            } else {
//                getAction().endElement();
//            }
//        }
//    }

}

// Copyright (c) 2013-2022 Saxonica Limited