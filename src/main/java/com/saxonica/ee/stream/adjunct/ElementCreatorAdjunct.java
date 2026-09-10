package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.feed.ElementCreatorFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ElementCreator;
import net.sf.saxon.trans.XPathException;


/**
 * Generate streaming code for an xsl:element instruction. Used for xsl:element and for literal result
 * elements, but not for xsl:copy
 */
public class ElementCreatorAdjunct extends ParentConstructorAdjunct implements FeedMaker {

    @Override
    public FeedMaker getFeedMaker(final int arg) throws XPathException {
        ElementCreator exp = (ElementCreator)getExpression();
        if (Streamability.getConsumingOperand(exp).getChildExpression() == exp.getContentExpression()) {
            return this;
        } else {
            return getDefaultFeedMaker(arg);
        }
    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new ElementCreatorFeed(getExpression(), out, context);
    }


}

// Copyright (c) 2010-2023 Saxonica Limited