////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.AbstractBlockIterator;
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

    private static class RepeatingIterator extends AbstractBlockIterator {

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

