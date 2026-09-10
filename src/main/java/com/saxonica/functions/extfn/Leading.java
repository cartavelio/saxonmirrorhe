package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;

/**
 * This class implements the saxon:leading() extension functions.
 * <p>The first argument is a sequence S, the second a function F (defaulting to fn:data).
 * The function returns the item or items I in the input sequence S up to and excluding the first
 * one for which F(I) returns false.</p>
 */

public class Leading extends SystemFunction {


        /**
         * Evaluate this function call at run-time
         *
         * @param context   The XPath dynamic evaluation context
         * @param arguments The values of the arguments to the function call. Each argument value (which is in general
         *                  a sequence) is supplied in the form of an iterator over the items in the sequence. If required, the
         *                  supplied sequence can be materialized by calling, for example, <code>new SequenceExtent(arguments[i])</code>.
         *                  If the argument is always a singleton, then the single item may be obtained by calling
         *                  <code>arguments[i].next()</code>. The implementation is not obliged to read all the items in each
         *                  <code>SequenceIterator</code> if they are not required to compute the result; but if any SequenceIterator is not read
         *                  to completion, it is good practice to call its close() method.
         * @return an iterator over the results of the function. If the result is a single item, it can be
         *         returned in the form of a {@link net.sf.saxon.tree.iter.SingletonIterator}. If the result is an empty sequence,
         *         the method should return <code>EmptyIterator.getInstance()</code>
         * @throws net.sf.saxon.trans.XPathException
         *          if a dynamic error occurs during evaluation of the function. The Saxon run-time
         *          code will add information about the error location.
         */

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            final XPathContext c2 = context.newCleanContext();
            final FunctionItem function = arguments.length == 2 ? (FunctionItem) arguments[1].head() : context.getConfiguration().makeSystemFunction("data", 1);
            return SequenceTool.toLazySequence(new LeadingIterator(arguments[0].iterate(), function, c2));
        }


    private static class LeadingIterator implements SequenceIterator {

        private final SequenceIterator base;
        private final FunctionItem function;
        private final XPathContext context;
        private boolean exhausted = false;
        private final Sequence[] args = new Sequence[1];

        public LeadingIterator(SequenceIterator base, FunctionItem function, XPathContext context) {
            this.base = base;
            this.function = function;
            this.context = context;
        }

        @Override
        public Item next() {
            if (exhausted) {
                return null;
            }
            Item next = base.next();
            if (next == null) {
                return null;
            }
            args[0] = next;
            try {
                boolean matches =
                        ExpressionTool.effectiveBooleanValue(dynamicCall(function, context, args).iterate());
                if (matches) {
                    return next;
                } else {
                    exhausted = true;
                    base.close();
                    return null;
                }
            } catch (XPathException e) {
                throw new UncheckedXPathException(e);
            }
        }

        @Override
        public void close() {
            base.close();
        }

    }


}

// Copyright (c) 2004-2023 Saxonica Limited