package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.TypedValueFeed;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Genre;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.UType;
import net.sf.saxon.type.coercion.AtomicCoercionPlan;
import net.sf.saxon.type.coercion.CoercionPlan;
import net.sf.saxon.type.coercion.CoercionRequest;
import net.sf.saxon.type.coercion.SequenceCoercer;
import net.sf.saxon.value.SequenceType;

import java.util.List;


/**
 * Streaming adjunct class for a coercer() (which implements the coercion rules)
 */
public class CoercerAdjunct extends ComposingAdjunct implements FeedMaker {

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        SequenceCoercer coercer = (SequenceCoercer) getExpression();
        ItemType requiredType = coercer.getItemType();
        if (requiredType.getGenre() == Genre.NODE) {
            Expression operand = coercer.getOperand().getChildExpression();
            return Streamability.getStreamability(operand, contextInfo, reasons);
        }
        return super.computeStreamability(contextInfo, reasons);
    }

    @Override
    public ItemFeed makeComposingAction(WatchManager watchManager, ItemFeed out, XPathContext context) {
        //return new TypedValueFeed(out, context);
        return makeItemFeed(watchManager, out, context);
    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context)  {
        SequenceCoercer coercer = (SequenceCoercer) getExpression();
        CoercionPlan plan = ((SequenceCoercer)getExpression()).getCoercionPlan();

        if (plan instanceof AtomicCoercionPlan) {

            UType supplied = coercer.getOperand().getChildExpression().getItemType().getUType();
            if (supplied.overlaps(UType.ANY_NODE)) {
                CoercingItemFeed converter = new CoercingItemFeed(getExpression(), out, context);
                return new TypedValueFeed(converter, context);
            }
        }
        return new CoercingItemFeed(getExpression(), out, context);
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