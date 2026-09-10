////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr;


import net.sf.saxon.expr.elab.Elaborator;
import net.sf.saxon.expr.elab.ItemElaborator;
import net.sf.saxon.expr.elab.ItemEvaluator;
import net.sf.saxon.expr.parser.ContextItemStaticInfo;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.ma.MapOrArray;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.Cardinality;


/**
 * Cast Expression: implements "cast as data-type ( expression )" where the data type is
 * a map or array, as permitted in 4.0
 */

public class CastToMapOrArrayExpr extends UnaryExpression  {

    private final AnyFunctionType targetType;
    private final boolean allowEmpty;

    /**
     * Create a cast expression
     *
     * @param source     expression giving the value to be converted
     * @param target     the type to which the value is to be converted, must be an array or map type
     * @param allowEmpty true if the expression allows an empty sequence as input, producing
     *                   an empty sequence as output. If false, an empty sequence is a type error.
     */

    public CastToMapOrArrayExpr(Expression source, AnyFunctionType target, boolean allowEmpty) {
        super(source);
        this.targetType = target;
        this.allowEmpty = allowEmpty;
    }

    @Override
    protected OperandRole getOperandRole() {
        return OperandRole.ABSORB;
    }

    /**
     * Type-check the expression
     */

    /*@NotNull*/
    @Override
    public Expression typeCheck(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo) throws XPathException {
        return super.typeCheck(visitor, contextInfo);
    }

    /**
     * Perform optimisation of an expression and its subexpressions.
     * <p>This method is called after all references to functions and variables have been resolved
     * to the declaration of the function or variable, and after all type checking has been done.</p>
     *
     * @param visitor         an expression visitor
     * @param contextInfo the static type of "." at the point where this expression is invoked.
     *                        The parameter is set to null if it is known statically that the context item will be undefined.
     *                        If the type of the context item is not known statically, the argument is set to
     *                        {@link Type#ITEM_TYPE}
     * @return the original expression, rewritten if appropriate to optimize execution
     * @throws XPathException
     *          if an error is discovered during this phase
     *          (typically a type error)
     */

    /*@NotNull*/
    @Override
    public Expression optimize(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo) throws XPathException {
        return super.optimize(visitor, contextInfo);
    }


    /**
     * Get the static cardinality of the expression
     */

    @Override
    protected int computeCardinality() {
        return allowEmpty && Cardinality.allowsZero(getBaseExpression().getCardinality())
                ? StaticProperty.ALLOWS_ZERO_OR_ONE : StaticProperty.EXACTLY_ONE;
    }

    /**
     * Get the static type of the expression
     */

    /*@NotNull*/
    @Override
    public ItemType getItemType() {
        return targetType;
    }


    /**
     * Get the static type of the expression as a UType, following precisely the type
     * inference rules defined in the XSLT 3.0 specification.
     *
     * @return the static item type of the expression according to the XSLT 3.0 defined rules
     * @param contextItemType static information about the context item
     */
    @Override
    public UType getStaticUType(UType contextItemType) {
        return targetType.getUType();
    }



    /**
     * Copy an expression. This makes a deep copy.
     *
     * @return the copy of the original expression
     * @param rebindings  variables that need to be rebound
     */

    /*@NotNull*/
    @Override
    public Expression copy(RebindingMap rebindings) {
        return new CastToMapOrArrayExpr(getBaseExpression().copy(rebindings), targetType, allowEmpty);
    }

    /**
     * An implementation of Expression must provide at least one of the methods evaluateItem(), iterate(), or process().
     * This method indicates which of these methods is provided directly. The other methods will always be available
     * indirectly, using an implementation that relies on one of the other methods.
     *
     * @return the implementation method, for example {@link #ITERATE_METHOD} or {@link #EVALUATE_METHOD} or
     * {@link #PROCESS_METHOD}
     */
    @Override
    public int getImplementationMethod() {
        return EVALUATE_METHOD;
    }

    /**
     * Evaluate the expression
     */

    /*@Nullable*/
    @Override
    public MapOrArray evaluateItem(XPathContext context) throws XPathException {
        return (MapOrArray) makeElaborator().elaborateForItem().eval(context);
    }

    /**
     * Is this expression the same as another expression?
     */

    public boolean equals(Object other) {
        return other instanceof CastToMapOrArrayExpr e2 &&
                getBaseExpression().isEqual(e2.getBaseExpression()) &&
                targetType == e2.targetType &&
                allowEmpty == e2.allowEmpty;
    }

    /**
     * get HashCode for comparing two expressions.
     */

    @Override
    protected int computeHashCode() {
        return super.computeHashCode() ^ targetType.hashCode();
    }

    /**
     * Represent the expression as a string. The resulting string will be a valid XPath 3.0 expression
     * with no dependencies on namespace bindings.
     *
     * @return the expression as a string in XPath 3.0 syntax
     */

    public String toString() {
        return getBaseExpression().toString() + " cast as " + targetType;
    }

    @Override
    public String toShortString() {
        return getBaseExpression().toShortString() + " cast as " + targetType;
    }

    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     */

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        out.startElement("castToArray");
        // TODO
        out.endElement();
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
        return "castToArray";
    }

    /**
     * Make an elaborator for this expression
     *
     * @return a suitable elaborator
     */

    @Override
    public Elaborator getElaborator() {
        return new CastToArrayElaborator();
    }

    /**
     * Elaborator for {@code cast as} expression, or the equivalent constructor function call
     */

    public static class CastToArrayElaborator extends ItemElaborator {

        public ItemEvaluator elaborateForItem() {
            CastToMapOrArrayExpr exp = (CastToMapOrArrayExpr) getExpression();
            Expression arg = exp.getBaseExpression();
            ItemEvaluator argEval = arg.makeElaborator().elaborateForItem();
            NamespaceResolver resolver = exp.getRetainedStaticContext();
            boolean allowEmpty = exp.allowEmpty;
            CastingTarget targetType = (CastingTarget)exp.targetType;

            return context -> {
                try {
                    Item value = argEval.eval(context);
                    if (value == null) {
                        if (allowEmpty) {
                            return null;
                        } else {
                            throw new XPathException("Cast expression: input is empty, but empty input is not allowed", "XPTY0004");
                        }
                    }
                    return targetType.cast(value, context, resolver, allowEmpty);
                } catch (XPathException e) {
                    throw e.maybeWithLocation(exp.getLocation())
                            .maybeWithContext(context);
                }
            };

        }

    }
}

