package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.sort.SortExpression;

/**
 * Elaborator for a sort expression - sorts nodes into order based on a user-supplied sort key
 */

public class SortExprElaborator extends PullElaborator {

    public PullEvaluator elaborateForPull() {

        // TODO: elaborate the sort key expression, and other expressions in the sort key definition

        final SortExpression expr = (SortExpression) getExpression();
        final PullEvaluator baseEval = makeElaborator(expr.getBaseExpression()).elaborateForPull();

        return context -> expr.iterateSorted(baseEval.eval(context), context);
    }

}

