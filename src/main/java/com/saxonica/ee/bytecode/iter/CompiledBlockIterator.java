package com.saxonica.ee.bytecode.iter;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;

/**
 * Iterate over the instructions in the Block, concatenating the result of each instruction
 * into a single combined sequence.
 */

public abstract class CompiledBlockIterator implements SequenceIterator {

    protected int state = 0;
    protected XPathContext context;
    protected SequenceIterator currentIterator = null;

    public CompiledBlockIterator() {
    }

    public CompiledBlockIterator(XPathContext context) {
        this.context = context;
    }


    public XPathContext getContext() {
        return context;
    }

    public void setContext(XPathContext context) {
        this.context = context;
    }

    /**
     * Get the next item in the sequence. <BR>
     *
     * @return the next item, or null if there are no more items.
     * @throws net.sf.saxon.trans.XPathException
     *          if an error occurs retrieving the next item
     */

    @Override
    public abstract Item next() throws XPathException;

}

// Copyright (c) 2018-2020 Saxonica Limited

