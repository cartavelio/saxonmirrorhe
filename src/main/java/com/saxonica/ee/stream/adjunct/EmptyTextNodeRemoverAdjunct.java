package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.StreamingPatternMaker;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.SimpleContentFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.*;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.AdjacentTextNodeMerger;
import net.sf.saxon.expr.EmptyTextNodeRemover;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;

/**
 * Compile an EmptyTextNodeRemover expression to Java source code
 * Needed only for streaming - this class is not used in XQuery
 */

public class EmptyTextNodeRemoverAdjunct extends StreamingAdjunct implements FeedMaker {

    public WatchMaker getWatchMaker(boolean forGrouping) {
        final EmptyTextNodeRemover scc = (EmptyTextNodeRemover) getExpression();
        if (Streamability.getSweep(scc) == Sweep.CONSUMING) {
            final Expression select = scc.getBaseExpression();
            final Pattern selection = StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);
            if (selection != null) {
                return new WatchMaker() {
                    public Trigger makeWatch(WatchManager watchManager, Feed out, final XPathContext context) {
                        SimpleContentFeed action = new SimpleContentFeed(out);
                        Trigger watch = new Trigger(selection, action, context);
                        watch.setPipelineConfiguration(watchManager.getPipelineConfiguration());
                        return watch;
                    }
                };
            }
            return null;
        } else {
            return null;
        }
    }

    public Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
        return new ItemFeed(getExpression(), out, context) {

            @Override
            public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
                return getResult().startSelectedParentNode(node, locationId);
            }

            @Override
            public void endSelectedParentNode(Location locationId) throws XPathException {
                getResult().endSelectedParentNode(locationId);
            }

            @Override
            public void processItem(Item<?> item) throws XPathException {
                if (!AdjacentTextNodeMerger.isTextNode(item) || item.getStringValueCS().length() != 0) {
                    getResult().processItem(item);
                } else {
                    // no-op: discard this empty text node
                }
            }
        };
    }
}

// Copyright (c) 2010-2013 Saxonica Limited.
