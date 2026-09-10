package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.ExistenceFeed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Int64Value;


/**
 * Streaming code for the count() function.
 */
public class CountStreamer extends InspectionAdjunct implements FeedMaker {

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new ExistenceFeed(getExpression(), out, context) {

            long count = 0;

            @Override
            public void action() {
                count++;
            }

            @Override
            public void close() throws XPathException {
                getNextOutputter().append(Int64Value.makeIntegerValue(count));
                count = 0;
                getNextOutputter().close();
            }
        };
    }

}

// Copyright (c) 2013-2022 Saxonica Limited