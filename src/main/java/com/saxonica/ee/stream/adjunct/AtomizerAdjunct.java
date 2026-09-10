package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.TypedValueFeed;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;


/**
 * Streaming adjunct class for an atomizer (or call on the data() function)
 */
public class AtomizerAdjunct extends ComposingAdjunct implements FeedMaker {
    @Override
    public ItemFeed makeComposingAction(WatchManager watchManager, ItemFeed out, XPathContext context) {
        return new TypedValueFeed(out, context);
    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new TypedValueFeed(out, context);
    }

}

// Copyright (c) 2010-2022 Saxonica Limited