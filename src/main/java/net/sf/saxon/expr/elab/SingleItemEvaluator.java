package net.sf.saxon.expr.elab;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;

public class SingleItemEvaluator implements SequenceEvaluator {

    final ItemEvaluator evaluator;

    public SingleItemEvaluator(ItemEvaluator eval) {
        this.evaluator = eval;
    }

    /**
     * Evaluate a construct to produce a value (which might be a lazily evaluated Sequence)
     *
     * @param context the evaluation context
     * @return a Sequence (not necessarily grounded)
     * @throws XPathException if a dynamic error occurs during the evaluation.
     */
    @Override
    public Sequence evaluate(XPathContext context) throws XPathException {
        try {
            return evaluator.eval(context);
        } catch (UncheckedXPathException e) {
            throw e.getXPathException();
        }
    }

}
