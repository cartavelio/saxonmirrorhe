package com.saxonica.functions.qt4;

import net.sf.saxon.expr.LastItemExpression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;

/**
 * This class implements the function fn:foot(), which is proposed as a standard function in XPath 4.0
 */

public class FootFn extends SystemFunction {

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
        try {
            Item foot = LastItemExpression.getLast(arguments[0].iterate());
            return SequenceTool.itemOrEmpty(foot);
        } catch (UncheckedXPathException e) {
            throw e.getXPathException();
        }
    }
}

// Copyright (c) 2023 Saxonica Limited.
