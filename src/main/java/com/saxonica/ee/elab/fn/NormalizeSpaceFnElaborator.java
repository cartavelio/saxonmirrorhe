package com.saxonica.ee.elab;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.StringElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.Whitespace;

public class NormalizeSpaceFnElaborator extends StringElaborator {

    public StringEvaluator getStringEvaluator(UnicodeString resultIfEmpty) {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        StringEvaluator argEval = Elaborator.make(arg).getStringEvaluator(null);
        return context -> {
            UnicodeString in = argEval.eval(context);
            if (in == null || in.isEmpty()) {
                return resultIfEmpty;
            }
            return Whitespace.collapseWhitespace(in);
        };
    }

    @Override
    public BooleanEvaluator getBooleanEvaluator() {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        StringEvaluator argEval = Elaborator.make(arg).getStringEvaluator(null);
        return context -> {
            UnicodeString in = argEval.eval(context);
            if (in == null || in.isEmpty()) {
                return false;
            }
            return !Whitespace.isAllWhite(in);
        };
    }
}

