package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.NegateExpression;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.DoubleValue;
import net.sf.saxon.value.NumericValue;

/**
 * Elaborator for a negate expression (that is, unary minus)
 */

public class NegateElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {

        final NegateExpression exp = (NegateExpression)getExpression();
        final ItemEvaluator argEval = makeElaborator(exp.getBaseExpression()).elaborateForItem();
        final boolean maybeEmpty = Cardinality.allowsZero(exp.getBaseExpression().getCardinality());
        final boolean backwardsCompatible = exp.isBackwardsCompatible();

        if (maybeEmpty) {
            if (backwardsCompatible) {
                return context -> {
                    NumericValue v1 = (NumericValue) argEval.eval(context);
                    return v1 == null ? DoubleValue.NaN : v1.negate();
                };
            } else {
                return context -> {
                    NumericValue v1 = (NumericValue) argEval.eval(context);
                    return v1 == null ? null : v1.negate();
                };
            }
        } else {
            return context -> ((NumericValue) argEval.eval(context)).negate();
        }
    }

}

