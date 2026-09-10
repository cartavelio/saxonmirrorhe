package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.SequenceReceiver;
import net.sf.saxon.event.TreeReceiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.tiny.TinyBuilder;


/**
 * Generate streaming code for a consuming xsl:on-non-empty instruction
 */
public class OnNonEmptyAdjunct extends TransmissionAdjunct {

    @Override
    public Feed makeItemFeed(WatchManager watchManager, final Feed out, XPathContext context) throws XPathException {
        return new ItemFeed(out, context) {
            final ConditionalBlockAdjunct.ConditionalBlockFeed nextFeed = (ConditionalBlockAdjunct.ConditionalBlockFeed) out;
            private SequenceReceiver builder = null;

            @Override
            public void processItem(Item<?> item) throws XPathException {
                if (!nextFeed.isEmpty()) {
                    out.processItem(item);
                } else {
                    nextFeed.addTentativeContent(item);
                }
            }

            @Override
            public Receiver getReceiver() {
                if (!nextFeed.isEmpty()) {
                    return out.getReceiver();
                } else if (builder != null) {
                    return builder;
                } else {
                    final TinyBuilder tb = new TinyBuilder(getPipelineConfiguration());
                    final TreeReceiver tr = new TreeReceiver(tb);
                    builder = new ProxyReceiver(tr) {
                        @Override
                        public void close() throws XPathException {
                            nextFeed.addTentativeContent(tb.getCurrentRoot());
                        }
                    };
                    try {
                        builder.open();
                    } catch (XPathException e) {
                        throw new AssertionError(e);
                    }
                    return builder;
                }
            }
        };
    }

//    /**
//     * Make a filter for the event stream
//     *
//     * @param context the XPath dynamic context
//     * @param out the next feed in the pipeline, which will always be the feed for the containing ConditionalBlock
//     *            instruction
//     * @return a filter that can be added to the event stream
//     * @throws XPathException if a dynamic evaluation error occurs
//     */
//    @Override
//    public FilteringFeed.Filter makeFilter(XPathContext context, Feed out) throws XPathException {
//        final ConditionalBlockAdjunct.ConditionalBlockFeed nextFeed = (ConditionalBlockAdjunct.ConditionalBlockFeed)out;
//        return new FilteringFeed.Filter() {
//            public int matches(Item item, int position) throws XPathException {
//                if (!nextFeed.isEmpty()) {
//                    return MATCHES;
//                } else {
//                    nextFeed.addTentativeContent(item);
//                    return SKIP;
//                }
//            }
//        };
//    }

}

// Copyright (c) 2018 Saxonica Limited.