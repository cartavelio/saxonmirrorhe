package com.saxonica.functions.qt4;

import net.sf.saxon.expr.ItemMappingIterator;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.LazySequence;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.LookaheadIterator;
import net.sf.saxon.tree.iter.LookaheadIteratorImpl;

/**
 * This class implements the function fn:trunk(), which is proposed as a standard function in XPath 4.0
 */

public class Trunk extends SystemFunction {

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as Sequences
     * @return the result of the evaluation, in the form of a Sequence
     * @throws XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        LookaheadIterator spy =
                LookaheadIteratorImpl.makeLookaheadIterator(arguments[0].iterate());
        ItemMappingIterator mapper =
                new ItemMappingIterator(spy, item -> spy.hasNext() ? item : null);
        return new LazySequence(mapper);
    }
    
}

// Copyright (c) 2023 Saxonica Limited.
