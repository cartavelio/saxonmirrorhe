package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import net.sf.saxon.expr.OrExpression;

/**
 * Elaborator for an "or" expression ({@code A or B})
 */

public class OrElaborator extends BooleanElaborator {

    public BooleanEvaluator elaborateForBoolean() {

        OrExpression expr = (OrExpression) getExpression();
        BooleanEvaluator eval0 = makeElaborator(expr.getLhsExpression()).elaborateForBoolean();
        BooleanEvaluator eval1 = makeElaborator(expr.getRhsExpression()).elaborateForBoolean();

        return context -> eval0.eval(context) || eval1.eval(context);
    }


}

