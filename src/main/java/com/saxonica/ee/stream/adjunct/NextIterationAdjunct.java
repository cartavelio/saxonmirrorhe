package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.*;
import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;

/**
 * Compile a xsl:next-intruction instruction to streaming code.
 */

public class NextIterationAdjunct extends StreamingAdjunct {

    public FeedMaker getFeedMaker(final int arg) {
        return new FeedMaker() {
            public Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
                return new NextIterationFeed(getExpression(), out, context, arg);
            }
        };
    }

}

// Copyright (c) 2010 Saxonica Limited.