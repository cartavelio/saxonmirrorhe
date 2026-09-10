package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;

/**
 * This class supports the two extension functions saxon:items-until and saxon:items-before,
 * also available as experimental XPath 4.0 functions in the fn namespace.
 * These functions take two arguments: the first is a sequence of items, the second is a
 * function used as a predicate to determine the last item to be included in the result.
 * The functions differ in whether the matching item is included in the result or not.
 */

public abstract class ItemsEnding extends SystemFunction {

    protected abstract boolean includeMatchingItem();

    public static class ItemsUntil extends ItemsEnding {
        @Override
        protected boolean includeMatchingItem() {
            return true;
        }

        @Override
        public String getStreamerName() {
            return "saxon:ItemsUntil";
        }
    }

    public static class ItemsBefore extends ItemsEnding {
        @Override
        protected boolean includeMatchingItem() {
            return false;
        }

        @Override
        public String getStreamerName() {
            return "saxon:ItemsBefore";
        }
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
        FunctionItem endCondition = (FunctionItem) arguments[1].head();
        return new LazySequence(new EndingIterator(
                input.iterate(), endCondition, includeMatchingItem(), context));
    }

    private static class EndingIterator implements SequenceIterator {

        private final SequenceIterator baseIterator;
        private final FunctionItem end;
        private final boolean inclusive;
        private final XPathContext context;
        private boolean finished;

        EndingIterator(SequenceIterator in, FunctionItem end, boolean inclusive, XPathContext context) {
            this.baseIterator = in;
            this.end = end;
            this.inclusive = inclusive;
            this.context = context;
        }

        @Override
        public Item next() {
            try {
                if (finished) {
                    return null;
                }
                Item item = baseIterator.next();
                if (item == null) {
                    finished = true;
                    return null;
                }
                BooleanValue matches = (BooleanValue)end.call(context, SequenceTool.fromItems(item)).head();
                if (matches.getBooleanValue()) {
                    finished = true;
                    if (inclusive) {
                        return item;
                    } else {
                        baseIterator.close();
                        return null;
                    }
                } else {
                    return item;
                }
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
