package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.value.DateTimeValue;

/**
 * This class implements the saxon:timestamp() function which returns the date/time at the moment
 * of execution
 */

public class TimestampFn extends SystemFunction {

    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context. Not used in this case
     * @param arguments The values of the arguments to the function call. In this case there are none.
     * @return the result of the function. This will have a timezone, specifically, the platform
     * default timezone. The returned value will be an instance of the built-in
     * subtype {@code xs:dateTimeStamp}
     */

    @Override
    public DateTimeValue call(XPathContext context, Sequence[] arguments) {
        return DateTimeValue.now();
    }


}

// Copyright (c) 2018-2022 Saxonica Limited
