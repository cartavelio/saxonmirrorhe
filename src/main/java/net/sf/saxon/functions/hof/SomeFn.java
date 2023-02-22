////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions.hof;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;

/**
 * This class implements the function fn:some(), which is a proposed function for XPath 4.0
 */

public class SomeFn extends SystemFunction {

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
        SequenceIterator iter = arguments[0].iterate();
        Function predicate = (Function) arguments[1].head();
        for (Item item; (item = iter.next()) != null; ) {
            boolean condition = ((BooleanValue) dynamicCall(predicate, context, new Sequence[]{item}).head()).getBooleanValue();
            if (condition) {
                return BooleanValue.TRUE;
            }
        }
        return BooleanValue.FALSE;
    }


}


// Copyright (c) 2018-2022 Saxonica Limited