package com.saxonica.functions.hof;

import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.expr.ItemMappingIterator;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;

/**
 * This class implements the function fn:filter(), which is a standard function in XQuery 3.0
 */

public class FilterFn extends SystemFunction {

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws net.sf.saxon.trans.XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    public Sequence<?> call(XPathContext context, Sequence[] arguments) throws XPathException {
        return SequenceTool.toLazySequence(
                evalFilter((Function) arguments[1].head(),
                           ((Sequence<Item<?>>)arguments[0]).iterate(), context));
    }

    private SequenceIterator<?> evalFilter(
            final Function function, SequenceIterator<Item<?>> base, final XPathContext context) {
        ItemMappingFunction<Item<?>, Item<?>> map = new ItemMappingFunction<Item<?>, Item<?>>() {
            private final Sequence[] args = new Sequence[1];

            public Item<?> mapItem(Item item) throws XPathException {
                args[0] = item;
                BooleanValue result = (BooleanValue) dynamicCall(function, context, args).head();
                return result.getBooleanValue() ? item : null;
            }
        };
        return new ItemMappingIterator<>(base, map);
    }

    public String getStreamerName() {
        return "FilterFn";
    }
}


// Copyright (c) 2018 Saxonica Limited.