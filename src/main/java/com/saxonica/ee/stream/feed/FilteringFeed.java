package com.saxonica.ee.stream.feed;


import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOptions;
import net.sf.saxon.event.SequenceReceiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;

import java.util.Stack;

/**
 * This class selects a subset of the items, nodes, or events selected by some supplying expression. The
 * subset is determined as a function of the position of the node (among all the nodes
 * matched since the watch was activated).
 */

public class FilteringFeed extends ItemFeed {

    private Filter filter;
    private int position;
    private Stack<Boolean> matchStack = new Stack<>();
    private boolean terminated;

    /**
     * Create a filtering feed
     * @param nextFeed the feed to which the filtered items/events will be sent
     * @param filter a function which tests whether the item/node/event at a particular
     * position is to be selected
     */

    public FilteringFeed(Feed nextFeed, XPathContext context, Filter filter) {
        super(nextFeed, context);
        this.filter = filter;
    }

    public void open(Terminator terminator) throws XPathException {
        getResult().open(terminator);
        position = 0;
        this.terminated = false;
    }

    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        if (!terminated) {
            int match = filter.matches(node, ++position);
            matchStack.push(match == Filter.MATCHES);
            if (match == Filter.MATCHES) {
                return getResult().startSelectedParentNode(node, locationId);
            } else if (match == Filter.DONE && getTerminator() != null) {
                getResult().close();
                terminated = true;
                getTerminator().terminate();
            }
        }
        return null;
    }

    public void endSelectedParentNode(Location locationId) throws XPathException {
        if (!terminated && matchStack.pop()) {
            getResult().endSelectedParentNode(locationId);
        }
    }

    public void processItem(Item<?> item) throws XPathException {
        if (!terminated && !hasFailed()) {
            try {
                int match = filter.matches(item, ++position);
                if (match == Filter.MATCHES) {
                    getResult().processItem(item);
                } else if (match == Filter.DONE && getTerminator() != null) {
                    getResult().close();
                    terminated = true;
                    getTerminator().terminate();
                }
            } catch (XPathException e) {
                dynamicError(e);
            }
        }
    }

    public void close() throws XPathException {
        if (!terminated && !hasFailed()) {
            getResult().close();
        }
    }

    /**
     * Get a receiver to which start and end events for documents and elements can be
     * written.
     */
    @Override
    public SequenceReceiver getReceiver() {
        Receiver next = getResult().getReceiver();
        return new ProxyReceiver(next) {
            /**
             * Append an arbitrary item (node or atomic value) to the output
             *
             * @param item           the item to be appended
             * @param locationId     the location of the calling instruction, for diagnostics
             * @param copyNamespaces if the item is an element node, this indicates whether its namespaces
             *                       need to be copied. Values are {@link ReceiverOptions#ALL_NAMESPACES},
             *                       {@link ReceiverOptions#LOCAL_NAMESPACES}; the default (0) means
             *                       no namespaces
             */
            @Override
            public void append(Item item, Location locationId, int copyNamespaces) throws XPathException {
                processItem(item);
            }
        };
    }

    /**
     * Interface defining the filter function, to be implemented by the caller
     */

    public interface Filter {
        int SKIP = 0;
        int MATCHES = 1;
        int DONE = 2;
        /**
         * Test whether a given item, at a given position in the sequence, matches the filter
         * @param item the item to be matched
         * @param position the 1-based position of the item in the sequence
         * @return MATCHES if the item is to be included in the result, SKIP if it is to be excluded;
         * DONE if this and all future items are to be excluded
         * @throws XPathException if a dynamic error occurs
         */
        int matches(Item item, int position) throws XPathException;
    }

    /**
     * A filter that matches everything
     */

    public static class TransparentFilter implements Filter {
        /**
         * Test whether a given item, at a given position in the sequence, matches the filter
         *
         * @param item     the item to be matched
         * @param position the 1-based position of the item in the sequence
         * @return MATCHES if the item is to be included in the result, SKIP if it is to be excluded;
         * DONE if this and all future items are to be excluded
         * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs
         */
        public int matches(Item item, int position) throws XPathException {
            return MATCHES;
        }
    }

    /**
     * A filter that matches nothing
     */

    public static class OpaqueFilter implements Filter {
        /**
         * Test whether a given item, at a given position in the sequence, matches the filter
         *
         * @param item     the item to be matched
         * @param position the 1-based position of the item in the sequence
         * @return MATCHES if the item is to be included in the result, SKIP if it is to be excluded;
         * DONE if this and all future items are to be excluded
         * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs
         */
        public int matches(Item item, int position) throws XPathException {
            return DONE;
        }
    }

}

// Copyright (c) 2018 Saxonica Limited. All rights reserved