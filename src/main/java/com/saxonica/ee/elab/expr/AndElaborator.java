package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import net.sf.saxon.expr.AndExpression;

/**
 * Elaborator for an AndExpression (P and Q)
 */
public class AndElaborator extends BooleanElaborator {

    public BooleanEvaluator elaborateForBoolean() {
        AndExpression expr = (AndExpression) getExpression();
        BooleanEvaluator eval0 = makeElaborator(expr.getLhsExpression()).elaborateForBoolean();
        BooleanEvaluator eval1 = makeElaborator(expr.getRhsExpression()).elaborateForBoolean();
        return context -> eval0.eval(context) && eval1.eval(context);
    }


}

