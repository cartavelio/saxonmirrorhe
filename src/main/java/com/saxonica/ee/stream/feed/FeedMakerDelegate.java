package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpDelegate;

public class FeedMakerDelegate implements FeedMaker {

    private Lambda delegate;

    public FeedMakerDelegate(Lambda delegate) {
        this.delegate = delegate;
    }

    @FunctionalInterface
    @CSharpDelegate(true)
    public interface Lambda {
        ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException;
    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return delegate.makeItemFeed(watchManager, out, context);
    }
}

// Copyright (c) 2020-2023 Saxonica Limited

