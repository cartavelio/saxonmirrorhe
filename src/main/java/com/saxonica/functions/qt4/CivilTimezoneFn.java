package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.value.DateTimeValue;
import net.sf.saxon.value.DayTimeDurationValue;
import net.sf.saxon.value.StringValue;

import java.math.BigDecimal;
import java.util.Set;
import java.util.TimeZone;

/**
 * Implements the fn:civil-timezone() function.
 */

public class CivilTimezoneFn extends SystemFunction {

    /**
     * Evaluate this function call
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call. Each argument value (which is in general
     *                  a sequence) is supplied in the form of a sequence.
     * @return the results of the function.
     * @throws XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                                           code will add information about the error location.
     */

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        DateTimeValue time = (DateTimeValue)arguments[0].head();
        TimeZone tz = TimeZone.getDefault();
        if (arguments.length > 1) {
            StringValue placeVal = (StringValue) arguments[1].head();
            if (placeVal != null) {
                String place = placeVal.getStringValue();
                if (!isAvailableTimeZone(place)) {
                    throw new XPathException("Unknown timezone " + place, "FODT0004");
                }
                tz = TimeZone.getTimeZone(place);
            }
        }
        BigDecimal seconds = time.toDateTime().secondsSinceEpoch();
        int millisOffset = tz.getOffset(seconds.longValue() * 1000);
        return new DayTimeDurationValue(BigDecimal.valueOf(millisOffset/1000));
    }

    @CSharpReplaceBody(code="return Saxon.Ejava.util.TimeZone.isAvailableID(place);")
    private boolean isAvailableTimeZone(String place) {
        return Set.of(TimeZone.getAvailableIDs()).contains(place);
    }



}

// Copyright (c) 2020-2023 Saxonica Limited

