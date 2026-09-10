package com.saxonica.ee.stream.feed;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.ContextMappingIterator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ForEach;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ListIterator;

import java.util.ArrayList;
import java.util.List;

/**
 * This class is used when a streaming xsl:for-each requests processing of a grounded consuming
 * sequence with a static dependency on last()
 */


public class BufferingForEachExpressionFeed extends ItemFeed {

    Expression action;
    XPathContext localContext;
    private final List<Item> inputValue = new ArrayList<>(4);

    public BufferingForEachExpressionFeed(ForEach expr, ItemFeed result, XPathContext context) {
        super(expr, result, context);
        action = expr.getActionExpression();
        localContext = context;
    }

    /**
     * Supply an item to the feed
     *
     * @param item the item being fed
     * @throws XPathException
     *          to report any failure
     */

    @Override
    public void append(Item item) throws XPathException {
        inputValue.add(item);
    }

    @Override
    public void close() throws XPathException {
        XPathContext c2 = localContext.newMinorContext();
        c2.trackFocus(new ListIterator.Of<>(inputValue));
        SequenceIterator mappingIter = new ContextMappingIterator(c1 -> action.iterate(c1), c2);

        Outputter out = getNextOutputter();
        processItems(mappingIter, out);
        out.close();
    }
}

// Copyright (c) 2018-2022 Saxonica Limited