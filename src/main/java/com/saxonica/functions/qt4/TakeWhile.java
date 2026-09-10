package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Int64Value;

/**
 * This class supports the proposed QT4 function fn:take-while.
 * The function takes two arguments: the first is a sequence of items, the second is a
 * function used as a predicate to determine the first item to be excluded from the result.
 */

public class TakeWhile extends SystemFunction {


    

    @Override
    public String getStreamerName() {
        return "saxon:takeWhile";
    }



    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call.
     * @return the results of the function, as a sequence of arrays
     * @throws XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                        code will add information about the error location.
     */

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Sequence input = arguments[0];
        FunctionItem whileCondition = (FunctionItem) arguments[1].head();
        return new LazySequence(new WhileIterator(
                input.iterate(), whileCondition, context));
    }

    private static class WhileIterator implements SequenceIterator {

        private final SequenceIterator baseIterator;
        private final FunctionItem whileCondition;
        private final XPathContext context;
        private int position;
        private final int whileArity;


        WhileIterator(SequenceIterator in, FunctionItem whileCondition, XPathContext context) {
            this.baseIterator = in;
            this.whileCondition = whileCondition;
            this.context = context;
            this.position = 0;
            this.whileArity = whileCondition.getArity();
        }

        @Override
        public Item next() {
            if (position == -1) {
                return null;
            }
            Item item = baseIterator.next();
            if (item != null) {
                position++;
                try {
                    boolean matches;
                    switch (whileArity) {
                        case 0:
                            matches = ExpressionTool.effectiveBooleanValue(
                                    whileCondition.call(context, new Sequence[]{}).iterate());
                            break;
                        case 1:
                            matches = ExpressionTool.effectiveBooleanValue(
                                    whileCondition.call(context, new Sequence[]{item}).iterate());
                            break;
                        case 2:
                        default:
                            matches = ExpressionTool.effectiveBooleanValue(
                                    whileCondition.call(context, new Sequence[]{item, Int64Value.makeIntegerValue(position)}).iterate());
                            break;
                    }
                    if (matches) {
                        return item;
                    } else {
                        position = -1;
                        baseIterator.close();
                        return null;
                    }
                } catch (XPathException e) {
                    throw new UncheckedXPathException(e);
                }
            }
            position = -1;
            return null;
        }

        @Override
        public void close() {
            baseIterator.close();
        }

    }

}

// Copyright (c) 2018-2023 Saxonica Limited
