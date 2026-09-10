package net.sf.saxon.expr.elab;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trace.Instrumentation;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Closure;

/**
 * A <code>LearningSequenceEvaluator</code> initially performs
 * lazy evaluation of an expression; but if this proves unproductive,
 * it switches after a while to eager evaluation. Lazy evaluation is
 * considered unproductive if the input sequence is read to completion.
 * A MemoClosure reports back if the input sequence is read to completion,
 * and if the number of sequences that are completely read is equal to
 * the total number of evaluations, lazy evaluation is abandoned.
 */
public class LearningSequenceEvaluator implements SequenceEvaluator {

    private final static int EVAL_LIMIT = 20;

    private final Expression expression;
    private SequenceEvaluator evaluator;
    private int completed = 0;
    private int count = 0;
    private boolean flipped;

    public LearningSequenceEvaluator(Expression expr, SequenceEvaluator lazy) {
        Instrumentation.count("LearningEvaluator");
        this.expression = expr;
        this.evaluator = lazy;
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
        Instrumentation.count("LearningEvaluator.evaluate()");
        if (count > EVAL_LIMIT) {
            Instrumentation.count("LearningEvaluator.evaluate() " + (flipped ? "eager" : "lazy"));
            return evaluator.evaluate(context);
        } else {
            Sequence result = evaluator.evaluate(context);
            Instrumentation.count("LearningEvaluator.evaluate() uncommitted");
            if (result instanceof Closure) {
                Instrumentation.count("LearningEvaluator.evaluate() returned Closure");
                ((Closure)result).setLearningEvaluator(this, count);
            } else {
                flipped = true;
            }
            count++;
            return result;
        }
    }

    /**
     * Callback method called when a lazily-evaluated variable has been read to completion
     * @param serialNumber identifies the evaluation
     */

    public void reportCompletion(int serialNumber) {
        Instrumentation.count("LearningEvaluator.reportCompletion()");
        // Note, does thread-unsafe updates to the statistics
        // Note: three things might happen to a MemoClosure: it might be read to completion, it might
        // be partially read, and it might never be accessed at all. In the final case we will get no
        // feedback. The condition we want to test for is that of the first N MemoClosures created,
        // each one was read to completion
        //System.err.println("Completion " + expression.toShortString() + " " + serialNumber);
        if (completed++ >= EVAL_LIMIT && completed == count) {
            //System.err.println("Switch " + expression.toShortString() + " to eager evaluation");
            Instrumentation.count("LearningEvaluator.switchToEager()");
            evaluator = (expression.makeElaborator().eagerly());
            count = Integer.MAX_VALUE;
            flipped = true;
        }

    }

    /**
     * Get a code identifying the evaluator. Note that the codes are held in SEF files
     * so they should be stable.
     */
    @Override
    public int getEvaluationMethodCode() {
        return evaluator.getEvaluationMethodCode();
    }
}
