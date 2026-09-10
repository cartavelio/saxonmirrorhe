package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FilteringFeed;
import net.sf.saxon.expr.TailExpression;
import net.sf.saxon.expr.XPathContext;


public class TailExpressionAdjunct extends FilteringAdjunct {

    /**
     * Make a filter for the event stream
     *
     * @param context the XPath dyamic context
     * @param out the feed to which matching items are to be sent
     * @return a filter that can be added to the event stream
     */

    public FilteringFeed.Filter makeFilter(XPathContext context, Feed out)  {
        final int start = ((TailExpression)getExpression()).getStart();
        return (item, position) -> position >= start ? FilteringFeed.Filter.MATCHES : FilteringFeed.Filter.SKIP;
    }

}

// Copyright (c) 2018 Saxonica Limited.