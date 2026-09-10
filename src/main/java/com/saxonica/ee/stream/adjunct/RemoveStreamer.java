package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.IntegerValue;

import static com.saxonica.ee.stream.feed.FilteringFeed.FilterResult.MATCHES;
import static com.saxonica.ee.stream.feed.FilteringFeed.FilterResult.SKIP;


/**
 * Generate streaming code for a remove() function call.
 */
public class RemoveStreamer extends FilteringAdjunct implements FeedMaker {

    @Override
    public FilteringFeed.Filter makeFilter(XPathContext context, ItemFeed out) throws XPathException {
        SystemFunctionCall fnc = (SystemFunctionCall)getExpression();
        IntegerValue startD = (IntegerValue) fnc.getArg(1).evaluateItem(context);
        return new RemoveFilter((int)startD.longValue());
    }

    @Override
    public FeedMaker getFeedMaker(int arg) throws XPathException {
        return arg == 0 ? this : getDefaultFeedMaker(arg);
    }

    private static class RemoveFilter implements FilteringFeed.Filter {

        private final int removeIndex;

        public RemoveFilter(int removeIndex) {
            this.removeIndex = removeIndex;
        }

        @Override
        public FilteringFeed.FilterResult matches(Item item, int position) {
            return position != removeIndex ? MATCHES : SKIP;
        }
    }

}

// Copyright (c) 2013-2022 Saxonica Limited