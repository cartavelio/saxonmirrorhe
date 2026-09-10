package com.saxonica.functions.hof;

import net.sf.saxon.expr.Callable;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ManualIterator;

/**
 * A Callable that wraps another Callable and a Dynamic Context, in effect acting as a closure that
 * executes the original callable with a saved context.
 */
public class CallableWithBoundFocus implements Callable {

    private Callable target;
    private XPathContext boundContext;

    public CallableWithBoundFocus(Callable target, final XPathContext context) {
        this.target = target;
        boundContext = context.newContext();
        if (context.getCurrentIterator() == null) {
            boundContext.setCurrentIterator(null);
        } else {
            ManualIterator<?> iter =
                    new ManualIterator<>(context.getContextItem(), context.getCurrentIterator().position());
            iter.setLastPositionFinder(context::getLast);
            boundContext.setCurrentIterator(iter);
        }
    }

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
        return target.call(boundContext, arguments);
    }
}

// Copyright (c) 2018 Saxonica Limited.