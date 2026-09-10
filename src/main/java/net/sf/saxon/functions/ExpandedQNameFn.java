package net.sf.saxon.functions;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.QNameValue;
import net.sf.saxon.value.StringValue;

/**
 * Implements the fn:expanded-QName() function proposed for 4.0
 */

public class ExpandedQNameFn extends SystemFunction {

    /**
     * Evaluate this function call
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call. Each argument value (which is in general
     *                  a sequence) is supplied in the form of a sequence.
     * @return the results of the function.
     * @throws XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                                           code will add information about the error location.
     */

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        QNameValue s = (QNameValue)arguments[0].head();
        if (s == null) {
            return EmptySequence.getInstance();
        }
        return new StringValue(s.getStructuredQName().getEQName());
    }


}

// Copyright (c) 2020-2022 Saxonica Limited

