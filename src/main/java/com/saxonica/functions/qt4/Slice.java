package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.Reverse;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.SequenceExtent;

import java.util.ArrayList;
import java.util.List;


/**
 * This class implements the fn:slice() function in 4.0. Implementation based on SaxonSlice, but
 * the functionality has diverged
 */

public class Slice extends SystemFunction {

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

        GroundedValue input = arguments[0].materialize();
        int length = input.getLength();
        if (length == 0) {
            return input;
        }
        IntegerValue start = (IntegerValue) arguments[1].head();
        IntegerValue end = (IntegerValue) arguments[2].head();
        IntegerValue step = (IntegerValue) arguments[3].head();

        int s;
        if (start == null) {
            s = 1;
        } else {
            int startVal = (int) start.longValue();
            if (startVal == 0) {
                s = 1;
            } else if (startVal < 0) {
                s = length + startVal + 1;
            } else {
                s = startVal;
            }
        }

        int e;
        if (end == null) {
            e = length;
        } else {
            int endVal = (int) end.longValue();
            if (endVal == 0) {
                e = length;
            } else if (endVal < 0) {
                e = length + endVal + 1;
            } else {
                e = endVal;
            }
        }

        int st = 1;
        int stepVal = step == null ? 0 : (int) step.longValue();
        if (stepVal == 0) {
            st = e >= s ? +1 : -1;
        } else {
            st = stepVal;
        }

        if (st < 0) {
            SequenceExtent reversed = SequenceExtent.from(Reverse.getReverseIterator(input.iterate()));
            return positiveSlice(reversed, length-s+1, length-e+1, -st);
        } else {
            return positiveSlice(input, s, e, st);
        }
    }

    private Sequence positiveSlice(GroundedValue input, int start, int end, int step) {
        List<Item> result = new ArrayList<>(input.getLength());
        for (int i=0; i<input.getLength(); i++) {
            int pos = i+1;
            if (pos >= start && pos <= end && (pos - start) % step == 0) {
                result.add(input.itemAt(i));
            }
        }
        return SequenceExtent.makeSequenceExtent(result);

    }

}

// Copyright (c) 2020-2023 Saxonica Limited

