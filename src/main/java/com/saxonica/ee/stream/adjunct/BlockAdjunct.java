package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.Block;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;


/**
 * Generate streaming code for a Block, that is a sequence of expressions or instructions
 */
public class BlockAdjunct extends TransmissionAdjunct implements FeedMaker {

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

    private static class BlockFeed extends ItemFeed {

        int consumingArg;

        public BlockFeed(Block exp, int arg, ItemFeed result, XPathContext context) {
            super(exp, result, context);
            this.consumingArg = arg;
        }

        /**
         * Open the watch. This is done at the point where it first starts watching for nodes
         * that match the selection.
         * @param terminator used for early exit
         */

        @Override
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            Block block = (Block)getExpression();
            Outputter result = getNextOutputter();
            for (int i = 0; i < consumingArg; i++) {
                Expression child = block.getOperanda()[i].getChildExpression();
                SequenceIterator iter = child.iterate(getContext());
                processItems(iter, result);
            }
        }

        /**
         * The close() method is called immediately before the Watch is destroyed, that is,
         * when the element whose declaration scopes the constraint implemented by this
         * Watch goes out of scope. The default implementation does nothing.
         */

        @Override
        public void close() throws XPathException {
            Block block = (Block)getExpression();
            Outputter result = getNextOutputter();
            for (int i = consumingArg + 1; i < block.getOperanda().length; i++) {
                Expression child = block.getOperanda()[i].getChildExpression();
                SequenceIterator iter = child.iterate(getContext());
                processItems(iter, result);
            }
            result.close();
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
            getNextOutputter().append(item);
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
        @Override
        public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
            return getResultFeed().startSelectedParentNode(node, locationId);
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
        @Override
        public void endSelectedParentNode(Location locationId) throws XPathException {
            getResultFeed().endSelectedParentNode(locationId);
        }
    }


    @Override
    public ItemFeed makeItemFeed(final WatchManager watchManager, ItemFeed out, final XPathContext context) throws XPathException {
        consumingArg = findConsumingArg(getExpression(), consumingArg);
        return new BlockFeed((Block)getExpression(),  consumingArg, out, context);
    }

}

// Copyright (c) 2010-2023 Saxonica Limited