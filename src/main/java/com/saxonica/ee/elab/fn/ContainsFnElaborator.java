package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.CollatingFunctionFixed;
import net.sf.saxon.lib.SubstringMatcher;
import net.sf.saxon.str.UnicodeString;

public class ContainsFnElaborator extends BooleanElaborator {

    public BooleanEvaluator elaborateForBoolean() {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final StringEvaluator arg0Eval = makeElaborator(fnc.getArg(0)).elaborateForString(true);
        final StringEvaluator arg1Eval = makeElaborator(fnc.getArg(1)).elaborateForString(true);
        final CollatingFunctionFixed fn = (CollatingFunctionFixed)fnc.getTargetFunction();
        final SubstringMatcher collation = (SubstringMatcher)fn.getStringCollator();
        assert collation != null;

        final String name = fnc.getFunctionName().getLocalPart();
        switch (name) {
            case "contains":
                return context -> {
                    UnicodeString s0 = arg0Eval.eval(context);
                    UnicodeString s1 = arg1Eval.eval(context);
                    return collation.contains(s0, s1);
                };
            case "starts-with":
                return context -> {
                    UnicodeString s0 = arg0Eval.eval(context);
                    UnicodeString s1 = arg1Eval.eval(context);
                    return collation.startsWith(s0, s1);
                };
            case "ends-with":
                return context -> {
                    UnicodeString s0 = arg0Eval.eval(context);
                    UnicodeString s1 = arg1Eval.eval(context);
                    return collation.endsWith(s0, s1);
                };
            default:
                throw new UnsupportedOperationException();
        }

    }

}

