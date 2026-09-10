package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.TypedValueFeed;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.coercion.AtomicCoercionPlan;
import net.sf.saxon.type.coercion.CoercionPlan;
import net.sf.saxon.type.coercion.CoercionRequest;
import net.sf.saxon.type.coercion.SequenceCoercer;
import net.sf.saxon.value.SequenceType;


/**
 * Streaming adjunct class for an atomizer (or call on the data() function)
 */
public class CoercerAdjunct extends ComposingAdjunct implements FeedMaker {
    @Override
    public ItemFeed makeComposingAction(WatchManager watchManager, ItemFeed out, XPathContext context) {
        //return new TypedValueFeed(out, context);
        return makeItemFeed(watchManager, out, context);
    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context)  {
        CoercionPlan plan = ((SequenceCoercer)getExpression()).getCoercionPlan();
//        ItemType requiredType = ((SequenceCoercer) getExpression()).getItemType();
//        CoercionRequest request = new CoercionRequest(
//                SequenceType.ANY_SEQUENCE, context.getConfiguration(),
//                ((SequenceCoercer) getExpression()).getRoleSupplier(), getExpression().getLocation())

        if (plan instanceof AtomicCoercionPlan) {
            return new TypedValueFeed(out, context);
        } else {
            return new CoercingItemFeed(getExpression(), out, context);
        }
    }

    public static class CoercingItemFeed extends ItemFeed {

        final CoercionPlan plan;
        final ItemType requiredType;
        final CoercionRequest request;

        public CoercingItemFeed(Expression coercer, ItemFeed result, XPathContext context) {
            super(coercer, result, context);
            this.plan = ((SequenceCoercer) getExpression()).getCoercionPlan();
            this.requiredType = ((SequenceCoercer) getExpression()).getItemType();
            this.request = new CoercionRequest(
                    SequenceType.ANY_SEQUENCE, coercer.getConfiguration(),
                    ((SequenceCoercer) getExpression()).getRoleSupplier(), getExpression().getLocation());

        }
        public void append(Item item) throws XPathException {
            processItems(plan.coerceItem(item, requiredType, request).iterate(), getResultFeed());
        }
    }

    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        return super.getWatchMaker(forGrouping);
    }

}

// Copyright (c) 2010-2023 Saxonica Limited