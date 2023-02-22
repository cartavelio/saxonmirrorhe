package net.sf.saxon.expr.elab;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Evaluators;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;

public class LiteralEvaluator implements SequenceEvaluator {

    final GroundedValue value;

    public LiteralEvaluator(GroundedValue value) {
        this.value = value;
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
        return value;
    }

    /**
     * Get a code identifying the evaluator. Note that the codes are held in SEF files
     * so they should be stable.
     */
    @Override
    public int getEvaluationMethodCode() {
        return Evaluators.EVALUATE_LITERAL;
    }
}
