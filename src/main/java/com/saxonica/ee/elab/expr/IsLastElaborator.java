package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import net.sf.saxon.expr.IsLastExpression;

public class IsLastElaborator extends BooleanElaborator {

    public BooleanEvaluator getBooleanEvaluator() {
        IsLastExpression expr = (IsLastExpression) getExpression();
        if (expr.getCondition()) {
            return context -> context.isAtLast();
        } else {
            return context -> !context.isAtLast();
        }
    }

}

