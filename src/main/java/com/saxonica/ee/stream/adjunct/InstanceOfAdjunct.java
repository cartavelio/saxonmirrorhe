package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.InstanceOfExpression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Cardinality;

/**
 * Generate streaming code for an "instance of" expression
 */
public class InstanceOfAdjunct extends InspectionAdjunct implements FeedMaker {


    public Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
        return new InstanceOfFeed(getExpression(), out, context);
    }

    private static class InstanceOfFeed extends ItemFeed {

        ItemType itemType;
        int count;
        int cardinality;
        boolean matchesSoFar;


        public InstanceOfFeed(Expression exp, Feed result, XPathContext context) {
            super(exp, result, context);
            itemType = ((InstanceOfExpression)exp).getRequiredItemType();
            cardinality = ((InstanceOfExpression)exp).getRequiredCardinality();
            matchesSoFar = true;
        }

        @Override
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            count = 0;
            matchesSoFar = true;
        }

        @Override
        public void processItem(Item<?> item) throws XPathException {
            if (matchesSoFar) {
                if (!itemType.matches(item, getContext().getConfiguration().getTypeHierarchy())) {
                    matchesSoFar = false;
                }
                count++;
                if (count == 2 && !Cardinality.allowsMany(cardinality)) {
                    matchesSoFar = false;
                }
                if (!matchesSoFar) {
                    getResult().processItem(BooleanValue.FALSE);
                    getResult().close();
                    getTerminator().terminate();
                }
            }
        }

        @Override
        public void close() throws XPathException {
            if (matchesSoFar) {
                if (count == 0 && !Cardinality.allowsZero(cardinality)) {
                    matchesSoFar = false;
                }
                getResult().processItem(BooleanValue.get(matchesSoFar));
                getResult().close();
            }
        }
    }
}

// Copyright (c) 2018 Saxonica Limited.