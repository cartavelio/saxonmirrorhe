package com.saxonica.ee.elab;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import com.saxonica.ee.elab.gen.Elaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;

public class BooleanFnElaborator extends BooleanElaborator {

    public BooleanEvaluator getBooleanEvaluator() {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        return Elaborator.make(arg).getBooleanEvaluator();
    }

}

