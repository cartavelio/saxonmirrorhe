package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;

/**
 * A Feed which applies an ItemMappingFunction to each supplied item. This produces zero or one output
 * items for each input item. The ItemMappingFunction must be one that is capable of handling streamed nodes
 * (otherwise an UnsupportedOperationException is likely).
 */

public class ItemMappingFeed extends GroundedItemFeed {
    private final ItemMappingFunction function;

    public ItemMappingFeed(Expression expr, ItemFeed result, XPathContext context,
                           ItemMappingFunction function) {
        super(expr, result, context);
        this.function = function;
    }

    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        if (!hasFailed()) {
            Item mapped = null;
            try {
                mapped = function.mapItem(node);
            } catch (XPathException err) {
                dynamicError(err);
            }
            if (mapped != null) {
                if (mapped instanceof FleetingParentNode) {
                    return getResultFeed().startSelectedParentNode((FleetingParentNode)mapped, locationId);
                } else {
                    throw new AssertionError("Item mapping feed must map events to events");
                }
            }
        }
        return null;
    }

    @Override
    public void endSelectedParentNode(Location locationId) throws XPathException {
        if (!hasFailed()) {
            getResultFeed().endSelectedParentNode(locationId);
        }
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
        if (!hasFailed()) {
            Item mapped = null;
            try {
                mapped = function.mapItem(item);
            } catch (XPathException err) {
                dynamicError(err);
            }
            if (mapped != null) {
                getResultFeed().append(mapped);
            }
        }
    }

    /**
     * Finish evaluating the expression. The default implementation does nothing.
     *
     * @throws net.sf.saxon.trans.XPathException
     *          if a dynamic error occurs
     */
    @Override
    public void close() throws XPathException {
        if (!hasFailed()) {
            super.close();
        }
    }
}

// Copyright (c) 2009-2022 Saxonica Limited


