package com.saxonica.ee.elab;

import net.sf.saxon.expr.OrExpression;

public abstract class OrElaborator extends BooleanElaborator {

    public BooleanEvaluator getBooleanEvaluator() {
        OrExpression expr = (OrExpression) getExpression();
        BooleanEvaluator eval0 = Elaborator.make(expr.getLhsExpression()).getBooleanEvaluator();
        BooleanEvaluator eval1 = Elaborator.make(expr.getRhsExpression()).getBooleanEvaluator();
        return context -> eval0.eval(context) || eval1.eval(context);
    }


}

