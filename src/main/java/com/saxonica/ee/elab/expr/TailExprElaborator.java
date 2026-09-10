package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.ContextMappingIterator;
import net.sf.saxon.expr.TailExpression;
import net.sf.saxon.expr.TailIterator;
import net.sf.saxon.om.SequenceIterator;

/**
 * Elaborator for a tail expression
 */

public class TailExprElaborator extends PullElaborator {

    public PullEvaluator elaborateForPull() {
        final TailExpression expr = (TailExpression) getExpression();
        final PullEvaluator baseEval = makeElaborator(expr.getBaseExpression()).elaborateForPull();
        final int start = expr.getStart();

        return context -> {
            SequenceIterator baseIter = baseEval.eval(context);
            return TailIterator.make(baseIter, start);
        };

    }

}

