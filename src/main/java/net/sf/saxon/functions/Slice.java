package net.sf.saxon.functions;

import net.sf.saxon.expr.SubsequenceIterator;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.IntegerRange;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.SequenceExtent;

import java.util.ArrayList;
import java.util.List;


/**
 * This class implements the saxon:slice() (proposed as fn:slice() for 4.0))
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

        if (arguments[1] instanceof IntegerRange) {
            // Special-case slice($input, N to M) to avoid materializing the result
            IntegerRange range = (IntegerRange)arguments[1];
            if (range.getStep() == 1L
                    && Math.abs(range.getStart()) < Integer.MAX_VALUE
                    && Math.abs(range.getEnd()) < Integer.MAX_VALUE) {
                int start = (int)range.getStart();
                if (start > 0) {
                    return new LazySequence(
                            SubsequenceIterator.make(arguments[0].iterate(),
                                                     start,
                                                     start - 1 + (int) range.getEnd()));
                }
            }
        }
        List<Item> selected = new ArrayList<>();
        GroundedValue input = arguments[0].materialize();
        int length = input.getLength();
        Item index;
        SequenceIterator indices = arguments[1].iterate();
        while ((index = indices.next()) != null) {
            IntegerValue value = (IntegerValue)index;
            int sign = value.signum();
            if (sign == 0) {
                continue;
            }
            int i = value.abs().asSubscript();
            if (i < 0 || i > length) {
                // indicates out of range for subscripting
                continue;
            }

            if (sign < 0) {
                selected.add(input.itemAt(length - i));
            } else {
                selected.add(input.itemAt(i-1));
            }
        }
        return SequenceExtent.makeSequenceExtent(selected);
    }

}

// Copyright (c) 2020-2022 Saxonica Limited

