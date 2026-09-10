package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.ContextItemExpression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;


/**
 * Generate streaming code for an accumulator-after() function call, particularly in the case where the instruction
 * is consuming and therefore has to defer evaluation until the endElement event.
 */
public class AccumulatorAfterStreamer extends InspectionAdjunct implements FeedMaker {

    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        return getInspectionWatchMaker(getExpression(), new ContextItemExpression(), forGrouping);
    }


    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new ItemFeed(getExpression(), out, context) {

            /**
             * Supply one item towards the streamed input of the expression
             *
             * @param item the item to be supplied
             */
            @Override
            public void append(Item item) throws XPathException {
                // do nothing
            }

            /**
             * Signal that the endElement event has occurred for the element whose startElement
             * event caused the Watch to be activated.
             *
             * @param locationId the location of the element
             * @throws XPathException May be raised if a constraint implemented by this
             *                        Watch is violated
             */
            @Override
            public void endSelectedParentNode(Location locationId) throws XPathException {
                processItems(getExpression().iterate(getContext()), getResultFeed());
            }

        };
    }

}

// Copyright (c) 2018-2022 Saxonica Limited