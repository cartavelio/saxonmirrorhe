package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.SimpleNodeConstructorFeed;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;

/**
 * Generate streaming code for an xsl:value-of, xsl:comment, xsl:processing-instruction instruction, etc
 */
public class SimpleNodeConstructorAdjunct extends StreamingAdjunct implements FeedMaker {

    public FeedMaker getFeedMaker(int arg) throws XPathException {
        return arg == 0 ? this : getDefaultFeedMaker(arg);
    }

    public Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
        return new SimpleNodeConstructorFeed(getExpression(), out, context);
    }
}

// Copyright (c) 2010-2013 Saxonica Limited.

