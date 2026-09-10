package com.saxonica.ee.stream.adjunct;


import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
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
     * @param context the XPath dynamic context
     * @param out the destination for the filtered events
     * @return a filter that can be added to the event stream
     * @throws net.sf.saxon.trans.XPathException if a dynamic evaluation error occurs
     */

    public abstract FilteringFeed.Filter makeFilter(XPathContext context, ItemFeed out) throws XPathException;

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        final FilteringFeed.Filter filter = makeFilter(context, out);
        return new FilteringFeed(watchManager, out, context, filter);
    }



}

// Copyright (c) 2018-2022 Saxonica Limited