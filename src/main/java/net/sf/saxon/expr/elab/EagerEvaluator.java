package net.sf.saxon.expr.elab;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.trans.XPathException;

@FunctionalInterface
public interface EagerEvaluator extends SequenceEvaluator {

    /**
     * Evaluate a construct to produce a grounded value
     * @param context the evaluation context
     * @return a grounded value
     * @throws XPathException if a dynamic error occurs during the evaluation.
     */

    GroundedValue evaluate(XPathContext context) throws XPathException;

}

