package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.CardinalityCheckingFeed;
import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;


/**
 * Generate streaming code for a "treat" function call (for example one-or-more)
 */
public class TreatFnStreamer extends TransmissionAdjunct implements FeedMaker {

    public Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
        return new CardinalityCheckingFeed(getExpression(), out, context);
    }

}

// Copyright (c) 2013-2015 Saxonica Limited.