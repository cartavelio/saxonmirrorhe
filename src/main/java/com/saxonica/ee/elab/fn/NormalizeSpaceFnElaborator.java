package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.eval.UnicodeStringEvaluator;
import com.saxonica.ee.elab.gen.StringElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.Whitespace;

public class NormalizeSpaceFnElaborator extends StringElaborator {

    public UnicodeStringEvaluator elaborateForUnicodeString(boolean zeroLengthWhenAbsent) {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        UnicodeStringEvaluator argEval = makeElaborator(arg).elaborateForUnicodeString(zeroLengthWhenAbsent);
        return context -> {
            UnicodeString in = argEval.eval(context);
            if (in == null || in.isEmpty()) {
                return handleNullUnicodeString(zeroLengthWhenAbsent);
            }
            return Whitespace.collapseWhitespace(in);
        };
    }

    public StringEvaluator elaborateForString(boolean zeroLengthWhenAbsent) {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        StringEvaluator argEval = makeElaborator(arg).elaborateForString(zeroLengthWhenAbsent);
        return context -> {
            String in = argEval.eval(context);
            if (in == null || in.isEmpty()) {
                return handleNullString(zeroLengthWhenAbsent);
            }
            return Whitespace.collapseWhitespace(in);
        };
    }

    /**
     * Get a function that evaluates the underlying expression in the form of
     * a boolean, this being the effective boolean value of the expression.
     *
     * <p>This method is implemented for normalize-space() because it is a common
     * idiom to use normalize-space() in a boolean context to test whether a value
     * contains non-whitespace characters, and this can be done without actually
     * constructing the normalized string.</p>
     *
     * @return an evaluator for the expression that returns a boolean.
     */

    @Override
    public BooleanEvaluator elaborateForBoolean() {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        UnicodeStringEvaluator argEval = makeElaborator(arg).elaborateForUnicodeString(false);
        return context -> {
            UnicodeString in = argEval.eval(context);
            if (in == null || in.isEmpty()) {
                return false;
            }
            return !Whitespace.isAllWhite(in);
        };
    }
}

