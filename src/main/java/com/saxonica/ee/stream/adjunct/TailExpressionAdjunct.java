package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import net.sf.saxon.expr.TailExpression;
import net.sf.saxon.expr.XPathContext;


public class TailExpressionAdjunct extends FilteringAdjunct {

    /**
     * Make a filter for the event stream
     *
     * @param context the XPath dynamic context
     * @param out the feed to which matching items are to be sent
     * @return a filter that can be added to the event stream
     */

    @Override
    public FilteringFeed.Filter makeFilter(XPathContext context, ItemFeed out)  {
        final int start = ((TailExpression)getExpression()).getStart();
        return new FilteringFeed.FilterLambda((item, position) -> position >= start
                ? FilteringFeed.FilterResult.MATCHES
                : FilteringFeed.FilterResult.SKIP);
    }

}

// Copyright (c) 2018-2023 Saxonica Limited