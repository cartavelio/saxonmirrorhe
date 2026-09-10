package com.saxonica.ee.stream.adjunct;


import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;

/**
 * A subclass of StreamingAdjunct used for expressions that have a sequence-valued operand with usage
 * transmission. Such expressions may appear as a filter for striding or crawling nodes, in which case
 * they need to filter (or in some cases augment) the event stream rather than operating at the level of
 * composed items.
 */

public abstract class FilteringAdjunct extends TransmissionAdjunct implements FeedMaker {

    /**
     * Make a filter for the event stream
     *
     * @param context the XPath dyamic context
     * @param out
     * @return a filter that can be added to the event stream
     * @throws net.sf.saxon.trans.XPathException if a dynamic evaluation error occurs
     */

    public abstract FilteringFeed.Filter makeFilter(XPathContext context, Feed out) throws XPathException;

    public Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
        final FilteringFeed.Filter filter = makeFilter(context, out);
        return new FilteringFeed(out, context, filter);
    }



}

// Copyright (c) 2018 Saxonica Limited.