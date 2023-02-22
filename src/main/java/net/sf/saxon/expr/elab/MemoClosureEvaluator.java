package net.sf.saxon.expr.elab;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Evaluators;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.MemoClosure;

public class MemoClosureEvaluator implements SequenceEvaluator {

    final Expression input;

    public MemoClosureEvaluator(Expression input) {
        // TODO: supply a PullEvaluator to the MemoClosure, not the original expression
        this.input = input;
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
            return new MemoClosure(input, context);
        } catch (UncheckedXPathException e) {
            throw e.getXPathException();
        }
    }

    /**
     * Get a code identifying the evaluator. Note that the codes are held in SEF files
     * so they should be stable.
     */
    @Override
    public int getEvaluationMethodCode() {
        return Evaluators.MAKE_MEMO_CLOSURE;
    }
}
