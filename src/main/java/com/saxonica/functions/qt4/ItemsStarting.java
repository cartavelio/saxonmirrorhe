package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;

/**
 * This class supports the two extension functions saxon:items-from and saxon:items-after,
 * also available as experimental XPath 4.0 functions in the fn namespace.
 * These functions take two arguments: the first is a sequence of items, the second is a
 * function used as a predicate to determine the first item to be included in the result.
 * The functions differ in whether the matching item is included in the result or not.
 */

public abstract class ItemsStarting extends SystemFunction {

    protected abstract boolean includeMatchingItem();

    public static class ItemsFrom extends ItemsStarting {
        @Override
        protected boolean includeMatchingItem() {
            return true;
        }

        @Override
        public String getStreamerName() {
            return "saxon:ItemsFrom";
        }
    }

    public static class ItemsAfter extends ItemsStarting {
        @Override
        protected boolean includeMatchingItem() {
            return false;
        }

        @Override
        public String getStreamerName() {
            return "saxon:ItemsAfter";
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
        FunctionItem startCondition = (FunctionItem) arguments[1].head();
        return new LazySequence(new StartingIterator(
                input.iterate(), startCondition, includeMatchingItem(), context));
    }

    private static class StartingIterator implements SequenceIterator {

        private final SequenceIterator baseIterator;
        private final FunctionItem start;
        private final boolean inclusive;
        private final XPathContext context;
        private boolean started;

        StartingIterator(SequenceIterator in, FunctionItem start, boolean inclusive, XPathContext context) {
            this.baseIterator = in;
            this.start = start;
            this.inclusive = inclusive;
            this.context = context;
        }

        @Override
        public Item next() {
            Item item;
            while ((item = baseIterator.next()) != null) {
                if (started) {
                    return item;
                }
                try {
                    BooleanValue matches = (BooleanValue) start.call(context, SequenceTool.fromItems(item)).head();
                    if (matches.getBooleanValue()) {
                        started = true;
                        if (inclusive) {
                            return item;
                        }
                    }
                } catch (XPathException e) {
                    throw new UncheckedXPathException(e);
                }
            }
            return null;
        }

        @Override
        public void close() {
            baseIterator.close();
        }

    }

}

// Copyright (c) 2018-2023 Saxonica Limited
