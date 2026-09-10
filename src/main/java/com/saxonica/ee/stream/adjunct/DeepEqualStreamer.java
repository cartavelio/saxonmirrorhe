package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.feed.CopyOfFeed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.functions.CollatingFunctionFree;
import net.sf.saxon.functions.DeepEqual;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.value.BooleanValue;


/**
 * Implement streaming of deep-equal function. This is complicated by the fact that
 * we need separate implementations depending on which argument is being streamed.
 */
public class DeepEqualStreamer extends StreamingAdjunct implements FeedMaker {

    private int consumingArg = -1;

    /**
     * Get a WatchMaker, a factory class that creates a Watch to evaluate this expression
     * in streaming mode. This method will only be called if the getImplementationMethod()
     * method of the expression includes the value {@link net.sf.saxon.expr.Expression#WATCH_METHOD}
     *
     * @return a Watchmaker to evaluate the expression, or null if it is not streamable with respect
     *         to the specificed argument.
     * @throws net.sf.saxon.trans.XPathException
     *          in the event of a failure
     * @param forGrouping true if the selected items are being grouped
     */
    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        consumingArg = findConsumingArg(getExpression(), consumingArg);
        return super.getWatchMaker(forGrouping);
    }

    @Override
    public ItemFeed makeItemFeed(final WatchManager watchManager, ItemFeed out, final XPathContext context) throws XPathException {
        consumingArg = findConsumingArg(getExpression(), consumingArg);
        if (consumingArg >= 2) {
            return getDefaultFeedMaker(consumingArg).makeItemFeed(watchManager, out, context);
        }
        SystemFunctionCall call = (SystemFunctionCall)getExpression();
        Posture posture = Streamability.getPosture(call.getArg(consumingArg));
        if (posture == Posture.GROUNDED || posture == Posture.CLIMBING) {
            return new GroundedDeepEqualFeed(watchManager, call, out, context, consumingArg);
        } else {
            // TODO: tough case. At present, we don't do a fully streamed implementation: we effectively do
            // deep-equal(copy-of(X/Y/Z), $val) instead of deep-equal(X/Y/Z, val).
            ItemFeed comparisonFeed = new GroundedDeepEqualFeed(watchManager, call, out, context, consumingArg);
            return new CopyOfFeed(watchManager, comparisonFeed, context);
        }
    }

    /**
     * Streamed deep-equal feed where one of the two arguments is consuming and grounded.
     */

    private static class GroundedDeepEqualFeed extends ItemFeed {

        WatchManager watchManager;
        int consumingArg;
        SequenceIterator other;
        AtomicComparer comparer;
        boolean done;
        boolean closed;

        public GroundedDeepEqualFeed(WatchManager watchManager, SystemFunctionCall exp,
                                     ItemFeed result, XPathContext context, int consumingArg) throws XPathException {
            super(exp, result, context);
            this.watchManager = watchManager;
            other = exp.getArg(1 - consumingArg).iterate(context);
            this.consumingArg = consumingArg;
            SystemFunctionCall call = (SystemFunctionCall) getExpression();
            DeepEqual function;
            if (call.isCallOn(CollatingFunctionFree.class)) {
                CollatingFunctionFree fn = (CollatingFunctionFree)call.getTargetFunction();
                String collation = call.getArg(2).evaluateAsString(context).toString();
                try {
                    function = (DeepEqual)fn.bindCollation(collation);
                } catch (XPathException e) {
                    dynamicError(e);
                    done = true;
                    return;
                }
            } else {
                function = (DeepEqual) call.getTargetFunction();
            }
            comparer = function.getAtomicComparer(getContext());
        }

        /**
         * Signal that a node (other than a document or element node) that matches the selection
         * has been found, or that a grounded item is being passed to the next expression in the
         * evaluation pipeline.
         *
         * @param item the node that was matched.
         * @throws net.sf.saxon.trans.XPathException
         *          May be raised if a dynamic error occurs
         */
        @Override
        public void append(Item item) throws XPathException {
            if (!done) {
                Item otherItem = other.next();
                if (otherItem == null ||
                        !DeepEqual.deepEqual(
                            SingletonIterator.makeIterator(item),
                            SingletonIterator.makeIterator(otherItem),
                            comparer, getContext(), 0)) {
                    getNextOutputter().append(BooleanValue.FALSE);
                    done = true;
                    if (watchManager.allowsEarlyExit()) {
                        closed = true;
                        getNextOutputter().close();
                        getTerminator().terminate();
                    }
                }
            }
        }

        /**
         * The close() method is called immediately before the Watch is destroyed, that is,
         * when the element whose declaration scopes the constraint implemented by this
         * Watch goes out of scope. The default implementation does nothing.
         */

        @Override
        public void close() throws XPathException {
            if (!done) {
                boolean more = other.next() != null;
                getNextOutputter().append(BooleanValue.get(!more));
            }
            if (!closed) {
                getNextOutputter().close();
            }
        }


    }

}

// Copyright (c) 2010-2023 Saxonica Limited - 2015.