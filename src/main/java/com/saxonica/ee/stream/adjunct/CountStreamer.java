package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Int64Value;


/**
 * Streaming code for the count() function.
 */
public class CountStreamer extends InspectionAdjunct implements FeedMaker {

    public Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
        return new ItemFeed(getExpression(), out, context) {

            long count = 0;

            public void open(Terminator terminator) throws XPathException {
                super.open(terminator);
            }

            public void processItem(Item<?> item) throws XPathException {
                count++;
            }

            public void close() throws XPathException {
                getResult().processItem(Int64Value.makeIntegerValue(count));
                count = 0;
                super.close();
            }
        };
    }

}

// Copyright (c) 2013 Saxonica Limited.