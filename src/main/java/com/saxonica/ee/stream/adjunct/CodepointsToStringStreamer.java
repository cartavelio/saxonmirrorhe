package com.saxonica.ee.stream.adjunct;


import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.FastStringBuffer;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.StringValue;

import java.util.function.IntPredicate;

public class CodepointsToStringStreamer extends StreamingAdjunct implements FeedMaker {

    public Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
        return new ItemFeed(getExpression(), out, context) {
            FastStringBuffer buffer = new FastStringBuffer(FastStringBuffer.C64);
            IntPredicate checker = getConfiguration().getValidCharacterChecker();

            @Override
            public void processItem(Item<?> item) throws XPathException {
                if (!hasFailed()) {
                    long next = ((IntegerValue)item).longValue();
                    if (next < 0 || next > Integer.MAX_VALUE || !checker.test((int) next)) {
                        dynamicError(
                                new XPathException("Invalid XML character [x " + Integer.toHexString((int) next) + ']', "FOCH0001"));
                        return;
                    }
                    buffer.appendWideChar((int)((IntegerValue)item).longValue());
                }
            }

            @Override
            public void close() throws XPathException {
                if (!hasFailed()) {
                    getResult().processItem(new StringValue(buffer.toString()));
                    super.close();
                }
                buffer.setLength(0);
            }
        };
    }
}

// Copyright (c) 2018 Saxonica Limited.