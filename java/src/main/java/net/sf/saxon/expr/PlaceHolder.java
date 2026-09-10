package net.sf.saxon.expr;

import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;

/**
 * A PlaceHolder corresponds to the symbol "?" in a partial function application;
 * it is a PseudoExpression because it can appear in the expression tree in the place
 * of an expression, but is never actually evaluated.
 *
 * <p>In 4.0, placeholders can be associated with keywords, and the mapping from arguments
 * of the partially-applied function to arguments of the base function is therefore
 * more complex. The PlaceHolder holds an integer indicating a 0-based offset of the placeholder
 * in the argument list of the partially-applied function.</p>
 */

public class PlaceHolder extends PseudoExpression {

    private final StructuredQName paramName;
    private final int argumentPosition;
    private final int placeHolderSequence;

    /**
     * Create a placeholder, representing a "?" in a partial function application
     * @param paramName the keyword associated with the placeholder, or null if there is no keyword
     * @param argumentPosition the 0-based position of the placeholder in the sequence of arguments
     *                         passed in the partial function call
     * @param placeHolderSequence the 0-based position of this placeholder among all the
     *                            placeholders in the partial function call
     */

    public PlaceHolder(StructuredQName paramName, int argumentPosition, int placeHolderSequence) {
        this.paramName = paramName;
        this.argumentPosition = argumentPosition;
        this.placeHolderSequence = placeHolderSequence;
    }

    public int getArgumentPosition() {
        return argumentPosition;
    }

    public int getPlaceHolderSequence() {
        return placeHolderSequence;
    }

    public StructuredQName getParamName() {
        return paramName;
    }

    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     *
     * @param out the expression presenter used to display the structure
     * @throws XPathException if the export fails, for example if an expression is found that won't work
     *                        in the target environment.
     */
    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        // no action
    }

    /**
     * Copy an expression. This makes a deep copy.
     *
     * @param rebindings a mutable list of (old binding, new binding) pairs
     *                   that is used to update the bindings held in any
     *                   local variable references that are copied.
     * @return the copy of the original expression
     */
    @Override
    public Expression copy(RebindingMap rebindings) {
        return this;
    }
}

