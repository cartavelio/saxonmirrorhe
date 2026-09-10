package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;

/**
 * This feed suppresses the call on close(). Used on some streamed grouping paths.
 */
public class NoCloseFeed extends ItemFeed {

    public NoCloseFeed(Feed parentFeed, XPathContext context) {
        super(parentFeed, context);
    }

    // open and close are called once for each iteration of the for-each loop,
    // but we only want to pass them up the pipeline once at the beginning and end
    // of the instruction evaluation. So we insert a feed into the pipeline whose
    // effect is to suppress the repeated open/close calls. Test case stream-agg-018.
    public void open(Terminator terminator) throws XPathException {
        getResult().open(terminator);
    }

    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        return getResult().startSelectedParentNode(node, locationId);
    }

    public void processItem(Item<?> item) throws XPathException {
        getResult().processItem(item);
    }

    @Override
    public void endSelectedParentNode(Location locationId) throws XPathException {
        getResult().endSelectedParentNode(locationId);
    }

    public void close() {
        // no action
    }
}

// Copyright (c) 2012-2013 Saxonica Limited
