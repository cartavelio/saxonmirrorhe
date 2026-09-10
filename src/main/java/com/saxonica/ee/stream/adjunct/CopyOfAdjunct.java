package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.CopyOfFeed;
import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.CopyOf;
import net.sf.saxon.trans.XPathException;


public class CopyOfAdjunct extends ComposingAdjunct implements FeedMaker {

    @Override
    public Feed makeComposingAction(WatchManager watchManager, Feed out, XPathContext context) {
        CopyOfFeed feed = new CopyOfFeed(watchManager, out, context);
        CopyOf exp = (CopyOf) getExpression();
        feed.setCopyNamespaces(exp.isCopyNamespaces());
        feed.setCopyAccumulators(exp.isCopyAccumulators());
        feed.setValidationOptions(exp.getValidationAction(), exp.getSchemaType());
        return feed;

    }

    public Feed makeItemFeed(WatchManager watchManager, final Feed out, XPathContext context) throws XPathException {
        return makeComposingAction(watchManager, out, context);
    }
}

// Copyright (c) 2010-2013 Saxonica Limited.