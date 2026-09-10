package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import com.saxonica.ee.elab.gen.Elaborator;
import net.sf.saxon.expr.AndExpression;

public class AndElaborator extends BooleanElaborator {

    public BooleanEvaluator getBooleanEvaluator() {
        AndExpression expr = (AndExpression) getExpression();
        BooleanEvaluator eval0 = Elaborator.make(expr.getLhsExpression()).getBooleanEvaluator();
        BooleanEvaluator eval1 = Elaborator.make(expr.getRhsExpression()).getBooleanEvaluator();
        return context -> eval0.eval(context) && eval1.eval(context);
    }


}

