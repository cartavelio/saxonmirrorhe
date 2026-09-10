package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.functions.hof.CoercedFunction;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.SequenceType;

/**
 * This class implements the function fn:some(), which is a proposed function for XPath 4.0
 */

public class SomeFn extends SystemFunction {

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
        SequenceIterator iter = arguments[0].iterate();
        FunctionItem predicate = arguments.length > 1
                ? (FunctionItem) arguments[1].head()
                : new CoercedFunction(SystemFunction.makeFunction("identity", getRetainedStaticContext(), 1),
                                      new SpecificFunctionType(
                                            new SequenceType[]{SequenceType.SINGLE_ITEM},
                                            SequenceType.SINGLE_BOOLEAN), false);
        for (Item item; (item = iter.next()) != null; ) {
            boolean condition = ((BooleanValue) dynamicCall(predicate, context, new Sequence[]{item}).head()).getBooleanValue();
            if (condition) {
                iter.close();
                return BooleanValue.TRUE;
            }
        }
        return BooleanValue.FALSE;
    }


}


// Copyright (c) 2023 Saxonica Limited