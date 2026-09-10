package com.saxonica.ee.stream.feed;


import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;

import java.util.Stack;

/**
 * This class selects a subset of the items, nodes, or events selected by some supplying expression. The
 * subset is determined as a function of the position of the node (among all the nodes
 * matched since the watch was activated).
 */

public class FilteringFeed extends GroundedItemFeed {

    private Filter filter;
    private int position;
    private Stack<Boolean> matchStack = new Stack<>();
    //private Stack<Integer> nodeKindStack = new Stack<>();
    private boolean terminated;

    /**
     * Create a filtering feed
     * @param nextFeed the feed to which the filtered items/events will be sent
     * @param filter a function which tests whether the item/node/event at a particular
     * position is to be selected
     */

    public FilteringFeed(ItemFeed nextFeed, XPathContext context, Filter filter) {
        super(nextFeed, context);
        this.filter = filter;
    }

    @Override
    public void open(Terminator terminator) throws XPathException {
        getResult().open(terminator);
        position = 0;
        this.terminated = false;
    }

    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        if (!terminated) {
            int match = filter.matches(node, ++position);
            matchStack.push(match == Filter.MATCHES);
            //nodeKindStack.push(node.getNodeKind());
            if (match == Filter.MATCHES) {
                return getResult().startSelectedParentNode(node, locationId);
                //node.notify(getNextOutputter());
            } else if (match == Filter.DONE && getTerminator() != null) {
                getNextOutputter().close();
                terminated = true;
                getTerminator().terminate();
            }
        }
        return null;
    }

    @Override
    public void endSelectedParentNode(Location locationId) throws XPathException {
        if (!terminated) {
            boolean matches = matchStack.pop();
            //int kind = nodeKindStack.pop();
            if (matches) {
                getResult().endSelectedParentNode(locationId);
            }
        }
    }

    @Override
    public void append(Item item) throws XPathException {
        if (!terminated && !hasFailed()) {
            try {
                int match = filter.matches(item, ++position);
                if (match == Filter.MATCHES) {
                    getNextOutputter().append(item);
                } else if (match == Filter.DONE && getTerminator() != null) {
                    getNextOutputter().close();
                    terminated = true;
                    getTerminator().terminate();
                }
            } catch (XPathException e) {
                dynamicError(e);
            }
        }
    }

    @Override
    public void close() throws XPathException {
        if (!terminated && !hasFailed()) {
            getNextOutputter().close();
        }
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
        @Override
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
        @Override
        public int matches(Item item, int position) throws XPathException {
            return DONE;
        }
    }

}

// Copyright (c) 2018-2020 Saxonica Limited