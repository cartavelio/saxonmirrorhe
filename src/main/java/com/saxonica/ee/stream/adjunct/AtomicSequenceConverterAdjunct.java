package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.ItemMappingFeed;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.AtomicSequenceConverter;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;


/**
 * Compile a AtomicSequenceConverter or UntypedSequenceConverter expression to streaming code.
 */

public class AtomicSequenceConverterAdjunct extends StreamingAdjunct implements FeedMaker {

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        AtomicSequenceConverter.AtomicSequenceMappingFunction mapper =
                new AtomicSequenceConverter.AtomicSequenceMappingFunction();
        mapper.setConverter(((AtomicSequenceConverter) getExpression()).getConverter());
        return new ItemMappingFeed(getExpression(), out, context, mapper);
    }
}

// Copyright (c) 2010-2022 Saxonica Limited