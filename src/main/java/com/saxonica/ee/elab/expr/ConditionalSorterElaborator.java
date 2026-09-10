package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.ContextMappingIterator;
import net.sf.saxon.expr.sort.ConditionalSorter;

/**
 * Elaborator for a conditional sorter, which sorts the results of a subexpression into document
 * order only if a supplied condition is true
 */

public class ConditionalSorterElaborator extends PullElaborator {

    public PullEvaluator elaborateForPull() {
        final ConditionalSorter expr = (ConditionalSorter) getExpression();
        final BooleanEvaluator condition = makeElaborator(expr.getCondition()).elaborateForBoolean();
        final PullEvaluator sorter = makeElaborator(expr.getDocumentSorter()).elaborateForPull();
        final PullEvaluator nonSorter = makeElaborator(expr.getDocumentSorter().getBaseExpression()).elaborateForPull();
        return context -> {
            boolean b = condition.eval(context);
            if (b) {
                return sorter.eval(context);
            } else {
                return nonSorter.eval(context);
            }
        };
    }

}

