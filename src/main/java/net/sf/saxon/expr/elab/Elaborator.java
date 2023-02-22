package net.sf.saxon.expr.elab;

import net.sf.saxon.Configuration;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.UnicodeString;

/**
 * This class is at the heart of the mechanism for elaborating expressions.
 *
 * <p>This mechanism evaluates expressions in two stages. The first stage (the elaboration stage),
 * converts the expression to a lambda function, specifically a function of the context that can
 * be called to produce the expression result. The second stage invokes this function (supplying
 * the context as an argument) to generate the result.</p>
 *
 * <p>The mechanism is complicated by the fact that expressions can be evaluated in different modes.
 * Specifically: pull evaluation delivers an iterator over the result, item evaluation delivers
 * the singleton result as an item; push evaluation sends the results to an {@link Outputter},
 * boolean evaluation returns the effective boolean value, and string evaluation returns the
 * string value.</p>
 *
 * <p>The {@code Evaluator} class is the abstract root of a hierarchy of classes for elaborating
 * different kinds of expression.</p>
 */

public abstract class Elaborator {

    private Expression expression;

    public Elaborator() {
    }

    /**
     * Get the expression being elaborated
     * @return the expression
     */
    public Expression getExpression() {
        return expression;
    }

    /**
     * Set the expression being elaborated
     * @param expr the expression
     */

    public void setExpression(Expression expr) {
        this.expression = expr;
    }

    /**
     * Get the Saxon Configuration
     * @return  the Configuration
     */

    protected Configuration getConfiguration() {
        return expression.getConfiguration();
    }


    /**
     * Factory method to construct an {@code Elaborator} for a specific expression
     * @param expression the expression to be elaborated
     * @return an elaborator for the expression
     */

    public static Elaborator makeElaborator(Expression expression) {
        Elaborator elab = expression.getElaborator();
        elab.setExpression(expression);
        return elab;
    }

    /**
     * Get a function that evaluates the underlying expression eagerly
     *
     * @return an evaluator for the expression that returns a {@link GroundedValue}
     */

    public abstract EagerEvaluator eagerly();

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
     * @return an evaluator for the expression that returns an {@link Item}, or
     * null to represent an empty sequence.
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
     * a unicode string, this being the result of applying fn:string() to the result
     * of the expression.
     *
     * @param zeroLengthWhenAbsent if true, then when the result of the expression
     *                             is an empty sequence, the result of the StringEvaluator
     *                             should be a zero-length string. If false, the return value
     *                             should be null.
     * @return an evaluator for the expression that returns a string.
     */

    public abstract UnicodeStringEvaluator elaborateForUnicodeString(boolean zeroLengthWhenAbsent);

    /**
     * Get a function that evaluates the underlying expression in the form of
     * a Java string, this being the result of applying fn:string() to the result
     * of the expression.
     *
     * @param zeroLengthWhenAbsent if true, then when the result of the expression
     *                             is an empty sequence, the result of the StringEvaluator
     *                             should be a zero-length string. If false, the return value
     *                             should be null.
     * @return an evaluator for the expression that returns a string.
     */

    public StringEvaluator elaborateForString(boolean zeroLengthWhenAbsent) {
        return context -> elaborateForUnicodeString(zeroLengthWhenAbsent).eval(context).toString();
    }


    /**
     * Helper method that returns either a zero length string or null, depending
     * on the parameter
     * @param zeroLengthWhenAbsent if true, return a zero length string; if false, return null
     */

    protected final UnicodeString handleNullUnicodeString(boolean zeroLengthWhenAbsent) {
        return zeroLengthWhenAbsent ? EmptyUnicodeString.getInstance() : null;
    }

    /**
     * Helper method that returns either a zero length string or null, depending
     * on the parameter
     *
     * @param zeroLengthWhenAbsent if true, return a zero length string; if false, return null
     */

    protected final String handleNullString(boolean zeroLengthWhenAbsent) {
        return zeroLengthWhenAbsent ? "" : null;
    }

}

