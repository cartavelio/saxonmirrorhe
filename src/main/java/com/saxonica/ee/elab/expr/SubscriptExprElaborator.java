package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SubscriptExpression;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.value.NumericValue;

/**
 * An elaborator for a "subscript" expression, typically written as {@code X[$n]} where {@code $n} is an
 * integer or a numeric expression.
 */

public class SubscriptExprElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        final SubscriptExpression expr = (SubscriptExpression) getExpression();
        final PullEvaluator baseEval = makeElaborator(expr.getBaseExpression()).elaborateForPull();
        final ItemEvaluator indexEval = makeElaborator(expr.getSubscriptExpression()).elaborateForItem();
        return context -> {
            NumericValue index = (NumericValue) indexEval.eval(context);
            if (index == null) {
                return null;
            }
            int intIndex = index.asSubscript();
            if (intIndex != -1) {
                SequenceIterator iter = baseEval.eval(context);
                return SubscriptExpression.getItemAt(iter, intIndex);
            } else {
                // there is no item at the required position
                return null;
            }
        };
    }

}

