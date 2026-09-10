package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.SequenceType;

/**
 * This class implements the function fn:compose(), which is a proposed function for XPath 4.0
 */

public class Compose extends SystemFunction {

    /**
     * Evaluate the function
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        return new CallableFunction((cxt, args) -> {
            SequenceIterator iter = arguments[0].iterate();
            Item it;
            Sequence val = args[0];
            while ((it = iter.next()) != null) {
                FunctionItem fi = (FunctionItem)it;
                val = fi.call(cxt, new Sequence[]{val});
            }
            return val;
        }, new SpecificFunctionType(SequenceType.ANY_SEQUENCE, SequenceType.ANY_SEQUENCE));
    }
}


// Copyright (c) 2023 Saxonica Limited