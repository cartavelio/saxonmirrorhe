package com.saxonica.ee.elab.gen;

import com.saxonica.ee.elab.eval.*;
import com.saxonica.ee.elab.expr.SystemFunctionCallElaborator;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.UnicodeString;

public abstract class Elaborator {


    private Expression expression;

    public Expression getExpression() {
        return expression;
    }

    public void setExpression(Expression expr) {
        this.expression = expr;
    }

    protected Configuration getConfiguration() {
        return expression.getConfiguration();
    }

    public Elaborator() {}


    public static Elaborator makeElaborator(Expression expression) {
        if (expression instanceof SystemFunctionCall) {
            String label = ((SystemFunctionCall)expression).getTargetFunction().getCompilerName();
            Elaborator elab = ElaboratorFactory.getElaborator(label);
            if (elab == null) {
                elab = new SystemFunctionCallElaborator();
            }
            elab.setExpression(expression);
            return elab;
        } else {
            String label = expression.getExpressionName();
            Elaborator elab = ElaboratorFactory.getElaborator(label);
            if (elab == null) {
                System.err.println("Using fallback elaborator for " + expression.toShortString());
                return new FallbackElaborator(expression);
            } else {
                elab.setExpression(expression);
                return elab;
            }
        }
    }

    /**
     * Get a function that evaluates the underlying expression in the form of
     * a {@link SequenceIterator}
     * @return an evaluator for the expression that returns a {@link SequenceIterator}
     */

    public abstract PullEvaluator elaborateForPull();

    /**
     * Get a function that evaluates the underlying expression in push mode, by
     * writing events to an {@link net.sf.saxon.event.Outputter}
     *
     * @return an evaluator for the expression in push mode
     */

    public abstract PushEvaluator elaborateForPush();

    /**
     * Get a function that evaluates the underlying expression in the form of
     * a {@link Item}. This must only be called for expressions whose result
     * has cardinality zero or one.
     *
     * @return an evaluator for the expression that returns an {@link Item}.
     */

    public abstract ItemEvaluator elaborateForItem();

    /**
     * Get a function that evaluates the underlying expression in the form of
     * a boolean, this being the effective boolean value of the expression.
     *
     * @return an evaluator for the expression that returns a boolean.
     */

    public abstract BooleanEvaluator elaborateForBoolean();

    /**
     * Get a function that evaluates the underlying expression in the form of
     * a string, this being the result of applying fn:string() to the result
     * of the expression.
     *
     * @param zeroLengthWhenAbsent if true, then when the result of the expression
     *                             is an empty sequence, the result of the StringEvaluator
     *                             should be a zero-length string. If false, the return value
     *                             should be null.
     * @return an evaluator for the expression that returns a string.
     */

    public abstract StringEvaluator elaborateForString(boolean zeroLengthWhenAbsent);

    /**
     * Helper method that returns either a zero length string or null, depending
     * on the parameter
     * @param zeroLengthWhenAbsent if true, return a zero length string; if false, return null
     */

    protected final UnicodeString handleNullString(boolean zeroLengthWhenAbsent) {
        return zeroLengthWhenAbsent ? EmptyUnicodeString.getInstance() : null;
    }

}

