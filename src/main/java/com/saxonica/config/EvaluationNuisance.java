package com.saxonica.config;

import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.FastStringBuffer;

/**
 * This filter sometimes gets inserted into a serialization pipeline when running under an evaluation
 * license: it's a deterrent to using the evaluation version for production
 */
public class EvaluationNuisance extends ProxyReceiver {

    public EvaluationNuisance(Receiver next) {
        super(next);
    }

    @Override
    public void characters(CharSequence chars, Location locationId, int properties) throws XPathException {
        FastStringBuffer fsb = new FastStringBuffer(chars.length());
        for (int i = 0; i < chars.length(); i++) {
            fsb.cat(chars.charAt(i));
            if (i % 10 == 9) {
                fsb.cat('*');
            }
        }
        super.characters(fsb, locationId, properties);
    }
}

// Copyright (c) 2009-2020 Saxonica Limited


