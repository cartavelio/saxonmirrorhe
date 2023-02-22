package net.sf.saxon.expr.elab;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.trans.XPathException;

@FunctionalInterface
public interface PushEvaluator {

    /**
     * Evaluate a construct in push mode, sending the value of the construct
     * to a supplied outputter
     * @param out the outputter to which the result should be sent
     * @param context the evaluation context
     * @return either a {@link TailCall} or null. A {@link TailCall} represents unfinished
     * work that must be completed by the caller. Specifically, if a non-null value is returned,
     * the caller must either complete the evaluation by evaluating tail calls until null is
     * returned (which can be conveniently achieved by calling {@link Expression#dispatchTailCall(TailCall)},
     * or it must return the {@code TailCall to its own caller}
     * @throws XPathException if a dynamic error occurs during the evaluation.
     */

    TailCall processLeavingTail(Outputter out, XPathContext context) throws XPathException;

}

