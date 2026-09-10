package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.RecordTest;
import net.sf.saxon.ma.map.Shape;
import net.sf.saxon.ma.map.ShapedMap;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.str.Twine8;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.value.*;

import java.math.BigDecimal;
import java.math.RoundingMode;

/**
 * Implements the proposed QT4 divide-decimals function
 */

public class DivideDecimals extends SystemFunction {

    public static RecordTest QUOTIENT_AND_REMAINDER_RECORD = RecordTest.nonExtensible(
            new RecordTest.Field("quotient", SequenceType.SINGLE_DECIMAL, false),
            new RecordTest.Field("scheme", SequenceType.SINGLE_DECIMAL, false)
    );

    /**
     * Evaluate this function call at run-time
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
        DecimalValue d1 = (DecimalValue)arguments[0].head();
        DecimalValue d2 = (DecimalValue)arguments[1].head();
        if (d1 == null || d2 == null) {
            return EmptySequence.getInstance();
        }
        int precision = 0;
        if (arguments.length > 2) {
            IntegerValue precisionArg = (IntegerValue) arguments[2].head();
            if (precisionArg != null) {
                precision = (int)precisionArg.longValue();
            }
        }
        return divide(d1, d2, precision);

    }

    @CSharpReplaceBody(code="return Saxon.Impl.Helpers.BigDecimalUtils.DivideDecimals(d1, d2, scale);")
    private MapItem divide(DecimalValue d1, DecimalValue d2, int scale) throws XPathException {
        try {
            if (scale == 0) {
                BigDecimal[] pair = d1.getDecimalValue().divideAndRemainder(d2.getDecimalValue());
                return new ShapedMap(quotientAndRemainder, new BigDecimalValue(pair[0]), new BigDecimalValue(pair[1]));
            }
            BigDecimal quotient = d1.getDecimalValue().divide(d2.getDecimalValue(), scale, RoundingMode.DOWN);
            BigDecimal remainder = d1.getDecimalValue().subtract(quotient.multiply(d2.getDecimalValue()));
            return new ShapedMap(quotientAndRemainder, new BigDecimalValue(quotient), new BigDecimalValue(remainder));
        } catch (ArithmeticException e) {
            throw new XPathException("Division by zero", "FOAR0001");
        }
    }

    public static Shape quotientAndRemainder = new Shape(new Twine8("quotient"), new Twine8("remainder"));


}

// Copyright (c) 2018-2023 Saxonica Limited

