package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.BooleanValue;


/**
 * Streaming code for a not() function call
 */
public class NotFnStreamer extends InspectionAdjunct implements FeedMaker {

    public Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
        return new NotFnFeed(getExpression(), out, context);
    }

    private static class NotFnFeed extends ItemFeed {
        int count;
        boolean foundAtomic;
        boolean truth;

        public NotFnFeed(Expression exp, Feed result, XPathContext context) {
            super(exp, result, context);
        }
        @Override
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            count = 0;
            truth = false;
        }

        @Override
        public void processItem(Item<?> item) throws XPathException {
            if (!hasFailed()) {
                try {
                    if (++count == 1) {
                        truth = ExpressionTool.effectiveBooleanValue(item);
                        foundAtomic = item instanceof AtomicValue;
                    } else {
                        if (foundAtomic) {
                            ExpressionTool.ebvError("a sequence of two or more items starting with an atomic value", getExpression());
                        }
                    }
                } catch (XPathException e) {
                    dynamicError(e);
                }
            }
        }

        @Override
        public void close() throws XPathException {
            if (!hasFailed()) {
                getResult().processItem(BooleanValue.get(!truth));
                super.close();
            }
        }
    }

}

// Copyright (c) 2013 Saxonica Limited.