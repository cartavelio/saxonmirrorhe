package com.saxonica.functions.extfn;

import net.sf.saxon.Controller;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XsltController;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.QNameValue;

import java.util.Map;

/**
 * This class implements the saxon:message-count() function which returns the value of message counters
 * at the moment of execution
 */

public class MessageCount extends SystemFunction {

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
     */

    @Override
    public IntegerValue call(XPathContext context, Sequence[] arguments) throws XPathException {
        Controller controller = context.getController();
        if (!(controller instanceof XsltController)) {
            return Int64Value.ZERO;
        }
        Map<StructuredQName, Integer> counters = ((XsltController)controller).getMessageCounters();
        QNameValue arg = (QNameValue)arguments[0].head();
        if (arg == null) {
            // return the count of all messages regardless of error code
            long total = 0;
            for (int n : counters.values()) {
                total += n;
            }
            return new Int64Value(total);
        } else {
            int result = counters.getOrDefault(arg.getStructuredQName(), 0);
            return new Int64Value(result);
        }
    }


}

// Copyright (c) 2018-2022 Saxonica Limited
