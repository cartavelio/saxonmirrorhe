package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.StringElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.trans.XPathException;

public class StringFnElaborator extends StringElaborator {

    public StringEvaluator elaborateForString(boolean zeroLengthWhenAbsent) {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        StringEvaluator argEval = makeElaborator(arg).elaborateForString(true);
        return context -> {
            try {
                return argEval.eval(context);
            } catch (UnsupportedOperationException err) {
                throw new XPathException(err.getMessage(), "FOTY0014");
            }
        };
    }
}
