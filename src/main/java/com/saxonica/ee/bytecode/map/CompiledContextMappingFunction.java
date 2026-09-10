package com.saxonica.ee.bytecode.map;

import net.sf.saxon.expr.ContextMappingFunction;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;

/**
 * Abstract ContextMappingFunction class that must be passed to a ContextMappingIterator.
 * This is a specialization of the more general MappingFunction class: it differs in that
 * each item being processed becomes the context item while it is being processed.
 */
public abstract class CompiledContextMappingFunction implements ContextMappingFunction {

    public CompiledContextMappingFunction() {
    }

    @Override
    public abstract SequenceIterator map(XPathContext context) throws XPathException;

}

// Copyright (c) 2011-2022 Saxonica Limited
