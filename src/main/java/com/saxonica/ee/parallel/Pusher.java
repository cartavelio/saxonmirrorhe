package com.saxonica.ee.parallel;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.QuitParsingException;
import net.sf.saxon.trans.XPathException;

/**
 * Interface implemented by instructions that push data to an Outputter
 */

public interface Pusher {
    /**
     * Evaluate an instruction in push mode
     *
     * @param output  the destination for the result
     * @param context the evaluation context
     * @throws XPathException       in the event of a failure
     * @throws QuitParsingException if there was an early exit, that is, if the instruction was evaluated
     *                              without reading the input to completion
     */

    void push(Outputter output, XPathContext context) throws XPathException;
}

// Copyright (c) 2021-2022 Saxonica Limited