package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.eval.UnicodeStringEvaluator;
import com.saxonica.ee.elab.gen.StringElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.functions.SubstringAfter;
import net.sf.saxon.lib.SubstringMatcher;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.UnicodeString;

public class SubstringAfterFnElaborator extends StringElaborator {

    public UnicodeStringEvaluator elaborateForUnicodeString(boolean zeroLengthWhenAbsent) {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final SubstringAfter fn = (SubstringAfter) fnc.getTargetFunction();
        final SubstringMatcher stringCollator = (SubstringMatcher) fn.getStringCollator();
        assert stringCollator != null; // We don't go down this path until the collation is known

        final UnicodeStringEvaluator arg0Eval = makeElaborator(fnc.getArg(0)).elaborateForUnicodeString(true);
        final UnicodeStringEvaluator arg1Eval = makeElaborator(fnc.getArg(1)).elaborateForUnicodeString(true);

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
                long i = s0.indexOf(s1, 0);
                if (i < 0) {
                    return EmptyUnicodeString.getInstance();
                }
                return s0.substring(i + s1.length());
            };
        } else {
            return context -> {
                UnicodeString s0 = arg0Eval.eval(context);
                UnicodeString s1 = arg1Eval.eval(context);
                if (s1.isEmpty()) {
                    return s0;
                }
                if (s0.isEmpty()) {
                    return EmptyUnicodeString.getInstance();
                }
                return stringCollator.substringAfter(s0, s1);
            };
        }
    }

    public StringEvaluator elaborateForString(boolean zeroLengthWhenAbsent) {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final SubstringAfter fn = (SubstringAfter) fnc.getTargetFunction();
        final SubstringMatcher stringCollator = (SubstringMatcher) fn.getStringCollator();
        assert stringCollator != null; // We don't go down this path until the collation is known


        if (stringCollator instanceof CodepointCollator) {
            final StringEvaluator arg0Eval = makeElaborator(fnc.getArg(0)).elaborateForString(true);
            final StringEvaluator arg1Eval = makeElaborator(fnc.getArg(1)).elaborateForString(true);

            return context -> {
                String s0 = arg0Eval.eval(context);
                String s1 = arg1Eval.eval(context);
                if (s1.isEmpty()) {
                    return s0;
                }
                if (s0.isEmpty()) {
                    return "";
                }
                int i = s0.indexOf(s1, 0);
                if (i < 0) {
                    return "";
                }
                return s0.substring(i + s1.length());
            };
        } else {
            return super.elaborateForString(zeroLengthWhenAbsent);
        }
    }

}

