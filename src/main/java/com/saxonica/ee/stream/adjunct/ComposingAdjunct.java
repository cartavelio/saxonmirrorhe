package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.StreamingPatternMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;


/**
 * Watch factory class for an class that composes items from sequences of events:
 * examples are copy-of(), data(), and string().
 */
public abstract class ComposingAdjunct extends StreamingAdjunct {

    /**
     * Method that must be implemented in subclasses to supply the feed to which the
     * composed items are sent
     *
     * @param watchManager the watch manager
     * @param out the destination of composed items
     * @param context XPath evaluation context
     * @return a the relevant ItemFeed
     */

    public abstract ItemFeed makeComposingAction(WatchManager watchManager, ItemFeed out, XPathContext context);

    /*@Nullable*/
    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        final Operand consumingChild = Streamability.getConsumingOperand(getExpression());
        final Expression select = consumingChild == null ? null :
                consumingChild.getChildExpression();
        if (select == null /*|| forGrouping*/) {
            return null;
        }
        final Pattern selection = StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);
        if (selection != null) {
            return (watchManager, out, context) -> {
                PipelineConfiguration pipe = watchManager.getPipelineConfiguration();
                ItemFeed action = makeComposingAction(watchManager, out, context);
                Trigger watch = new Trigger(selection, action, context);
                watch.setPipelineConfiguration(pipe);
                return watch;
            };
        }
        if (Streamability.isIncrementalPosture(Streamability.getPosture(select))) {
            final StreamingAdjunct adjunct = StreamingAdjunct.makeStreamingAdjunct(getConfiguration(), select);
            if (adjunct instanceof TransmissionAdjunct) {
                return (watchManager, out, context) -> {
                    ItemFeed action = makeComposingAction(watchManager, out, context);
                    return ((TransmissionAdjunct)adjunct).makeTransmissionFlow(watchManager, select, action, context);
                };
            }
        }
        return null;

    }

}

// Copyright (c) 2018-2022 Saxonica Limited