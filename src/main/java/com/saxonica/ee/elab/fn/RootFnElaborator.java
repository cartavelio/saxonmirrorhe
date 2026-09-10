package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import com.saxonica.ee.elab.gen.Elaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.CollatingFunctionFixed;
import net.sf.saxon.lib.SubstringMatcher;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.UnicodeString;

public class ContainsFnElaborator extends BooleanElaborator {

    public BooleanEvaluator getBooleanEvaluator() {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final StringEvaluator arg0Eval = Elaborator.make(fnc.getArg(0)).getStringEvaluator(EmptyUnicodeString.getInstance());
        final StringEvaluator arg1Eval = Elaborator.make(fnc.getArg(1)).getStringEvaluator(EmptyUnicodeString.getInstance());
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

