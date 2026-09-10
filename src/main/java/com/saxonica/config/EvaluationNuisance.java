package com.saxonica.config;

import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.z.IntIterator;

/**
 * This filter sometimes gets inserted into a serialization pipeline when running under an evaluation
 * license: it's a deterrent to using the evaluation version for production
 */
public class EvaluationNuisance extends ProxyReceiver {

    public EvaluationNuisance(Receiver next) {
        super(next);
    }

    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
        UnicodeBuilder fsb = new UnicodeBuilder(chars.length32() + 20);
        IntIterator iter = chars.codePoints();
        int i = 0;
        while (iter.hasNext()) {
            fsb.append(iter.next());
            if (i++ % 10 == 9) {
                fsb.append('*');
            }
        }
        super.characters(fsb.toUnicodeString(), locationId, properties);
    }
}

// Copyright (c) 2009-2023 Saxonica Limited


