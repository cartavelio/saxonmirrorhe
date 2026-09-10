package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.value.BigDecimalValue;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.NumericValue;

import java.math.RoundingMode;

/**
 * Implements the saxon:decimal-divide() function
 */

public class DecimalDivide extends SystemFunction {

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
        NumericValue d1 = (NumericValue)arguments[0].head();
        NumericValue d2 = (NumericValue)arguments[1].head();
        if (d1 == null || d2 == null) {
            return EmptySequence.getInstance();
        }
        int scale = (int) ((IntegerValue)arguments[2].head()).longValue();
        return divide(d1, d2, scale);

    }

    @CSharpReplaceBody(code="return new Saxon.Hej.value.BigDecimalValue(Singulink.Numerics.BigDecimal.Divide(d1.getDecimalValue(), d2.getDecimalValue(), scale < 18 ? 18 :scale, Singulink.Numerics.RoundingMode.ToZero)).round(scale);")
    private NumericValue divide(NumericValue d1, NumericValue d2, int scale) throws ValidationException {
        return new BigDecimalValue(
                d1.getDecimalValue().divide(d2.getDecimalValue(), scale, RoundingMode.DOWN));
    }


}

// Copyright (c) 2018-2023 Saxonica Limited

