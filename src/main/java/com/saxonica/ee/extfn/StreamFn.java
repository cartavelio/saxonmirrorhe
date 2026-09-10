package com.saxonica.ee.extfn;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.ContextItemStaticInfo;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.expr.parser.Optimizer;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;


/**
 * This class implements the saxon:stream() extension function.
 * <p>This is a pseudo-function which translates at compile time into a streaming copy instruction</p>
 */

public class StreamFn extends SystemFunction {

    /**
     * Make an expression that either calls this function, or that is equivalent to a call
     * on this function
     *
     * @param arguments the supplied arguments to the function call
     * @return either a function call on this function, or an expression that delivers
     * the same result
     */
    @Override
    public Expression makeFunctionCall(Expression... arguments) {
        return new SystemFunctionCall(this, arguments) {
            @Override
            public Expression optimize(/*@NotNull*/ ExpressionVisitor visitor, ContextItemStaticInfo contextItemType) throws XPathException {
                visitor.setOptimizeForStreaming(true);
                return super.optimize(visitor, contextItemType);
            }
        };
    }

    /**
     * Allow the function to create an optimized call based on the values of the actual arguments
     *
     * @param visitor     the expression visitor
     * @param contextInfo information about the context item
     * @param arguments   the supplied arguments to the function call. Note: modifying the contents
     *                    of this array should not be attempted, it is likely to have no effect.
     * @return either a function call on this function, or an expression that delivers
     * the same result, or null indicating that no optimization has taken place
     * @throws XPathException if an error is detected
     */

    @Override
    public Expression makeOptimizedFunctionCall(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo, Expression... arguments) throws XPathException {
        Optimizer opt = visitor.getConfiguration().obtainOptimizer();
        Expression result = opt.optimizeSaxonStreamFunction(visitor, contextInfo, arguments[0]);
        if (result == null) {
            if (visitor.getConfiguration().getBooleanProperty(Feature.STREAMING_FALLBACK)) {
                return arguments[0];
            } else {
                throw new XPathException("The argument to saxon:stream() has the wrong form for streaming to be possible", "XTSE3430");
            }
        }
        if (result.isCallOn(StreamFn.class)) {
            return ((SystemFunctionCall)result).getArg(0);
        }
        return result;
    }

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
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        throw new XPathException("The function saxon:stream() cannot be called dynamically");
    }


}

// Copyright (c) 2013-2022 Saxonica Limited
