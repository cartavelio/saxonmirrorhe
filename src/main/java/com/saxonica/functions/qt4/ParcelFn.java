package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.Parcel;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;

/**
 * This class implements the function fn:parcel(), which is a proposed function in XPath 4.0
 * to wrap a sequence as a single item
 */

public class ParcelFn extends SystemFunction {

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
        return new Parcel(arguments[0].materialize());
    }
}

// Copyright (c) 2023 Saxonica Limited.
