package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.ExistenceFeed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;


/**
 * Streaming code for an fn:empty() function call
 */

public class EmptyStreamer extends InspectionAdjunct implements FeedMaker {

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new EmptyExistenceFeed(getExpression(), out, context, watchManager);
    }

    private static class EmptyExistenceFeed extends ExistenceFeed {

        boolean found = false;
        boolean closed = false;
        WatchManager watchManager;

        protected EmptyExistenceFeed(Expression exp, ItemFeed result, XPathContext context, WatchManager watchManager) {
            super(exp, result, context);
            this.watchManager = watchManager;
        }


        @Override
        public void action() throws XPathException {
            if (!found) {
                getNextOutputter().append(BooleanValue.FALSE);
                found = true;
                if (watchManager.allowsEarlyExit()) {
                    closed = true;
                    getNextOutputter().close();
                    getTerminator().terminate();
                }
            }
        }

        @Override
        public void close() throws XPathException {
            if (!found) {
                getNextOutputter().append(BooleanValue.TRUE);
            }
            if (!closed) {
                getNextOutputter().close();
            }
        }
    }

}

// Copyright (c) 2010-2022 Saxonica Limited