package net.sf.saxon.expr.elab;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;

@FunctionalInterface
public interface SequenceEvaluator {

    /**
     * Evaluate a construct to produce a value (which might be a lazily evaluated Sequence)
     * @param context the evaluation context
     * @return a Sequence (not necessarily grounded)
     * @throws XPathException if a dynamic error occurs during the evaluation.
     */

    Sequence evaluate(XPathContext context) throws XPathException;

}

