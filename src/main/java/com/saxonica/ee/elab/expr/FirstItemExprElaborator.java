package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.FirstItemExpression;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;

/**
 * An elaborator for a "first-item" expression, typically written as {@code X[1]} or as {@code head(X)}.
 */

public class FirstItemExprElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        final FirstItemExpression expr = (FirstItemExpression) getExpression();
        final PullEvaluator baseEval = makeElaborator(expr.getBaseExpression()).elaborateForPull();
        return context -> {
            final SequenceIterator iter = baseEval.eval(context);
            Item result = iter.next();
            iter.close();
            return result;
        };
    }

}

