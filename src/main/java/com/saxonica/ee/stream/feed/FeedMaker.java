package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;

/**
 * An expression that can be evaluated in streaming mode is represented at compile time
 * by a list of FeedMaker objects. At run-time, the makeFeed() method on each of these
 * objects is called to create a list of Feed objects, representing the evaluation
 * pipeline. The output of the Watch that monitors the actual parsing events is then
 * fed into this pipeline, which eventually delivers its result in the current
 * SequenceReceiver of the XPathContext object.
 */
public interface FeedMaker {

    Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException;

}

//
// Copyright (c) 2009 Saxonica Limited.
//


