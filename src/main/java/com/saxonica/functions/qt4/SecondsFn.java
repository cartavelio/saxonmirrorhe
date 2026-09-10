package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.DayTimeDurationValue;
import net.sf.saxon.value.DecimalValue;
import net.sf.saxon.value.EmptySequence;

/**
 * This class implements the function fn:seconds(), which is a proposed function for XPath 4.0
 */

public class SecondsFn extends SystemFunction {

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments
     * @return the result of the evaluation
     * @throws XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        DecimalValue dv = (DecimalValue)arguments[0].head();
        if (dv == null) {
            return EmptySequence.getInstance();
        }
        return new DayTimeDurationValue(dv.getDecimalValue());

    }
}


// Copyright (c) 2023 Saxonica Limited