package com.saxonica.ee.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.QNameValue;


/**
 * This class implements the saxon:schema() extension function.
 * <p>There are two forms:
 * (a) saxon:schema() returns a function that can be called to get top-level schema information
 * (b) saxon:schema(component-kind, name) returns a function that can be called to get information about
 * a specific named schema component</p>
 */

public class SchemaFn extends SystemFunction {


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
     * returned in the form of a {@link net.sf.saxon.tree.iter.SingletonIterator}. If the result is an empty sequence,
     * the method should return <code>EmptyIterator.getInstance()</code>
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                                           code will add information about the error location.
     */

        /*@Nullable*/
    public Sequence<?> call(XPathContext context, Sequence[] arguments) throws XPathException {
        if (arguments.length == 0) {
            return context.getConfiguration().getSchemaAsFunctionItem();
        } else if (arguments.length == 2) {
            Function f = context.getConfiguration().getSchemaComponentAsFunctionItem(
                    arguments[0].head().getStringValue(),
                    (QNameValue) arguments[1].head()
            );
            return f == null ? EmptySequence.getInstance() : f;
        } else {
            throw new XPathException("Call to saxon:schema() requires either 0 or 2 arguments");
        }
    }


}

// Copyright (c) 2013-2016 Saxonica Limited.
