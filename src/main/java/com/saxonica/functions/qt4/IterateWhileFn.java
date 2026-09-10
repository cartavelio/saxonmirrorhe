package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;

/**
 * This class implements the function fn:iterate-while(), which is a proposed function for XPath 4.0
 */

public class IterateWhileFn extends SystemFunction {

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        GroundedValue input = arguments[0].materialize();
        FunctionItem predicate = (FunctionItem) arguments[1].head();
        FunctionItem action = (FunctionItem) arguments[2].head();
        while (true) {
            boolean condition = ((BooleanValue) dynamicCall(predicate, context, input).head()).getBooleanValue();
            if (condition) {
                input = dynamicCall(action, context, input).materialize();
            } else {
                return input;
            }
        }
    }


}


// Copyright (c) 2023 Saxonica Limited