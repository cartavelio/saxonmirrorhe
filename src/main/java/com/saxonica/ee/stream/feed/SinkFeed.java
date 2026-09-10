package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.watch.Terminator;
import net.sf.saxon.event.SinkOutputter;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;

/**
 * An ItemFeed that swallows (discards) all input passed to it
 */

public class SinkFeed extends ItemFeed {

    public SinkFeed(XPathContext context) {
        super(new SinkOutputter(), context, true);
    }

    @Override
    public void open(Terminator terminator) throws XPathException {
    }

    @Override
    public void append(Item item) throws XPathException {

    }

    @Override
    public boolean handlesAppend() {
        return true;
    }
}

// Copyright (c) 2009-2023 Saxonica Limited


