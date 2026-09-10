package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.StringElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.str.UnicodeString;

public class StringFnElaborator extends StringElaborator {

    public StringEvaluator getStringEvaluator(UnicodeString resultIfEmpty) {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        return Elaborator.make(arg).getStringEvaluator(null);
    }
}
