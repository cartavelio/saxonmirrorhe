package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.ma.arrays.SquareArrayConstructor;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.SequenceExtent;

import java.util.ArrayList;
import java.util.List;


/**
 * Generate streaming code for a SquareArrayConstructor, that is a sequence of expressions or instructions
 * used to establish the content of an XDM array
 */
public class ArrayBlockAdjunct extends TransmissionAdjunct implements FeedMaker {

    private int consumingArg = -1;

    /**
     * Get a WatchMaker, a factory class that creates a Watch to evaluate this expression
     * in streaming mode. This method will only be called if the getImplementationMethod()
     * method of the expression includes the value {@link Expression#WATCH_METHOD}
     *
     * @return a Watchmaker to evaluate the expression, or null if it is not streamable with respect
     *         to the specificed argument.
     * @throws XPathException
     *          in the event of a failure
     * @param forGrouping true if the selected items are being grouped
     */
    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        consumingArg = findConsumingArg(getExpression(), consumingArg);
        return super.getWatchMaker(forGrouping);
    }

    private static class SquareArrayFeed extends ItemFeed {

        int consumingArg;
        List<GroundedValue> members;
        List<Item> consumingMember = new ArrayList<>();

        public SquareArrayFeed(SquareArrayConstructor exp, int arg, ItemFeed result, XPathContext context) {
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
            SquareArrayConstructor block = (SquareArrayConstructor)getExpression();
            members = new ArrayList<>();
            for (int i = 0; i < consumingArg; i++) {
                Expression child = block.getOperanda().getOperand(i).getChildExpression();
                GroundedValue member = SequenceExtent.from(child.iterate(getContext()));
                members.add(member);
            }
        }

        /**
         * Signal that a node (other than a document or element node) that matches the selection
         * has been found, or that a grounded item is being passed to the next expression in the
         * evaluation pipeline.
         *
         * @param item the node that was matched.
         * @throws XPathException May be raised if a dynamic error occurs
         */
        @Override
        public void append(Item item) throws XPathException {
            consumingMember.add(item);
        }


        /**
         * The close() method is called immediately before the Watch is destroyed, that is,
         * when the element whose declaration scopes the constraint implemented by this
         * Watch goes out of scope. The default implementation does nothing.
         */

        @Override
        public void close() throws XPathException {
            members.add(SequenceExtent.makeSequenceExtent(consumingMember));
            SquareArrayConstructor block = (SquareArrayConstructor)getExpression();
            Outputter result = getNextOutputter();
            for (int i = consumingArg + 1; i < block.getOperanda().getNumberOfOperands(); i++) {
                Expression child = block.getOperanda().getOperand(i).getChildExpression();
                GroundedValue member = SequenceExtent.from(child.iterate(getContext()));
                members.add(member);
            }
            ArrayItem resultArray = new SimpleArrayItem(members);
            result.append(resultArray);
            result.close();
        }

    }


    @Override
    public ItemFeed makeItemFeed(final WatchManager watchManager, ItemFeed out, final XPathContext context) throws XPathException {
        consumingArg = findConsumingArg(getExpression(), consumingArg);
        return new SquareArrayFeed((SquareArrayConstructor)getExpression(), consumingArg, out, context);
    }

}

// Copyright (c) 2010-2023 Saxonica Limited