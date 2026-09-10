package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.AdjacentTextNodeMerger;
import net.sf.saxon.tree.iter.AdjacentTextNodeMergingIterator;

/**
 * Elaborator for an adjacent text node merging expression - inserted into a pipeline for node construction
 */

public class AdjacentTextNodeMergerElaborator extends PullElaborator {

    public PullEvaluator elaborateForPull() {

        final AdjacentTextNodeMerger expr = (AdjacentTextNodeMerger) getExpression();
        final PullEvaluator baseEval = makeElaborator(expr.getBaseExpression()).elaborateForPull();

        return context -> new AdjacentTextNodeMergingIterator(baseEval.eval(context));
    }

}

