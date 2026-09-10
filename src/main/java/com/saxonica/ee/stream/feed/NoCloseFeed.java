package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;

/**
 * This feed suppresses the call on close(). Used on some streamed grouping paths.
 */
public class NoCloseFeed extends ItemFeed {

    public NoCloseFeed(ItemFeed parentFeed, XPathContext context) {
        super(parentFeed, context);
    }

    // open and close are called once for each iteration of the for-each loop,
    // but we only want to pass them up the pipeline once at the beginning and end
    // of the instruction evaluation. So we insert a feed into the pipeline whose
    // effect is to suppress the repeated open/close calls. Test case stream-agg-018.
    @Override
    public void open(Terminator terminator) throws XPathException {
        getResultFeed().open(terminator);
    }

    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        return getResultFeed().startSelectedParentNode(node, locationId);
    }

    @Override
    public void append(Item item) throws XPathException {
        getNextOutputter().append(item);
    }

    @Override
    public void endSelectedParentNode(Location locationId) throws XPathException {
        getResultFeed().endSelectedParentNode(locationId);
    }

    @Override
    public void close() {
        // no action
    }
}

// Copyright (c) 2012-2023 Saxonica Limited
