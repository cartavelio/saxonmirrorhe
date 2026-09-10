package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpModifiers;


/**
 * Generate streaming code for a consuming xsl:on-non-empty instruction
 */
public class OnNonEmptyAdjunct extends TransmissionAdjunct {

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        FeedToEventBuffer feedToEventBuffer = new FeedToEventBuffer(out, out, context) {
            /**
             * Start evaluating the expression. The default implementation does nothing.
             *
             * @param terminator used to achieve early exit
             * @throws XPathException if a dynamic error occurs
             */
            @Override
            @CSharpModifiers(code={"public", "override"})
            public void open(Terminator terminator) throws XPathException {
                super.open(terminator);
            }

            @Override
            public void close() throws XPathException {
                ((ConditionalBlockAdjunct.ConditionalBlockFeed)getNextOutputter()).flushIfNonEmpty();
                getNextOutputter().close();
            }
        };
        feedToEventBuffer.setBuffer(((ConditionalBlockAdjunct.ConditionalBlockFeed) out).getTentativeContent());
        return feedToEventBuffer;
    }



}

// Copyright (c) 2018-2023 Saxonica Limited