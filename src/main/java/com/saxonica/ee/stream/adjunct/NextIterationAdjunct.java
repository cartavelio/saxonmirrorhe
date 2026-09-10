package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.NextIterationFeed;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;

/**
 * Compile a xsl:next-intruction instruction to streaming code.
 */

public class NextIterationAdjunct extends StreamingAdjunct {

    @Override
    public FeedMaker getFeedMaker(final int arg) {
        return new FeedMaker() {
            @Override
            public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
                return new NextIterationFeed(getExpression(), out, context, arg);
            }
        };
    }

}

// Copyright (c) 2010-2020 Saxonica Limited