package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import net.sf.saxon.expr.Callable;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.EmptySequence;

/**
 * A CallableFeed is used to wrap an expression that does (a) not have intrinsic capability
 * to accept streamed input, and (b) does not expect any operand to be a sequence, and (c)
 * implements the Callable interface.
 */


public class CallableFeed extends ItemFeed {

    Callable function;
    Expression[] arguments;
    boolean done = false;

    /**
     * Create a Feed that evaluates a Callable (often but not necessarily a function)
     * @param function the function or other Callable
     * @param result the place to send the results of evaluating this Callable
     * @param context dynamic context information
     * @param arguments the arguments to the Callable. Typically one of these is Consuming. The
     *     expression receives its input for the consuming argument via the processItem() call,
     *     and it then evaluates the other (non-streamed) arguments by calling iterate() on them.
     */

    public CallableFeed(Callable function, ItemFeed result, XPathContext context, Expression[] arguments) {
        super(result, context);
        this.function = function;
        this.arguments = arguments;
    }

    /**
     * Supply an item to the feed
     *
     * @param item the item being fed
     * @throws net.sf.saxon.trans.XPathException
     *          to report any failure
     */

    @Override
    public void append(Item item) throws XPathException {
        if (!hasFailed()) {
            if (done) {
                String msg = "A sequence of more than one item must not be supplied as the streaming argument";
                if (function instanceof Function) {
                    StructuredQName fName = ((Function)function).getFunctionName();
                    if (fName != null) {
                        msg += " of function " + fName.getDisplayName();
                    }
                }
                Location loc = getExpression() == null ? Loc.NONE : getExpression().getLocation();
                throw new XPathException(msg, "XPTY0004", loc);
            }
            doCall(item);
            done = true;
        }
    }

    @Override
    public void close() throws XPathException {
        if (!hasFailed()) {
            if (!done) {
                doCall(null);
            }
            super.close();
        }
    }

    /**
     * Method called to process the supplied item from the consuming operand
     * @param item the supplied item from the consuming operand, or null if the consuming
     * operand evaluates to an empty sequence
     * @throws XPathException if a dynamic error occurs
     */

    private void doCall(Item item) throws XPathException {
        Sequence[] args = SequenceTool.makeSequenceArray(arguments.length);
        int i=0;
        for (Expression exp : arguments) {
            if (Streamability.getSweep(exp) == Sweep.CONSUMING) {
                args[i++] = item == null ? EmptySequence.getInstance() : item;
            } else {
                args[i++] = new LazySequence(exp.iterate(getContext()));
            }
        }
        Sequence result;
        try {
            result = function.call(getContext(), args);
        } catch (XPathException e) {
            dynamicError(e);
            return;
        }
        processItems(result.iterate(), getNextOutputter());
    }


}

// Copyright (c) 2018-2022 Saxonica Limited