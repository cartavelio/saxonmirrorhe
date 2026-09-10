package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;


/**
 * This class implements the saxon:array-member() extension function. It wraps the supplied argument (which
 * can be any sequence) as an ArrayMemberValue. An ArrayMemberValue is an external object
 * that contains an arbitrary sequence; its significance is that when it appears in the input to an array constructor,
 * the sequence becomes a single member of the array rather than being flattened. As such it provides an intermediate
 * object that is invaluable during the construction of nested arrays.
 */

public class ArrayMember extends SystemFunction {

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
     * @throws XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                                           code will add information about the error location.
     */

    @Override
    public ArrayMemberValue call(XPathContext context, Sequence[] arguments) throws XPathException {
        return new ArrayMemberValue(arguments[0].materialize());
    }
}


// Copyright (c) 2018-2020 Saxonica Limited
