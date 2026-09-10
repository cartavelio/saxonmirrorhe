package net.sf.saxon.expr;

import net.sf.saxon.expr.elab.Elaborator;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;

/**
 * <code>DefaultedArgumentExpression</code> is a placeholder used in a function call for an argument
 * that is evaluated using the default value expression from the function definition, in cases where
 * that expression has not yet been compiled (typically because the function call precedes the
 * function declaration). It should be resolved to a real expression during type checking of the
 * function call.
 */

public class DefaultedArgumentExpression extends PseudoExpression {

    /**
     * Constructor
     */

    public DefaultedArgumentExpression() {
    }


    /**
     * Copy an expression. This makes a deep copy.
     *
     * @param rebindings variables that need to be re-bound
     * @return the copy of the original expression
     */

    /*@NotNull*/
    @Override
    public Expression copy(RebindingMap rebindings) {
        return this;
    }


    /**
     * Get a name identifying the kind of expression, in terms meaningful to a user.
     *
     * @return a name identifying the kind of expression, in terms meaningful to a user.
     * The name will always be in the form of a lexical XML QName, and should match the name used
     * in export() output displaying the expression.
     */
    @Override
    public String getExpressionName() {
        return "defaultValue";
    }


    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     */

    @Override
    public void export(ExpressionPresenter destination) throws XPathException {
        throw new UnsupportedOperationException();
    }


    @Override
    public Elaborator getElaborator() {
        throw new UnsupportedOperationException();
    }


}

