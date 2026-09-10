package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import net.sf.saxon.expr.IsLastExpression;

/**
 * Elaborator for an "isLast" expression, equivalent to {@code position() = last()} or {@code position() != last()}
 */

public class IsLastElaborator extends BooleanElaborator {

    public BooleanEvaluator elaborateForBoolean() {
        IsLastExpression expr = (IsLastExpression) getExpression();
        if (expr.getCondition()) {
            //noinspection Convert2MethodRef
            return context -> context.isAtLast();
        } else {
            return context -> !context.isAtLast();
        }
    }

}

