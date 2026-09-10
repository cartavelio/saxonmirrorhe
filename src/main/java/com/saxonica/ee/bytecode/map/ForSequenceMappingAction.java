package com.saxonica.ee.bytecode.map;

import net.sf.saxon.expr.*;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;

/**
 * The ForSequenceMappingAction supports compilation of a ForExpression.
 * It represents the action to be taken for each item in the
 * source sequence. It acts as the MappingFunction for the mapping iterator. The map()
 * method is generated in the subclass, and performs the "action" part of the for expression,
 * being called once per iteration, and returning an iterator over the results.
 */
public abstract class ForSequenceMappingAction implements MappingFunction {

    protected XPathContext context;
    protected int position = 0;

    public ForSequenceMappingAction() {
    }

    public void setContext(XPathContext context) {
        this.context = context;
    }

    @Override
    public abstract SequenceIterator map(Item item) throws XPathException;

}

// Copyright (c) 2011-2022 Saxonica Limited
