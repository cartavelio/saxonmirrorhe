package com.saxonica.ee.stream.feed;

import com.saxonica.ee.validate.Checker;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.FastStringBuffer;


/**
 * A StringValueAccumulator is a Receiver that computes the string value of an element in streaming mode, by
 * copying all the text nodes to a supplied receiver.
 */

public class StringValueGatherer extends Checker {

    private FastStringBuffer buffer;

    public StringValueGatherer(FastStringBuffer buffer) {
        this.buffer = buffer;
    }

    public void characters(CharSequence chars, Location locationId, int properties) throws XPathException {
        buffer.append(chars);
    }

}

// Copyright (c) 2009 Saxonica Limited.