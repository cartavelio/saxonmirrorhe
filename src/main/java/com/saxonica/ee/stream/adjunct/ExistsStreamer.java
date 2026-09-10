package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.ExistenceFeed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;


/**
 * Streaming code for an fn:exists() function call
 */
public class ExistsStreamer extends InspectionAdjunct implements FeedMaker {

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new ExistenceFeed(getExpression(), out, context) {

            boolean found = false;

            @Override
            public void action() throws XPathException {
                if (!found) {
                    getNextOutputter().append(BooleanValue.TRUE);
                    getNextOutputter().close();
                    found = true;
                    getTerminator().terminate();
                }
            }

            @Override
            public void close() throws XPathException {
                if (!found) {
                    getNextOutputter().append(BooleanValue.FALSE);
                    getNextOutputter().close();
                }
            }
        };
    }

}

// Copyright (c) 2010-2022 Saxonica Limited