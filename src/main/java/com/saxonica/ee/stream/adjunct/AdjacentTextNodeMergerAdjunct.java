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
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.FastStringBuffer;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.Type;

/**
 * Compile an AdjacentTextNodeMerger expression to Java source code
 * Needed only for streaming - this class is not used in XQuery
 */

public class AdjacentTextNodeMergerAdjunct extends TransmissionAdjunct implements FeedMaker {

    public WatchMaker getWatchMaker(boolean forGrouping) {
        final AdjacentTextNodeMerger scc = (AdjacentTextNodeMerger) getExpression();
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
        return new AdjacentTextNodeMergerFeed(getExpression(), out, context);
    }

    /**
     * This class is used when a streaming template requests evaluation of a simple content constructor
     * in streaming mode (that is, when it constructs a text, attribute, comment, or PI node based on the
     * contents of the streamed document). It removes empty text nodes arriving on the input, and concatenates
     * adjacent text nodes into a single text node.
     * <p>Both a Feed and a Watch are needed because a simple content constructor operates on a sequence of nodes without
     * simply atomizing the sequence.</p>
     */


    private static class AdjacentTextNodeMergerFeed extends ItemFeed {

        private FastStringBuffer buffer = new FastStringBuffer(100);
        boolean prevText = false;

        public AdjacentTextNodeMergerFeed(Expression exp, Feed result, XPathContext context) {
            super(exp, result, context);
        }

        @Override
        public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
            flush();
            return getResult().startSelectedParentNode(node, locationId);
        }

        @Override
        public void endSelectedParentNode(Location locationId) throws XPathException {
            flush();
            getResult().endSelectedParentNode(locationId);
        }

        /**
         * Supply an item to the feed
         *
         * @param item the item being fed
         * @throws net.sf.saxon.trans.XPathException
         *          to report any failure
         */

        public void processItem(Item<?> item) throws XPathException {
            if (AdjacentTextNodeMerger.isTextNode(item)) {
                CharSequence s = item.getStringValueCS();
                if (s.length() > 0) {
                    buffer.append(s);
                    prevText = true;
                }
            } else {
                flush();
                getResult().processItem(item);
            }
        }

        private void flush() throws XPathException {
            if (prevText) {
                Orphan o = new Orphan(getPipelineConfiguration().getConfiguration());
                o.setNodeKind(Type.TEXT);
                o.setStringValue(buffer.toString());
                getResult().processItem(o);
                buffer.setLength(0);
            }
            prevText = false;

        }

        /**
         * Signal the end of the sequence of items
         *
         * @throws net.sf.saxon.trans.XPathException
         *          to report any failure
         */

        public void close() throws XPathException {
//        getContextStack().pop();
            flush();
            getResult().close();
        }

    }


}

// Copyright (c) 2010-2013 Saxonica Limited.
