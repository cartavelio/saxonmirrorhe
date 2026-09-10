package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.EmptyTextNodeRemover;
import net.sf.saxon.expr.ItemMappingIterator;

/**
 * Elaborator for an empty text node remove expression - inserted into a pipeline for node construction
 */

public class EmptyTextNodeRemoverElaborator extends PullElaborator {

    public PullEvaluator elaborateForPull() {

        final EmptyTextNodeRemover expr = (EmptyTextNodeRemover) getExpression();
        final PullEvaluator baseEval = makeElaborator(expr.getBaseExpression()).elaborateForPull();

        return context -> new ItemMappingIterator(baseEval.eval(context), expr);
    }

}

