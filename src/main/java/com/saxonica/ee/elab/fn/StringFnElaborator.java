package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.StringElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;

public class StringFnElaborator extends StringElaborator {

    public StringEvaluator elaborateForString(boolean zeroLengthWhenAbsent) {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        return makeElaborator(arg).elaborateForString(true);
    }
}
