package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.ArithmeticExpression;
import net.sf.saxon.expr.Calculator;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;

/**
 * Elaborator for an ArithmeticExpression (for example P + Q)
 */

public class ArithmeticElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        final ArithmeticExpression exp = (ArithmeticExpression)getExpression();
        final ItemEvaluator arg0Eval = makeElaborator(exp.getLhsExpression()).elaborateForItem();
        final ItemEvaluator arg1Eval = makeElaborator(exp.getRhsExpression()).elaborateForItem();
        // The 'nullable' variables are designed to allow the null checks to be optimized away if not needed
        final boolean nullable0 = Cardinality.allowsZero(exp.getLhsExpression().getCardinality());
        final boolean nullable1 = Cardinality.allowsZero(exp.getRhsExpression().getCardinality());
        final Calculator calc = exp.getCalculator();
        return context -> {
            AtomicValue v0 = (AtomicValue) arg0Eval.eval(context);
            if (nullable0 && v0 == null) {
                return null;
            }
            AtomicValue v1 = (AtomicValue) arg1Eval.eval(context);
            if (nullable1 && v1 == null) {
                return null;
            }
            return calc.compute(v0, v1, context);
        };
    }

}

