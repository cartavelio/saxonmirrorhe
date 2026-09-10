package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Inversion;
import com.saxonica.ee.stream.feed.FeedMakerDelegate;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.LetExpression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharp;


/**
 * Compiles streaming code to implement a Let expression for which the "action" part (the return clause) is consuming
 */
public class LetExpressionAdjunctB extends TransmissionAdjunct {


    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        final Expression action = ((LetExpression) getExpression()).getAction();
        final Inversion ie = Inversion.invertExpression(action, forGrouping);
        final Inversion route = ie.copy();
        Inversion.PushAction letAction = new Inversion.PushAction();
        letAction.feedMaker = new FeedMakerDelegate((watchManager, out, context) ->
                  LetExpressionAdjunctB.this.makeItemFeed(watchManager, out, context));
        letAction.pushMethod = Expression.ITEM_FEED_METHOD;
        letAction.streamingChildSequence = 0;
        route.prepend(letAction);
        return CSharp.methodRef(route::getWatch);
    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, final ItemFeed out, final XPathContext context) throws XPathException {
        return new ItemFeed(getExpression(), out, context) {

            private final XPathContext context = getContext();

            @Override
            public void open(Terminator terminator) throws XPathException {
                super.open(terminator);
                try {
                    Sequence value = ((LetExpression) getExpression()).eval(context);
                    context.setLocalVariable(((LetExpression) getExpression()).getLocalSlotNumber(), value);
                } catch (XPathException e) {
                    dynamicError(e);
                }
            }

            @Override
            public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
                if (!hasFailed()) {
                    return getResultFeed().startSelectedParentNode(node, locationId);
                }
                return null;
            }

            @Override
            public void append(Item item) throws XPathException {
                if (!hasFailed()) {
                    getNextOutputter().append(item);
                }
            }

            @Override
            public void endSelectedParentNode(Location locationId) throws XPathException {
                if (!hasFailed()) {
                    getResultFeed().endSelectedParentNode(locationId);
                }
            }
        };
    }

}

// Copyright (c) 2018-2023 Saxonica Limited