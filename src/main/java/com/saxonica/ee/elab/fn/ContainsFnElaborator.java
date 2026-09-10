package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.eval.UnicodeStringEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.functions.CollatingFunctionFixed;
import net.sf.saxon.lib.SubstringMatcher;
import net.sf.saxon.str.UnicodeString;

/**
 * Expression elaborator for a call to contains(), starts-with(), or ends-with()
 */

public class ContainsFnElaborator extends BooleanElaborator {

    public BooleanEvaluator elaborateForBoolean() {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final CollatingFunctionFixed fn = (CollatingFunctionFixed)fnc.getTargetFunction();
        final SubstringMatcher collation = (SubstringMatcher)fn.getStringCollator();
        assert collation != null;
        final String name = fnc.getFunctionName().getLocalPart();
        if (collation == CodepointCollator.getInstance()) {
            final StringEvaluator arg0Eval = makeElaborator(fnc.getArg(0)).elaborateForString(true);
            final StringEvaluator arg1Eval = makeElaborator(fnc.getArg(1)).elaborateForString(true);
            switch (name) {
                case "contains":
                    return context -> arg0Eval.eval(context).contains(arg1Eval.eval(context));
                case "starts-with":
                    return context -> arg0Eval.eval(context).startsWith(arg1Eval.eval(context));
                case "ends-with":
                    return context -> arg0Eval.eval(context).endsWith(arg1Eval.eval(context));
                default:
                    throw new UnsupportedOperationException();
            }
        } else {
            final UnicodeStringEvaluator arg0Eval = makeElaborator(fnc.getArg(0)).elaborateForUnicodeString(true);
            final UnicodeStringEvaluator arg1Eval = makeElaborator(fnc.getArg(1)).elaborateForUnicodeString(true);
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

}

