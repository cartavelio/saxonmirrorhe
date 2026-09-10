package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.expr.sort.GenericAtomicComparer;
import net.sf.saxon.functions.IndexOf;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;


/**
 * Generate streaming code for the index-of() function.
 */
public class IndexOfStreamer extends StreamingAdjunct implements FeedMaker {

    public FeedMaker getFeedMaker(int arg) throws XPathException {
        return arg == 0 ? this : getDefaultFeedMaker(arg);
    }

    public Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
        return new IndexOfFeed(getExpression(), out, context);
    }

    private static class IndexOfFeed extends ItemFeed {

        AtomicValue search;
        AtomicComparer comparer;
        int count;

        public IndexOfFeed(Expression expr, Feed result, XPathContext context) {
            super(expr, result, context);
        }

        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            SystemFunctionCall call = (SystemFunctionCall)getExpression();
            SystemFunction fn = call.getTargetFunction();
            if (fn instanceof IndexOf) {
                comparer = ((IndexOf)fn).getAtomicComparer(getContext());
            } else {
                Expression collationArg = call.getArg(call.getArity() - 1);
                String collationName = collationArg.evaluateAsString(getContext()).toString();
                StringCollator collator = getContext().getConfiguration().getCollation(collationName);
                if (collator == null) {
                    dynamicError(new XPathException("Unknown collation " + collationName, "FOCH0002"));
                }
                comparer = new GenericAtomicComparer(collator, getContext());
            }
            Expression searchArg = call.getArg(1);
            search = (AtomicValue)searchArg.evaluateItem(getContext());
            count = 0;
        }

        /**
         * Supply an item to the feed
         *
         * @param item the item being fed
         * @throws net.sf.saxon.trans.XPathException
         *          to report any failure
         */

        public void processItem(Item<?> item) throws XPathException {
            if (!hasFailed()) {
                count++;
                if (comparer.comparesEqual((AtomicValue)item, search)) {
                    getResult().processItem(new Int64Value(count));
                }
            }
        }

    }

}

// Copyright (c) 2010-2015 Saxonica Limited.