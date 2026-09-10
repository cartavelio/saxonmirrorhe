package com.saxonica.ee.elab;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.ArithmeticExpression;
import net.sf.saxon.expr.Calculator;
import net.sf.saxon.value.AtomicValue;

public class ArithmeticElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        final ArithmeticExpression exp = (ArithmeticExpression)getExpression();
        final ItemEvaluator arg1Eval = Elaborator.make(exp.getLhsExpression()).getItemEvaluator();
        final ItemEvaluator arg2Eval = Elaborator.make(exp.getRhsExpression()).getItemEvaluator();
        final Calculator calc = exp.getCalculator();
        return context -> {
            AtomicValue v1 = (AtomicValue) arg1Eval.eval(context);
            AtomicValue v2 = (AtomicValue) arg2Eval.eval(context);
            return calc.compute(v1, v2, context);
        };
    }

}

