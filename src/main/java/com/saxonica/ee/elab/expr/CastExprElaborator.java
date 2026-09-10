package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.value.AtomicValue;

/**
 * Elaborator for {@code cast as} expression, or the equivalent constructor function call
 */

public class CastExprElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        CastExpression exp = (CastExpression) getExpression();
        Expression arg = exp.getBaseExpression();
        ItemEvaluator argEval = Elaborator.makeElaborator(arg).elaborateForItem();

        return context -> {
            AtomicValue value = (AtomicValue) argEval.eval(context);
            return exp.doCast(value, context);
        };
    }

}

