package com.saxonica.functions.qt4;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.IntegerValue;



/**
 * The saxon:index-where() or fn:index-where extension function
 */


public class IndexWhere extends SystemFunction {

    /**
     * For an expression that returns an integer or a sequence of integers, get
     * a lower and upper bound on the values of the integers that may be returned, from
     * static analysis. The default implementation returns null, meaning "unknown" or
     * "not applicable". Other implementations return an array of two IntegerValue objects,
     * representing the lower and upper bounds respectively. The values
     * UNBOUNDED_LOWER and UNBOUNDED_UPPER are used by convention to indicate that
     * the value may be arbitrarily large. The values MAX_STRING_LENGTH and MAX_SEQUENCE_LENGTH
     * are used to indicate values limited by the size of a string or the size of a sequence.
     *
     * @return the lower and upper bounds of integer values in the result, or null to indicate
     *         unknown or not applicable.
     */
    @Override
    public IntegerValue[] getIntegerBounds() {
        return new IntegerValue[]{Int64Value.PLUS_ONE, Expression.MAX_SEQUENCE_LENGTH};
    }

    private static class IndexIterator implements SequenceIterator {
        private int index = 0;
        private final SequenceIterator base;
        private final FunctionItem predicate;
        private final XPathContext context;
        private final int arity;

        public IndexIterator(SequenceIterator base, XPathContext context, FunctionItem predicate) {
            this.base = base;
            this.context = context;
            this.predicate = predicate;
            this.arity = predicate.getArity();
        }

        /**
         * Close the iterator.
         * @since 9.1
         */
        @Override
        public void close() {
            base.close();
        }

        /**
         * Get the next item in the sequence.
         */
        @Override
        public Int64Value next() {
            Item baseItem;
            while ((baseItem = base.next()) != null) {
                index++;
                try {
                    Int64Value ix = new Int64Value(index);
                    Sequence result;
                    switch (arity) {
                        case 0:
                            result = dynamicCall(predicate, context);
                            break;
                        case 1:
                            result = dynamicCall(predicate, context, baseItem);
                            break;
                        case 2:
                            result = dynamicCall(predicate, context, baseItem, ix);
                            break;
                        default:
                            result = EmptySequence.getInstance();
                    }
                    BooleanValue val = (BooleanValue)result.head();
                    if (val != null && val.getBooleanValue()) {
                        return ix;
                    }
                } catch (XPathException e) {
                    throw new UncheckedXPathException(e);
                }
            }
            return null;
        }

    }

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        SequenceIterator seq = arguments[0].iterate();
        FunctionItem predicate = (FunctionItem) arguments[1].head();
        SequenceIterator iter = new IndexIterator(seq, context, predicate);
        return new LazySequence(iter);
    }

}

// Copyright (c) 2020-2023 Saxonica Limited

