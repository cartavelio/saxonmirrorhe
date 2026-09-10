package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.StringValueFeed;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;


/**
 * Streaming code to implement the XPath string() function
 */
public class StringFnStreamer extends ComposingAdjunct {

    @Override
    public ItemFeed makeComposingAction(WatchManager watchManager, ItemFeed out, XPathContext context) {
        return new StringValueFeed(out, context);
    }

    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new StringValueFeed(out, context);
    }


}

// Copyright (c) 2010-2022 Saxonica Limited