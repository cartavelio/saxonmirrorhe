package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.Fold;
import net.sf.saxon.functions.FoldingFunction;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.trans.XPathException;


/**
 * Streaming code for functions implemented as fold operations.
 */
public class FoldStreamer extends StreamingAdjunct implements FeedMaker {

    public FeedMaker getFeedMaker(int arg) throws XPathException {
        return arg==0 ? this : getDefaultFeedMaker(arg);
    }

    public Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
        SystemFunctionCall call = (SystemFunctionCall)getExpression();
        return new FoldFeed(call, out, context);
    }

    private static class FoldFeed extends ItemFeed {

        private Fold fold;

        public FoldFeed(SystemFunctionCall expression, Feed result, XPathContext context) throws XPathException {
            super(expression, result, context);
            Sequence<?>[] additionalArgs = SequenceTool.makeSequenceArray(expression.getArity() - 1);
            for (int i=1; i<expression.getArity(); i++) {
                additionalArgs[i-1] = SequenceTool.toLazySequence(expression.getArg(i).iterate(context));
            }
            FoldingFunction ff = (FoldingFunction) expression.getTargetFunction();
            fold = ff.getFold(context, additionalArgs);
        }

        /**
         * Supply an item to the feed
         *
         * @param item the item being fed
         * @throws net.sf.saxon.trans.XPathException
         *          to report any failure
         */

        public void processItem(Item<?> item) throws XPathException {
            if (!hasFailed()) {
                try {
                    fold.processItem(item);
                } catch (XPathException err) {
                    dynamicError(err);
                }
                if (fold.isFinished()) {
                    close();
                    getTerminator().terminate();
                }
            }
        }

        /**
         * Signal the end of the sequence of items
         *
         * @throws net.sf.saxon.trans.XPathException
         *          to report any failure
         */

        public void close() throws XPathException {
            if (!hasFailed()) {
                Sequence<?> value = fold.result();
                Feed out = getResult();
                processItems(value.iterate(), out);
                out.close();
            }
        }

    }

}

// Copyright (c) 2014 Saxonica Limited.