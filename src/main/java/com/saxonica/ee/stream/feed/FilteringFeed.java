package com.saxonica.ee.stream.feed;


import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpDelegate;
import net.sf.saxon.transpile.CSharpSimpleEnum;

import java.util.Stack;

/**
 * This class selects a subset of the items, nodes, or events selected by some supplying expression. The
 * subset is determined as a function of the position of the node (among all the nodes
 * matched since the watch was activated).
 */

public class FilteringFeed extends GroundedItemFeed {

    private final Filter filter;
    private int position;
    private final Stack<Boolean> matchStack = new Stack<>();
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
        getResultFeed().open(terminator);
        position = 0;
        this.terminated = false;
    }

    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        if (!terminated) {
            FilterResult match = filter.matches(node, ++position);
            matchStack.push(match == FilterResult.MATCHES);
            //nodeKindStack.push(node.getNodeKind());
            if (match == FilterResult.MATCHES) {
                return getResultFeed().startSelectedParentNode(node, locationId);
                //node.notify(getNextOutputter());
            } else if (match == FilterResult.DONE && getTerminator() != null) {
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
                getResultFeed().endSelectedParentNode(locationId);
            }
        }
    }

    @Override
    public void append(Item item) throws XPathException {
        if (!terminated && !hasFailed()) {
            try {
                FilterResult match = filter.matches(item, ++position);
                if (match == FilterResult.MATCHES) {
                    getNextOutputter().append(item);
                } else if (match == FilterResult.DONE && getTerminator() != null) {
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
     * Three possible results of the Filter#matches operation
     */

    @CSharpSimpleEnum
    public enum FilterResult {
        /**
         * Item was not matched
         */
        SKIP,
        /**
         * Item was matched
         */
        MATCHES,
        /**
         * Item was not matched and no further items need to be tested, they can be treated as non-matching
         */
        DONE
    }

    /**
     * Interface defining the filter function, to be implemented by the caller
     */

    @FunctionalInterface
    @CSharpDelegate(false)
    public interface Filter {

        /**
         * Test whether a given item, at a given position in the sequence, matches the filter
         * @param item the item to be matched
         * @param position the 1-based position of the item in the sequence
         * @return MATCHES if the item is to be included in the result, SKIP if it is to be excluded;
         * DONE if this and all future items are to be excluded
         * @throws XPathException if a dynamic error occurs
         */
        FilterResult matches(Item item, int position) throws XPathException;
    }

    @FunctionalInterface
    @CSharpDelegate(true)
    public interface FilterDelegate {
        FilterResult matches(Item item, int position) throws XPathException;
    }

    /**
     * An implementation of Filter that accepts lambda expressions (C# delegates)
     */

    public static class FilterLambda implements Filter {
        private final FilterDelegate delegate;
        public FilterLambda(FilterDelegate delegate) {
            this.delegate = delegate;
        }

        @Override
        public FilterResult matches(Item item, int position) throws XPathException {
            return delegate.matches(item, position);
        }
    }

    /**
     * A filter that matches everything
     */

    public static FilterLambda TransparentFilter = new FilterLambda((item, position) -> FilterResult.MATCHES);

    /**
     * A filter that matches nothing
     */

    public static FilterLambda OpaqueFilter = new FilterLambda((item, position) -> FilterResult.DONE);

}

// Copyright (c) 2018-2022 Saxonica Limited