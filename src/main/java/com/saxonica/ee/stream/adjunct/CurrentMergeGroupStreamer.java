package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.pattern.AnchorPattern;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;

import java.util.ArrayList;
import java.util.List;

/**
 * Generate streaming code for a call on current-merge-group().
 */
public class CurrentMergeGroupStreamer extends TransmissionAdjunct {

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        return PostureAndSweep.GROUNDED_AND_MOTIONLESS;
    }


    /**
     * Convert this expression to a streaming pattern (a pattern used internally to match nodes during
     * push processing of an event stream)
     *
     *
     * @param config           the Saxon configuration
     * @return the equivalent pattern if conversion succeeds; otherwise null
     */
    @Override
    public Pattern toStreamingPattern(Configuration config) {
        return AnchorPattern.getInstance();
    }

    @Override
    public Trigger makeTransmissionFlow(WatchManager watchManager, Expression exp, ItemFeed out, XPathContext context) throws XPathException {
        final Pattern selection = AnchorPattern.getInstance();
        return new Trigger(selection, makeItemFeed(watchManager, out, context), context);
    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        if (Streamability.getPosture(getExpression()) == Posture.GROUNDED) {
            return new GroupingFeed(out, context);
        } else {
            return out;
        }
    }

    private class GroupingFeed extends ItemFeed {

        private List<Item> group;

        public GroupingFeed(ItemFeed out, XPathContext context) {
            super(out, context);
        }

        /**
         * Start evaluating the expression.
         *
         * @param terminator
         * @throws net.sf.saxon.trans.XPathException
         *          if a dynamic error occurs
         */
        @Override
        public void open(Terminator terminator) throws XPathException {
            group = new ArrayList<Item>();
        }

        /**
         * Supply one item towards the streamed input of the expression
         *
         * @param item the item to be supplied
         */
        @Override
        public void append(Item item) throws XPathException {
            group.add(item);
        }

        /**
         * Finish evaluating the expression.
         *
         * @throws net.sf.saxon.trans.XPathException
         *          if a dynamic error occurs
         */
        @Override
        public void close() throws XPathException {
            getResultFeed().open(getTerminator());
            getNextOutputter().append(group.get(0));
            getNextOutputter().close();
        }
    }


}

// Copyright (c) 2012-2023 Saxonica Limited