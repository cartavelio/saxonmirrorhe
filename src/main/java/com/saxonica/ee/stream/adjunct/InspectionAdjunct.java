package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
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
        if (Streamability.getPosture(select).isIncremental()) {
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
                Feed feed = feedMaker.makeItemFeed(watchManager, out, context);
                return new Trigger(selection, feed, context);
            };
        } else {
            StreamingAdjunct adjunct = StreamingAdjunct.makeStreamingAdjunct(getConfiguration(), select);
            final WatchMaker selectWM = adjunct.getWatchMaker(forGrouping);
            if (selectWM == null) {
                throw new XPathException("No watch found for " + select.toShortString());
            }
            return (watchManager, out, context) -> {
                Feed feed = feedMaker.makeItemFeed(watchManager, out, context);
                return selectWM.makeWatch(watchManager, feed, context);
            };
        }
    }

}

// Copyright (c) 2013 Saxonica Limited.