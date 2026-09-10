package com.saxonica.ee.elab;

import net.sf.saxon.expr.AndExpression;

public abstract class AndElaborator extends BooleanElaborator {

    public BooleanEvaluator getBooleanEvaluator() {
        AndExpression expr = (AndExpression) getExpression();
        BooleanEvaluator eval0 = Elaborator.make(expr.getLhsExpression()).getBooleanEvaluator();
        BooleanEvaluator eval1 = Elaborator.make(expr.getRhsExpression()).getBooleanEvaluator();
        return context -> eval0.eval(context) && eval1.eval(context);
    }


}

