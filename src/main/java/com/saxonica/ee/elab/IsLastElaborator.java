package com.saxonica.ee.elab;

import net.sf.saxon.expr.IsLastExpression;

public abstract class IsLastElaborator extends BooleanElaborator {

    public BooleanEvaluator getBooleanEvaluator() {
        IsLastExpression expr = (IsLastExpression) getExpression();
        if (expr.getCondition()) {
            return context -> context.isAtLast();
        } else {
            return context -> !context.isAtLast();
        }
    }

}

