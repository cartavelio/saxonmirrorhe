package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.functions.Translate;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.str.Twine8;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.StringValue;

import java.math.BigInteger;

/**
 * Implements the 4.0 parse-integer() function.
 */

public class ParseInteger extends SystemFunction {

    public static StringValue ignorables = new StringValue(new Twine8(new byte[]{9, 10, 13, 32, '_'}));

    /**
     * Evaluate this function call
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call. Each argument value (which is in general
     *                  a sequence) is supplied in the form of a sequence.
     * @return the results of the function.
     * @throws XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                        code will add information about the error location.
     */

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        StringValue s = (StringValue) arguments[0].head();
        if (s == null) {
            return EmptySequence.getInstance();
        }
        int radix = 10;
        if (arguments.length == 2) {
            IntegerValue r = (IntegerValue) arguments[1].head();
            if (r != null) {
                if (r.compareTo(2) < 0 || r.compareTo(36) > 0) {
                    throw new XPathException("Radix in parse-integer must be in range 2..36", "FORG0011");
                }
                radix = (int) r.longValue();
            }
        }
        String val = Translate.translate(s, ignorables, StringValue.EMPTY_STRING).getStringValue();
        try {
            BigInteger x = new BigInteger(val, radix);
            return IntegerValue.makeIntegerValue(x);
        } catch (NumberFormatException e) {
            throw new XPathException("Cannot parse integer '" + s.getStringValue() + "'", "FORG0012");
        }
    }


}

// Copyright (c) 2020-2022 Saxonica Limited

