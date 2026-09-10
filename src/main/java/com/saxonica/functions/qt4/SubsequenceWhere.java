package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Int64Value;

/**
 * This class supports the proposed QT4 function fn:subsequence-where.
 * The function takes three arguments: the first is a sequence of items, the second is a
 * function used as a predicate to determine the first item to be included in the result,
 * the third is a function used as a predicate to determine the last item to be included in the result,
 */

public class SubsequenceWhere extends SystemFunction {


    

    @Override
    public String getStreamerName() {
        return "saxon:ItemsFrom";
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
        FunctionItem startCondition = (FunctionItem) arguments[1].head();
        FunctionItem endCondition = (FunctionItem) arguments[2].head();
        return new LazySequence(new BetweenIterator(
                input.iterate(), startCondition, endCondition, context));
    }

    private static class BetweenIterator implements SequenceIterator {

        private final SequenceIterator baseIterator;
        private final FunctionItem start;
        private final FunctionItem end;
        private final XPathContext context;
        private boolean started;
        private boolean ended;
        private int position;
        private final int startArity;
        private final int endArity;

        BetweenIterator(SequenceIterator in, FunctionItem start, FunctionItem end, XPathContext context) {
            this.baseIterator = in;
            this.start = start;
            this.end = end;
            this.context = context;
            this.position = 0;
            if (start == null) {
                started = true;
            }
            startArity = start == null ? 0 : start.getArity();
            endArity = end == null ? 0 : end.getArity();
        }

        @Override
        public Item next() {
            if (ended) {
                return null;
            }
            Item item;
            while ((item = baseIterator.next()) != null) {
                position++;
                if (started) {
                    if (end != null) {
                        ended = isMatchesEnd(item, position);
                    }
                    return item;
                }
                try {
                    BooleanValue matchesStart;
                    switch (startArity) {
                        case 0:
                            matchesStart = (BooleanValue) start.call(context, new Sequence[]{}).head();
                            break;
                        case 1:
                            matchesStart = (BooleanValue) start.call(context, new Sequence[]{item}).head();
                            break;
                        case 2:
                        default:
                            matchesStart = (BooleanValue) start.call(context, new Sequence[]{item, new Int64Value(position)}).head();
                            break;
                    }
                    if (matchesStart != null && matchesStart.getBooleanValue()) {
                        started = true;
                        if (end != null) {
                            ended = isMatchesEnd(item, position);
                        }
                        return item;
                    }
                } catch (XPathException e) {
                    throw new UncheckedXPathException(e);
                }
            }
            return null;
        }

        private boolean isMatchesEnd(Item item, int position) {
            try {
                BooleanValue matchesEnd;
                switch (endArity) {
                    case 0:
                        matchesEnd = (BooleanValue) end.call(context, new Sequence[]{}).head();
                        break;
                    case 1:
                        matchesEnd = (BooleanValue) end.call(context, new Sequence[]{item}).head();
                        break;
                    case 2:
                    default:
                        matchesEnd = (BooleanValue) end.call(context, new Sequence[]{item, new Int64Value(position)}).head();
                        break;
                }
                return matchesEnd != null && matchesEnd.getBooleanValue();
            } catch (XPathException e) {
                throw new UncheckedXPathException(e);
            }
        }

        @Override
        public void close() {
            baseIterator.close();
        }

    }

}

// Copyright (c) 2018-2023 Saxonica Limited
