package com.saxonica.xqj.pull;

import net.sf.saxon.pull.PullEvent;
import net.sf.saxon.pull.PullFilter;
import net.sf.saxon.pull.PullProvider;
import net.sf.saxon.trans.XPathException;

/**
 * Returns the events provided by an underlying PullProvider, with the addition of a START_DOCUMENT
 * event immediately after the START_OF_INPUT, and an END_DOCUMENT event immediately before the
 * END_OF_INPUT
 */
public class DocumentWrappingPullProvider extends PullFilter {

    boolean atStart = true;
    boolean atEnd = false;

    public DocumentWrappingPullProvider(PullProvider base) {
        super(base);
    }

    @Override
    public PullEvent next() throws XPathException {
        if (atStart) {
            atStart = false;
            return PullEvent.START_DOCUMENT;
        } else if (atEnd) {
            return PullEvent.END_OF_INPUT;
        } else {
            PullEvent event = getUnderlyingProvider().next();
            if (event == PullEvent.END_OF_INPUT) {
                atEnd = true;
                return PullEvent.END_DOCUMENT;
            } else {
                return event;
            }
        }
    }
}

// Copyright (c) 2018-2023 Saxonica Limited

