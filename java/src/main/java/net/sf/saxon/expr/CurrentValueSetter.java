////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr;

import net.sf.saxon.expr.elab.Elaborator;
import net.sf.saxon.expr.elab.PullElaborator;
import net.sf.saxon.expr.elab.PullEvaluator;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.UType;


/**
 * Abstract 4.0 expression that invokes a supplied expression after setting the current value
 * (as returned by fn:current())
 */

public class CurrentValueSetter extends UnaryExpression {

    /**
     * Constructor
     *
     * @param target The expression to be evaluated after setting the context value
     */

    public CurrentValueSetter(Expression target) {
        super(target);
    }

    @Override
    public String getExpressionName() {
        return "currentValueSetter";
    }

    @Override
    protected OperandRole getOperandRole() {
        return OperandRole.FOCUS_CONTROLLED_ACTION;
    }

    /**
     * Get the static type of the expression as a UType, following precisely the type
     * inference rules defined in the XSLT 3.0 specification.
     *
     * @return the static item type of the expression according to the XSLT 3.0 defined rules
     */
    @Override
    public UType getStaticUType(UType contextItemType) {
        return getBaseExpression().getStaticUType(contextItemType);
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
     * Copy an expression. This makes a deep copy.
     *
     * @return the copy of the original expression
     * @param rebindings variables that need to be re-bound
     */

    /*@NotNull*/
    @Override
    public Expression copy(RebindingMap rebindings) {
        return new CurrentValueSetter(getBaseExpression().copy(rebindings));
    }

    /**
     * Get the static properties of this expression (other than its type). The result is
     * bit-significant. These properties are used for optimizations. In general, if
     * property bit is set, it is true, but if it is unset, the value is unknown.
     */

    @Override
    protected int computeSpecialProperties() {
        return super.computeSpecialProperties() | StaticProperty.DEPENDS_ON_CURRENT_ITEM;
    }


    /**
     * Is this expression the same as another expression?
     */

    public boolean equals(Object other) {
        if (other instanceof CurrentValueSetter p) {
            return getBaseExpression().isEqual(p.getBaseExpression());
        }
        return false;
    }

    /**
     * Get hashCode for comparing two expressions
     */

    @Override
    protected int computeHashCode() {
        return "CurrentValueSetter".hashCode() + getBaseExpression().hashCode();
    }


    /**
     * Export expression structure to SEF file. The abstract expression tree
     * is written to the supplied output destination.
     */

    @Override
    public void export(ExpressionPresenter destination) throws XPathException {
        destination.startElement("withCurrent", this);
        getBaseExpression().export(destination);
        destination.endElement();
    }
    

    public Item evaluateItem(XPathContext context) throws XPathException {
        return makeElaborator().elaborateForItem().eval(context);
    }

    @Override
    public SequenceIterator iterate(XPathContext context) throws XPathException {
        return makeElaborator().elaborateForPull().iterate(context);
    }

    /**
     * Make an elaborator for this expression
     *
     * @return a suitable elaborator
     */

    @Override
    public Elaborator getElaborator() {
        return new CurrentValueSetterElaborator();
    }

    /**
     * Elaborator for a context value setter expression.
     */

    public static class CurrentValueSetterElaborator extends PullElaborator {

        @Override
        public PullEvaluator elaborateForPull() {
            CurrentValueSetter expr = (CurrentValueSetter) getExpression();
            PullEvaluator select = expr.getBaseExpression().makeElaborator().elaborateForPull();
            return context -> {
                XPathContextMajor c2 = context.newContext();
                c2.setCurrentValue(context.getContextItem());
                return select.iterate(c2);
            };
        }

    }
}

