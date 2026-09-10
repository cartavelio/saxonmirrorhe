package com.saxonica.functions.qt4;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SubsequenceIterator;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.zeno.ZenoSequence;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.IntegerRange;
import net.sf.saxon.value.IntegerValue;

/**
 * Implements the XPath 4.0 items-at() function
 */

public class ItemsAt extends SystemFunction {

    /**
     * Get the static properties of this expression (other than its type). The result is
     * bit-significant. These properties are used for optimizations. In general, if
     * property bit is set, it is true, but if it is unset, the value is unknown.
     *
     * @param arguments the actual arguments to the function call
     */

    @Override
    public int getSpecialProperties(Expression[] arguments) {
        return arguments[0].getSpecialProperties();
    }


    /**
     * Call the function
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as Sequences.
     * @return the result of the function call
     * @throws XPathException if a dynamic error occurs
     */

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
         GroundedValue input = arguments[0].materialize();
         if (input.getLength() == 0) {
             return EmptySequence.getInstance();
         }
        if (arguments[1] instanceof IntegerRange) {
            // Special-case itemsAt($input, N to M) to avoid materializing the result
            IntegerRange range = (IntegerRange) arguments[1];
            if (range.getStep() == 1L
                    && Math.abs(range.getStart()) < Integer.MAX_VALUE
                    && Math.abs(range.getEnd()) < Integer.MAX_VALUE) {
                int start = (int) range.getStart();
                if (start > 0) {
                    return new LazySequence(
                            SubsequenceIterator.make(arguments[0].iterate(),
                                                     start,
                                                     (int) range.getEnd()));
                }
            }
        }
         ZenoSequence result = new ZenoSequence();
         SequenceIterator positions = arguments[1].iterate();
         for (Item it; (it = positions.next()) != null; ) {
              int offset = (int)((IntegerValue)it).longValue();
              Item selected = input.itemAt(offset - 1);
              if (selected != null) {
                  result = result.append(selected);
              }
         }
         return result;
    };

}

// Copyright (c) 2023 Saxonica Limited.