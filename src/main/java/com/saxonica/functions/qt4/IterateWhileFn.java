package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Int64Value;

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
        int predicateArity = predicate.getArity();
        Sequence[] predicateParams = new Sequence[predicateArity];

        FunctionItem action = (FunctionItem) arguments[2].head();
        int actionArity = action.getArity();
        Sequence[] actionParams = new Sequence[actionArity];

        int pos = 0;
        while (true) {
            pos++;
            if (predicateArity > 0) {
                predicateParams[0] = input;
                if (predicateArity > 1) {
                    predicateParams[1] = new Int64Value(pos);
                }
            }
            boolean condition = ((BooleanValue) dynamicCall(predicate, context, predicateParams).head()).getBooleanValue();
            if (condition) {
                if (actionArity > 0) {
                    actionParams[0] = input;
                    if (actionArity > 1) {
                        actionParams[1] = new Int64Value(pos);
                    }
                }
                input = dynamicCall(action, context, actionParams).materialize();
            } else {
                return input;
            }
        }
    }


}


// Copyright (c) 2023 Saxonica Limited