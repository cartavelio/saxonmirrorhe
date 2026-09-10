package com.saxonica.ee.stream.feed;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.FilterExpression;
import net.sf.saxon.expr.FilterIterator;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.FocusIterator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.SequenceExtent;

import java.util.ArrayList;
import java.util.List;

/**
 * This class is used when a streaming template requests filtering of items that don't
 * come directly from the streamed document, but rather are constructed in the course of the push pipeline
 */


public class BufferingFilterExpressionFeed extends ItemFeed {

    Expression filter;
    XPathContext localContext;
    private final List<Item> inputValue = new ArrayList<>(4);

    public BufferingFilterExpressionFeed(Expression expr, ItemFeed result, XPathContext context) {
        super(expr, result, context);
        filter = ((FilterExpression) expr).getActionExpression();
        localContext = context;
    }

    /**
     * Supply an item to the feed
     *
     * @param item the item being fed
     * @throws net.sf.saxon.trans.XPathException
     *          to report any failure
     */

    @Override
    public void append(Item item) throws XPathException {
        inputValue.add(item);
    }

    @Override
    public void close() throws XPathException {
        Sequence extent = SequenceExtent.makeSequenceExtent(inputValue);
        XPathContext c2 = localContext.newMinorContext();
        FocusIterator iter = c2.trackFocus(extent.iterate());
        SequenceIterator filterIter = new FilterIterator(iter, filter, c2);
        Outputter out = getNextOutputter();
        processItems(filterIter, out);
        out.close();
    }
}

// Copyright (c) 2018-2022 Saxonica Limited