package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.DateTimeValue;
import net.sf.saxon.value.DayTimeDurationValue;
import net.sf.saxon.value.IntegerValue;

/**
 * This class implements the function fn:unix-dateTime(), which is proposed as a standard function in XPath 4.0
 */

public class UnixDateTime extends SystemFunction {

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
        long input = 0;
        if (arguments.length > 0) {
            IntegerValue inVal = (IntegerValue) arguments[0].head();
            if (inVal != null) {
                input = inVal.longValue();
            }
        }
        return DateTimeValue.EPOCH
                    .add(DayTimeDurationValue.fromMilliseconds(input))
                    .adjustToUTC(0)
                    .withMetadata(BuiltInAtomicType.DATE_TIME_STAMP);
    }
    
}

// Copyright (c) 2023 Saxonica Limited.
