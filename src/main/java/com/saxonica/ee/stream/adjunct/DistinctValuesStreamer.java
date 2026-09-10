package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.functions.DistinctValues;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;

import java.util.HashSet;


/**
 * Implement the distinct-values function in streaming mode.
 */

public class DistinctValuesStreamer
        extends StreamingAdjunct implements FeedMaker {

    @Override
    public FeedMaker getFeedMaker(int arg) {
        return arg == 0 ? this : getDefaultFeedMaker(arg);
    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new DistinctValuesFeed(getExpression(), out, context);
    }

    private static class DistinctValuesFeed extends ItemFeed {

        private StringCollator collator;
        private HashSet<AtomicMatchKey> lookup;
        private final int implicitTimezone;

        DistinctValuesFeed(Expression expr, ItemFeed result, XPathContext context) {
            super(expr, result, context);
            implicitTimezone = context.getImplicitTimezone();
            SystemFunctionCall call = (SystemFunctionCall)expr;
            SystemFunction fn = call.getTargetFunction();
            if (fn instanceof DistinctValues) {
                collator = ((DistinctValues)fn).getStringCollator();
            }
        }

        @Override
        public void open(Terminator terminator) throws XPathException {
            lookup = new HashSet<>(40);
            if (collator == null) {
                SystemFunctionCall call = (SystemFunctionCall) getExpression();
                Expression collationArg = call.getArg(call.getArity()-1);
                String collationName = collationArg.evaluateAsString(getContext()).toString();
                collator = getContext().getConfiguration().getCollation(collationName);
                if (collator == null) {
                    dynamicError(new XPathException("Unknown collation " + collationName, "FOCH0002"));
                }
            }
            getResultFeed().open(terminator);
        }

        /**
         * Supply an item to the feed
         *
         * @param item the item being fed
         * @throws net.sf.saxon.trans.XPathException
         *          to report any failure
         */

        @Override
        public void append(Item item) throws XPathException {
            if (!hasFailed()) {
                AtomicValue next = (AtomicValue) item;
                AtomicMatchKey key = next.isNaN()
                        ? DistinctValues.NaN_MATCH_KEY
                        : next.getXPathMatchKey(collator, implicitTimezone);
                if (lookup.add(key)) {
                    // returns true if newly added
                    getNextOutputter().append(item);
                }
            }
        }

    }


}

// Copyright (c) 2009-2023 Saxonica Limited