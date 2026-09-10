package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.StringElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.Cardinality;

public class ForceCaseFnElaborator extends StringElaborator {

    public StringEvaluator elaborateForString(boolean zeroLengthWhenAbsent) {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final Expression arg = fnc.getArg(0);
        final StringEvaluator argEval = makeElaborator(arg).elaborateForString(false);
        final boolean nullable = Cardinality.allowsZero(arg.getCardinality());
        if (fnc.getFunctionName().getLocalPart().equals("lower-case")) {
            return context -> {
                UnicodeString s0 = argEval.eval(context);
                if (nullable && s0 == null) {
                    return handleNullString(zeroLengthWhenAbsent);
                }
                return StringView.of(s0.toString().toLowerCase());
            };
        } else {
            return context -> {
                UnicodeString s0 = argEval.eval(context);
                if (nullable && s0 == null) {
                    return handleNullString(zeroLengthWhenAbsent);
                }
                return StringView.of(s0.toString().toUpperCase());
            };
        }
    }
}
