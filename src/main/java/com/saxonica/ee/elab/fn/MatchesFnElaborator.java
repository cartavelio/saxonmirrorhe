package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.UnicodeStringEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.Matches;
import net.sf.saxon.regex.RegularExpression;
import net.sf.saxon.str.EmptyUnicodeString;

public class MatchesFnElaborator extends BooleanElaborator {

    public BooleanEvaluator elaborateForBoolean() {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final Matches fn = (Matches) fnc.getTargetFunction();
        final UnicodeStringEvaluator arg0eval = makeElaborator(fnc.getArg(0)).elaborateForUnicodeString(true);
        final RegularExpression staticRegex = fn.getStaticRegex();
        if (staticRegex == null) {
            final UnicodeStringEvaluator arg1eval = makeElaborator(fnc.getArg(1)).elaborateForUnicodeString(true);
            final UnicodeStringEvaluator arg2eval = fn.getArity() == 3
                    ? makeElaborator(fnc.getArg(2)).elaborateForUnicodeString(true)
                    : cxt -> EmptyUnicodeString.getInstance();
            return context -> fn.evalMatches(
                        arg0eval.eval(context),
                        arg1eval.eval(context),
                        arg2eval.eval(context),
                        context);
        } else {
            return context -> staticRegex.containsMatch(arg0eval.eval(context));
        }
    }

}

