package com.saxonica.ee.elab;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;

public abstract class NotFnElaborator extends BooleanElaborator {

    public BooleanEvaluator getBooleanEvaluator() {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        BooleanEvaluator argEval = Elaborator.make(arg).getBooleanEvaluator();
        return context -> !argEval.eval(context);
    }


}

