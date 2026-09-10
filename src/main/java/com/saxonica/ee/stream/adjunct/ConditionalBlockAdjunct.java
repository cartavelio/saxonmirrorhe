package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.SignificantItemDetector;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ConditionalBlock;
import net.sf.saxon.expr.instruct.OnEmptyExpr;
import net.sf.saxon.expr.instruct.OnNonEmptyExpr;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;

import java.util.ArrayList;
import java.util.List;


/**
 * Generate streaming code for a conditional Block, that is a sequence constructor containing
 * xsl:on-empty and/or xsl:on-not-empty instructions
 */
public class ConditionalBlockAdjunct extends TransmissionAdjunct implements FeedMaker {

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

    public static class ConditionalBlockFeed extends ItemFeed {

        int consumingArg;
        boolean empty = true;
        private boolean consumingArgIsOnEmpty = false;
        List<OnEmptyExpr> emptyPendingList = new ArrayList<OnEmptyExpr>();
        List<OnNonEmptyExpr> nonEmptyPendingList = new ArrayList<OnNonEmptyExpr>();
        List<Item> tentativeContent = null;

        public ConditionalBlockFeed(ConditionalBlock exp, int arg, Feed result, XPathContext context) {
            super(exp, result, context);
            this.consumingArg = arg;
            consumingArgIsOnEmpty = exp.getChildExpression(arg) instanceof OnEmptyExpr;
        }

        public boolean isEmpty() {
            return empty;
        }

        public void addTentativeContent(Item item) {
            if (tentativeContent == null) {
                tentativeContent = new ArrayList<>();
            }
            tentativeContent.add(item);
        }

        private boolean isSignificant(Item item) {
            return SignificantItemDetector.isSignificant(item);
        }

        /**
         * Open the watch. This is done at the point where it first starts watching for nodes
         * that match the selection.
         * @param terminator used for early exit
         */

        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            ConditionalBlock block = (ConditionalBlock)getExpression();
            Feed result = getResult();
            for (int i = 0; i < consumingArg; i++) {
                Expression child = block.getChildExpression(i);
                if (child instanceof OnEmptyExpr) {
                    if (empty) {
                        emptyPendingList.add((OnEmptyExpr)child);
                    }
                } else if (child instanceof OnNonEmptyExpr) {
                    if (empty) {
                        nonEmptyPendingList.add((OnNonEmptyExpr) child);
                    } else {
                        SequenceIterator<?> iter = child.iterate(getContext());
                        processItems(iter, result);
                    }
                } else {
                    child.iterate(getContext()).forEachOrFail(item -> {
                        if (empty && isSignificant(item)) {
                            empty = false;
                            for (OnNonEmptyExpr e : nonEmptyPendingList) {
                                SequenceIterator<?> si = e.iterate(getContext());
                                processItems(si, result);
                            }
                        }
                        result.processItem(item);
                    });
                }
            }
        }

        /**
         * The close() method is called immediately before the Watch is destroyed, that is,
         * when the element whose declaration scopes the constraint implemented by this
         * Watch goes out of scope. The default implementation does nothing.
         */

        public void close() throws XPathException {
            if (consumingArgIsOnEmpty) {
                // In assessing whether any items have been written, disregard any items written
                // by previous xsl:on-empty instructions
                empty = true;
            }
            ConditionalBlock block = (ConditionalBlock)getExpression();
            Feed result = getResult();
            for (int i = consumingArg + 1; i < block.size(); i++) {
                Expression child = block.getChildExpression(i);
                if (child instanceof OnEmptyExpr) {
                    if (empty) {
                        emptyPendingList.add((OnEmptyExpr) child);
                    }
                } else if (child instanceof OnNonEmptyExpr) {
                    if (empty) {
                        nonEmptyPendingList.add((OnNonEmptyExpr) child);
                    } else {
                        processItems(child.iterate(getContext()), getResult());
                    }
                } else {
                    child.iterate(getContext()).forEachOrFail(item -> {
                        if (isSignificant(item)) {
                            flushTentativeContent(result);
                            empty = false;
                        }
                        result.processItem(item);
                    });
                }
            }
            if (empty) {
                for (OnEmptyExpr e : emptyPendingList) {
                    SequenceIterator<?> si = e.iterate(getContext());
                    processItems(si, result);
                }
            }
            result.close();
        }

        private void flushTentativeContent(Feed out) throws XPathException {
            if (tentativeContent != null && !tentativeContent.isEmpty()) {
                for (Item it : tentativeContent) {
                    out.processItem(it);
                }
                tentativeContent = null;
                empty = false;
            }
            for (Expression e : nonEmptyPendingList) {
                processItems(e.iterate(getContext()), getResult());
            }
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
        public void processItem(Item<?> item) throws XPathException {
            if (empty && isSignificant(item)) {
                empty = false;
                for (OnNonEmptyExpr e : nonEmptyPendingList) {
                    SequenceIterator<?> si = e.iterate(getContext());
                    processItems(si, getResult());
                }
            }
            getResult().processItem(item);
        }

        /**
         * Signal that an element or document node has been found that matches the selection that this
         * Watch is looking for. This method is called by the WatchManager while
         * processing the startElement or startDocument event that matches the selection.
         *
         * @param node       the element or document node whose start event has been matched
         * @param locationId the location associated with the element or document node (may be
         *                   the location of the instruction that created it)
         * @return a Receiver to be notified of all events starting with the startElement/startDocument event
         *         for the matched element, and ending with the endElement event for that element; or null if this
         *         feature is not required.
         * @throws net.sf.saxon.trans.XPathException
         *          May be raised if a dynamic error occurs
         */
        public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
            if (empty) {
                empty = false;
                for (OnNonEmptyExpr e : nonEmptyPendingList) {
                    SequenceIterator<?> si = e.iterate(getContext());
                    processItems(si, getResult());
                }
            }
            return getResult().startSelectedParentNode(node, locationId);
        }

        /**
         * Signal that the endElement event has occurred for the element whose startElement
         * event caused the Watch to be activated.
         *
         * @param locationId the location of the element
         * @throws net.sf.saxon.trans.XPathException
         *          May be raised if a constraint implemented by this
         *          Watch is violated
         */
        public void endSelectedParentNode(Location locationId) throws XPathException {
            getResult().endSelectedParentNode(locationId);
        }

        /**
         * Get a receiver to which start and end events for documents and elements can be
         * written. By default this will be the same as the receiver for the result feed,
         * because very few XSLT constructs (specifically, variables and functions) allow
         * the results of document and element constructors to be captured for further
         * processing. A construct that does need to process these events will intercept
         * them by providing its own receiver, typically a SequenceOutputter.
         */
        @Override
        public Receiver getReceiver() {
            return new SignificantItemDetector(super.getReceiver(), () -> {
                flushTentativeContent(getResult());
                //empty = false;
            });
        }
    }


    public Feed makeItemFeed(final WatchManager watchManager, Feed out, final XPathContext context) throws XPathException {
        consumingArg = findConsumingArg(getExpression(), consumingArg);
        return new ConditionalBlockFeed((ConditionalBlock)getExpression(),  consumingArg, out, context);
    }



}

// Copyright (c) 2018 Saxonica Limited.