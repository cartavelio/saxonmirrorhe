package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.*;
import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;


/**
 * Watch factory class for a general comparison
 */
public class GeneralComparisonAdjunct extends StreamingAdjunct {

    public FeedMaker getFeedMaker(final int arg) {
        return new FeedMaker() {
            public Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
                return new GeneralComparisonFeed(getExpression(), arg, out, context);
            }
        };
    }
}

// Copyright (c) 2010 Saxonica Limited.