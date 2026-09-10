package com.saxonica.ee.bytecode.iter;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.AbstractBlockIterator;
import net.sf.saxon.om.SequenceIterator;

/**
 * Iterate over the instructions in the Block, concatenating the result of each instruction
 * into a single combined sequence.
 */

public abstract class CompiledBlockIterator extends AbstractBlockIterator {

    protected int currentOperand = 0;
    protected XPathContext context;
    protected SequenceIterator currentIterator = null;

    public CompiledBlockIterator() {
        super();
    }


}

// Copyright (c) 2018-2022 Saxonica Limited

