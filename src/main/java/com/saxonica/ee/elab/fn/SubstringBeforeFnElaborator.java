package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.StringElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.functions.SubstringBefore;
import net.sf.saxon.lib.SubstringMatcher;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.UnicodeString;

public class SubstringBeforeFnElaborator extends StringElaborator {

    public StringEvaluator elaborateForString(boolean zeroLengthWhenAbsent) {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final SubstringBefore fn = (SubstringBefore) fnc.getTargetFunction();
        final SubstringMatcher stringCollator = (SubstringMatcher) fn.getStringCollator();
        assert stringCollator != null; // We don't go down this path until the collation is known

        final StringEvaluator arg0Eval = makeElaborator(fnc.getArg(0)).elaborateForString(true);
        final StringEvaluator arg1Eval = makeElaborator(fnc.getArg(1)).elaborateForString(true);

        if (stringCollator instanceof CodepointCollator) {
            return context -> {
                UnicodeString s0 = arg0Eval.eval(context);
                UnicodeString s1 = arg1Eval.eval(context);
                if (s1.isEmpty()) {
                    return s0;
                }
                if (s0.isEmpty()) {
                    return EmptyUnicodeString.getInstance();
                }
                long j = s0.indexOf(s1, 0);
                if (j < 0) {
                    return EmptyUnicodeString.getInstance();
                }
                return s0.prefix(j);
            };
        } else {
            return context -> {
                UnicodeString s0 = arg0Eval.eval(context);
                UnicodeString s1 = arg1Eval.eval(context);
                return stringCollator.substringBefore(s0, s1);
            };
        }
    }

}

