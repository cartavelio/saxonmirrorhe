package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import com.saxonica.ee.elab.gen.Elaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.Matches;
import net.sf.saxon.regex.RegularExpression;
import net.sf.saxon.str.EmptyUnicodeString;

public class MatchesFnElaborator extends BooleanElaborator {

    public BooleanEvaluator getBooleanEvaluator() {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final Matches fn = (Matches) fnc.getTargetFunction();
        final StringEvaluator arg0eval = Elaborator.make(fnc.getArg(0)).getStringEvaluator(null);
        final RegularExpression staticRegex = fn.getStaticRegex();
        if (staticRegex == null) {
            final StringEvaluator arg1eval = Elaborator.make(fnc.getArg(1)).getStringEvaluator(null);
            final StringEvaluator arg2eval = fn.getArity() == 3
                    ? Elaborator.make(fnc.getArg(2)).getStringEvaluator(null)
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

