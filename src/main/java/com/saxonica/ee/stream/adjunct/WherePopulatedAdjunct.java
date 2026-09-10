package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.DelegatingFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.WherePopulatedOutputter;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;


/**
 * Generate streaming code for an xsl:where-populated instruction.
 */
public class WherePopulatedAdjunct extends TransmissionAdjunct {

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new WherePopulatedFeed(out, context);
    }

    private static class WherePopulatedFeed extends DelegatingFeed {

        // Delegate the work to the push processor used in the non-streamed case

        WherePopulatedFeed(ItemFeed out, XPathContext context) {
            super(new WherePopulatedOutputter(out), context);
        }
    }



}

// Copyright (c) 2014-2022 Saxonica Limited