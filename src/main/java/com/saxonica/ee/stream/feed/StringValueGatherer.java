package com.saxonica.ee.stream.feed;

import com.saxonica.ee.validate.AbstractReceiver;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.str.UnicodeBuilder;


/**
 * A StringValueGatherer is a Receiver that computes the string value of an element in streaming mode, by
 * copying all the text nodes to a supplied receiver.
 */

public class StringValueGatherer extends AbstractReceiver {

    private final UnicodeBuilder buffer;

    public StringValueGatherer(UnicodeBuilder buffer) {
        this.buffer = buffer;
    }

    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
        buffer.accept(chars);
    }

}

// Copyright (c) 2009-2023 Saxonica Limited