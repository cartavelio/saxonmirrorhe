package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.sort.DocumentOrderIterator;
import net.sf.saxon.expr.sort.DocumentSorter;
import net.sf.saxon.om.NodeInfo;

import java.util.Comparator;

/**
 * Elaborator for a docOrder expression - sorts nodes into document order and eliminates duplicates
 */

public class DocumentSorterElaborator extends PullElaborator {

    public PullEvaluator elaborateForPull() {

        final DocumentSorter expr = (DocumentSorter) getExpression();
        final PullEvaluator baseEval = makeElaborator(expr.getBaseExpression()).elaborateForPull();
        final Comparator<? super NodeInfo> comparer = expr.getComparer();

        return context -> new DocumentOrderIterator(baseEval.eval(context), comparer);
    }

}

