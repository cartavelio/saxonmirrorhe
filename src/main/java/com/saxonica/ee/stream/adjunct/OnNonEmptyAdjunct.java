package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;


/**
 * Generate streaming code for a consuming xsl:on-non-empty instruction
 */
public class OnNonEmptyAdjunct extends TransmissionAdjunct {

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new FeedToEventBuffer(out, out, context) {
            {
                setBuffer(((ConditionalBlockAdjunct.ConditionalBlockFeed)getNextOutputter()).getTentativeContent());
            }

            /**
             * Start evaluating the expression. The default implementation does nothing.
             *
             * @param terminator used to achieve early exit
             * @throws XPathException if a dynamic error occurs
             */
            @Override
            public void open(Terminator terminator) throws XPathException {
                super.open(terminator);
            }

            @Override
            public void close() throws XPathException {
                ((ConditionalBlockAdjunct.ConditionalBlockFeed)getNextOutputter()).flushIfNonEmpty();
                getNextOutputter().close();
            }
        };
    }



}

// Copyright (c) 2018-2020 Saxonica Limited