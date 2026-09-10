package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.DocumentOrderIterator;
import net.sf.saxon.expr.sort.GlobalOrderComparer;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.LazySequence;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.EmptySequence;

/**
 * This class implements the function fn:distinct-ordered-nodes(), which is a proposed function for XPath 4.0
 */

public class DistinctOrderedNodes extends SystemFunction {

    /**
     * Evaluate the function call
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        if (arguments.length == 0) {
            return EmptySequence.getInstance();
        }
        SequenceIterator in = arguments[0].iterate();
        SequenceIterator out = new DocumentOrderIterator(in, GlobalOrderComparer.getInstance());
        return new LazySequence(out);
    }


}


// Copyright (c) 2023 Saxonica Limited