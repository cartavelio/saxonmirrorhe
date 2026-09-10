package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.StringElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;

public class ForceCaseFnElaborator extends StringElaborator {

    public StringEvaluator getStringEvaluator(UnicodeString resultIfEmpty) {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        StringEvaluator argEval = Elaborator.make(arg).getStringEvaluator(null);
        if (fnc.getFunctionName().getLocalPart().equals("lower-case")) {
            return context -> {
                UnicodeString s0 = argEval.eval(context);
                if (s0 == null) {
                    return null;
                }
                return StringView.of(s0.toString().toLowerCase());
            };
        } else {
            return context -> {
                UnicodeString s0 = argEval.eval(context);
                if (s0 == null) {
                    return null;
                }
                return StringView.of(s0.toString().toUpperCase());
            };
        }
    }
}
