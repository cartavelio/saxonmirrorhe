package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.AbstractBlockIterator;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.LazySequence;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.NumericValue;

/**
 * The proposed XPath 4.0 replicate() function
 */


public class Replicate extends SystemFunction {

    /**
     * Evaluate the expression as a general function call
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        GroundedValue baseValue = arguments[0].materialize();
        NumericValue n = (NumericValue) arguments[1].head();
        int count = (int) n.longValue();

        if (count < 2) {
            if (count < 0) {
                throw new XPathException("Replication count is negative", "FORG0099");
            } else if (count == 0) {
                return EmptySequence.getInstance();
            } else {
                return baseValue;
            }
        } else {
            return new LazySequence(new RepeatingIterator(baseValue, count, context));
        }
    }

    public static class RepeatingIterator extends AbstractBlockIterator {

        private final GroundedValue baseValue;

        public RepeatingIterator(GroundedValue baseValue, int count, XPathContext context) {
            super(count, context);
            this.baseValue = baseValue;
        }

        @Override
        public SequenceIterator getNthChildIterator(int n) {
            return baseValue.iterate();
        }
    }
    
}

// Copyright (c) 2023 Saxonica Limited.

