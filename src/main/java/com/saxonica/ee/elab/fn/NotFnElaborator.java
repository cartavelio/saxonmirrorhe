package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;

public class NotFnElaborator extends BooleanElaborator {

    public BooleanEvaluator elaborateForBoolean() {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        BooleanEvaluator argEval = makeElaborator(arg).elaborateForBoolean();
        return context -> !argEval.eval(context);
    }


}

