package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.FilteringFeed;
import net.sf.saxon.expr.SubscriptExpression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.NumericValue;

/**
 * Streaming code for a subscript expression
 */
public class SubscriptExpressionAdjunct extends FilteringAdjunct implements FeedMaker {


    public FeedMaker getFeedMaker(int arg) throws XPathException {
        return arg == 0 ? this : getDefaultFeedMaker(arg);
    }

    @Override
    public FilteringFeed.Filter makeFilter(XPathContext context, Feed out) throws XPathException {
        SubscriptExpression fnc = (SubscriptExpression) getExpression();
        NumericValue startD = (NumericValue) fnc.getSubscriptExpression().evaluateItem(context);
        int subscript = startD.isWholeNumber() ? (int) startD.round(0).longValue() : -1;
        return new SubsequenceStreamer.SubsequenceFilter(subscript, subscript);
    }

}

// Copyright (c) 2018 Saxonica Limited.