package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.StreamingPatternMaker;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.SimpleContentFeed;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.AdjacentTextNodeMerger;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceMap;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.FastStringBuffer;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.Type;

/**
 * Streaming implementation of an AdjacentTextNodeMerger expression
 */

public class AdjacentTextNodeMergerAdjunct extends TransmissionAdjunct implements FeedMaker {

    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) {
        final AdjacentTextNodeMerger scc = (AdjacentTextNodeMerger) getExpression();
        if (Streamability.getSweep(scc) == Sweep.CONSUMING) {
            final Expression select = scc.getBaseExpression();
            final Pattern selection = StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);
            if (selection != null) {
                return new WatchMaker() {
                    @Override
                    public Trigger makeWatch(WatchManager watchManager, ItemFeed out, final XPathContext context) {
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


    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new AdjacentTextNodeMergerFeed(getExpression(), out, context);
    }

    /**
     * This class is used when a streaming template requests evaluation of a simple content constructor
     * in streaming mode (that is, when it constructs a text, attribute, comment, or PI node based on the
     * contents of the streamed document). It removes empty text nodes arriving on the input, and concatenates
     * adjacent text nodes into a single text node.
     * <p>Both a ItemFeed and a Watch are needed because a simple content constructor operates on a sequence of nodes without
     * simply atomizing the sequence.</p>
     */


    private static class AdjacentTextNodeMergerFeed extends ItemFeed {

        private FastStringBuffer buffer = new FastStringBuffer(100);
        private boolean prevText = false;

        AdjacentTextNodeMergerFeed(Expression exp, ItemFeed result, XPathContext context) {
            super(exp, result, context);
        }

        /**
         * Supply an item to the feed
         *
         * @param item the item being fed
         * @throws net.sf.saxon.trans.XPathException
         *          to report any failure
         */

        @Override
        public void append(Item item) throws XPathException {
            if (AdjacentTextNodeMerger.isTextNode(item)) {
                CharSequence s = item.getStringValueCS();
                if (s.length() > 0) {
                    buffer.cat(s);
                    prevText = true;
                }
            } else {
                flush();
                getNextOutputter().append(item);
            }
        }

        @Override
        public void characters(CharSequence chars, Location location, int properties) throws XPathException {
            if (chars.length() > 0) {
                buffer.cat(chars);
                prevText = true;
            }
        }

        private void flush() throws XPathException {
            if (prevText) {
                Orphan o = new Orphan(getPipelineConfiguration().getConfiguration());
                o.setNodeKind(Type.TEXT);
                o.setStringValue(buffer.toString());
                getNextOutputter().append(o);
                buffer.setLength(0);
            }
            prevText = false;

        }

        @Override
        public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
            prevText = false;
            super.startElement(elemName, typeCode, location, properties);
        }

        @Override
        public void startElement(NodeName elemName, SchemaType type, AttributeMap attributes, NamespaceMap namespaces, Location location, int properties) throws XPathException {
            prevText = false;
            super.startElement(elemName, type, attributes, namespaces, location, properties);
        }

        @Override
        public void endElement() throws XPathException {
            prevText = false;
            super.endElement();
        }

        @Override
        public void processingInstruction(String name, CharSequence data, Location location, int properties) throws XPathException {
            prevText = false;
            super.processingInstruction(name, data, location, properties);
        }

        @Override
        public void comment(CharSequence content, Location location, int properties) throws XPathException {
            prevText = false;
            super.comment(content, location, properties);
        }

        /**
         * Signal the end of the sequence of items
         *
         * @throws net.sf.saxon.trans.XPathException
         *          to report any failure
         */

        @Override
        public void close() throws XPathException {
//        getContextStack().pop();
            flush();
            getNextOutputter().close();
        }

    }


}

// Copyright (c) 2010-2020 Saxonica Limited
