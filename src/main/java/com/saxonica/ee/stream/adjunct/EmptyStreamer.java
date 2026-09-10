package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;


/**
 * Streaming code for an empty() function call
 */

public class EmptyStreamer extends InspectionAdjunct implements FeedMaker {

    public Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
        return new ItemFeed(getExpression(), out, context) {

            boolean found = false;

            @Override
            public void open(Terminator terminator) throws XPathException {
                super.open(terminator);
                found = false;
            }

            @Override
            public void processItem(Item<?> item) throws XPathException {
                if (!found) {
                    getResult().processItem(BooleanValue.FALSE);
                    found = true;
                    getResult().close();
                    getTerminator().terminate();
                }
            }

            @Override
            public void close() throws XPathException {
                if (!found) {
                    getResult().processItem(BooleanValue.TRUE);
                    getResult().close();
                }
                found = false;
            }
        };
    }
}

// Copyright (c) 2010-2013 Saxonica Limited.