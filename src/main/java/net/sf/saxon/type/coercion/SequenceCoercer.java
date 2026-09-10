////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.type.coercion;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.elab.Elaborator;
import net.sf.saxon.expr.elab.PullElaborator;
import net.sf.saxon.expr.elab.PullEvaluator;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.SequenceType;

import java.util.function.Supplier;


/**
 * An SequenceCoercer is an expression that performs coercion on a sequence of items:
 */

public final class SequenceCoercer extends UnaryExpression {

    private final SequenceType requiredType;
    private final Supplier<RoleDiagnostic> roleSupplier;
    private final boolean allow40;


    /**
     * Constructor
     *  @param sequence         this must be a sequence of function item values. This is not checked; a ClassCastException
     *                         will occur if the precondition is not satisfied.
     * @param requiredType the function item type to which all items in the sequence should be converted,
     */

    public SequenceCoercer(Expression sequence, SequenceType requiredType,
                           Supplier<RoleDiagnostic> role, boolean allow40) {
        super(sequence);
        this.requiredType = requiredType;
        this.roleSupplier = role;
        this.allow40 = allow40;
        ExpressionTool.copyLocationInfo(sequence, this);
    }

    @Override
    protected OperandRole getOperandRole() {
        return OperandRole.INSPECT;
    }

    /**
     * Simplify an expression
     *
     */

    /*@NotNull*/
    @Override
    public Expression simplify() throws XPathException {
        try {
            setBaseExpression(getBaseExpression().simplify());
            if (getBaseExpression() instanceof Literal) {
                GroundedValue val =
                        SequenceTool.toGroundedValue(iterate(new EarlyEvaluationContext(getConfiguration())));
                return Literal.makeLiteral(val, this);
            }
            return this;
        } catch (UncheckedXPathException e) {
            throw e.getXPathException();
        }
    }

    /**
     * Type-check the expression
     */

    /*@NotNull*/
    @Override
    public Expression typeCheck(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo) throws XPathException {
        getOperand().typeCheck(visitor, contextInfo);
        return this;
//        final TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
//        // We can't dispense with function coercion just because the supplied function is an instance
//        // of the required type, because it might not perform enough checking of arguments: see test VarDecl066.
//        // We can only dispense with it if the argument checking and result checking is at least
//        // as strict as the required type. For now, do coercion unless the type signatures are identical
//
//        // TODO: optimization
    }

//    /**
//     * Determine the special properties of this expression
//     *
//     * @return {@link StaticProperty#NO_NODES_NEWLY_CREATED}.
//     */
//
//    @Override
//    protected int computeSpecialProperties() {
//        int p = super.computeSpecialProperties();
//        return p | StaticProperty.NO_NODES_NEWLY_CREATED;
//    }

    /**
     * Copy an expression. This makes a deep copy.
     *
     * @return the copy of the original expression
     * @param rebindings variables that need to be re-bound
     */

    /*@NotNull*/
    @Override
    public Expression copy(RebindingMap rebindings) {
        SequenceCoercer fsc2 = new SequenceCoercer(
                getBaseExpression().copy(rebindings), requiredType, roleSupplier, allow40);
        ExpressionTool.copyLocationInfo(this, fsc2);
        return fsc2;
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
        return ITERATE_METHOD;
    }

    /**
     * Iterate over the sequence of functions, wrapping each one in a CoercedFunction object
     */

    /*@NotNull*/
    @Override
    public SequenceIterator iterate(final XPathContext context) throws XPathException {
        return makeElaborator().elaborateForPull().iterate(context);
    }

    /**
     * Evaluate as an Item. 
     */

    /*@Nullable*/
    @Override
    public Item evaluateItem(XPathContext context) throws XPathException {
        return makeElaborator().elaborateForItem().eval(context);
    }


    /**
     * Determine the data type of the items returned by the expression, if possible
     *
     * @return a value such as Type.STRING, Type.BOOLEAN, Type.NUMBER, Type.NODE,
     * or Type.ITEM (meaning not known in advance)
     */

    /*@NotNull*/
    @Override
    public ItemType getItemType() {
        return requiredType.getPrimaryType();
    }

    /**
     * Determine the static cardinality of the expression
     */

    @Override
    protected int computeCardinality() {
        // TODO: cardinality can increase due to atomization
        return getBaseExpression().getCardinality();
    }

    /**
     * @return the role locator
     */
    public RoleDiagnostic getRoleSupplier() {
        return roleSupplier.get();
    }

    /**
     * Is this expression the same as another expression?
     */

    public boolean equals(Object other) {
        return super.equals(other) &&
            requiredType.equals(((SequenceCoercer) other).requiredType);
    }

    @Override
    protected int computeHashCode() {
        return super.computeHashCode() ^ requiredType.hashCode();
    }

    @Override
    public String getExpressionName() {
        return "coercer";
    }


    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     */

    @Override
    public void export(ExpressionPresenter destination) throws XPathException {
        destination.startElement("coercer", this);
        destination.emitAttribute("type", requiredType.toAlphaCode());
        destination.emitAttribute("diag", roleSupplier.get().save());
        String flags = "" + (allow40 ? "4" : "") ;
        if (!flags.isEmpty()) {
            destination.emitAttribute("flags", "4");
        }
        getBaseExpression().export(destination);
        destination.endElement();
    }

    /**
     * Make an elaborator for this expression
     *
     * @return an appropriate {@link Elaborator}
     */
    @Override
    public Elaborator getElaborator() {
        return new SequenceCoercerElaborator();
    }



    private static class SequenceCoercerElaborator extends PullElaborator {

        @Override
        public PullEvaluator elaborateForPull() {
            SequenceCoercer expr = (SequenceCoercer) getExpression();
            SequenceType requiredType = expr.requiredType;
            SequenceType suppliedType = SequenceType.makeSequenceType(expr.getItemType(), expr.getCardinality());
            CoercionPlan plan = requiredType.getPrimaryType().getCoercionPlan(expr.getPackageData().getHostLanguageVersion());
            PullEvaluator base = expr.getBaseExpression().makeElaborator().elaborateForPull();
            CoercionRequest request = new CoercionRequest(
                    suppliedType, expr.getConfiguration(), expr.roleSupplier, expr.getLocation());
            return context -> plan.coerceSequence(base.iterate(context), requiredType, request);
        }

    }
}

// Copyright (c) 2009-2023 Saxonica Limited