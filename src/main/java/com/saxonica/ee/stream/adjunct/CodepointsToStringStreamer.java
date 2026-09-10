package com.saxonica.ee.stream.adjunct;


import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.z.IntPredicateProxy;

public class CodepointsToStringStreamer extends StreamingAdjunct implements FeedMaker {

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new ItemFeed(getExpression(), out, context) {
            final StringBuilder buffer = new StringBuilder(64);
            final IntPredicateProxy checker = getConfiguration().getValidCharacterChecker();

            @Override
            public void append(Item item) throws XPathException {
                if (!hasFailed()) {
                    long next = ((IntegerValue)item).longValue();
                    if (next < 0 || next > Integer.MAX_VALUE || !checker.test((int) next)) {
                        dynamicError(
                                new XPathException("Invalid XML character [x " + Integer.toHexString((int) next) + ']', "FOCH0001"));
                        return;
                    }
                    buffer.appendCodePoint((int)((IntegerValue)item).longValue());
                }
            }

            @Override
            public void close() throws XPathException {
                if (!hasFailed()) {
                    getNextOutputter().append(new StringValue(buffer.toString()));
                    super.close();
                }
                buffer.setLength(0);
            }
        };
    }
}

// Copyright (c) 2018-2022 Saxonica Limited