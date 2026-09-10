package com.saxonica.ee.optim;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.elab.Elaborator;
import net.sf.saxon.expr.elab.PullElaborator;
import net.sf.saxon.expr.elab.PullEvaluator;
import net.sf.saxon.expr.instruct.GlobalVariable;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.EmptySequence;

/**
 * An IndexedFilterExpression contains a base expression and a filter predicate. The base expression
 * will always be a local variable reference. The filter will always be a Comparison, and it will
 * have no positional dependencies
 */

public final class IndexedFilterExpression extends Expression {

    private final Operand baseOp;
    private final Operand useOp;
    private final Operand searchOp;
    private final AtomicComparer comparer;
    private final boolean convertUntypedToOther;


    /**
     * Constructor
     *
     * @param start             Variable containing the sequence of items from which the selection will take place.
     * @param filter            An expression defining the filter predicate
     * @param indexFirstOperand true if the first operand of the filter comparison is to be indexed, false
     *                          if the second operand is to be indexed
     */

    public IndexedFilterExpression(VariableReference start, ComparisonExpression filter, boolean indexFirstOperand) {
        baseOp = new Operand(this, start, OperandRole.FOCUS_CONTROLLING_SELECT);
        if (indexFirstOperand) {
            useOp = new Operand(this, filter.getLhsExpression(), FilterExpression.FILTER_PREDICATE);
            searchOp = new Operand(this, filter.getRhsExpression(), OperandRole.ATOMIC_SEQUENCE);
        } else {
            useOp = new Operand(this, filter.getRhsExpression(), FilterExpression.FILTER_PREDICATE);
            searchOp = new Operand(this, filter.getLhsExpression(), OperandRole.ATOMIC_SEQUENCE);
        }
        this.comparer = filter.getAtomicComparer();
        this.convertUntypedToOther = filter.convertsUntypedToOther();
        if (start.getBinding() instanceof GlobalVariable && !((GlobalVariable) start.getBinding()).isIndexedVariable()) {
            ((GlobalVariable) start.getBinding()).setIndexedVariable();
        }
    }

    public IndexedFilterExpression(Expression base, Expression use, Expression search, AtomicComparer comparer, boolean convertUntypedToOther) {
        baseOp = new Operand(this, base, OperandRole.FOCUS_CONTROLLING_SELECT);
        useOp = new Operand(this, use, FilterExpression.FILTER_PREDICATE);
        searchOp = new Operand(this, search, OperandRole.ATOMIC_SEQUENCE);
        this.comparer = comparer;
        this.convertUntypedToOther = convertUntypedToOther;
    }

    /**
     * Get the immediate sub-expressions of this expression, with information about the relationship
     * of each expression to its parent expression. Default implementation
     * works off the results of iterateSubExpressions()
     * <p>If the expression is a Callable, then it is required that the order of the operands
     * returned by this function is the same as the order of arguments supplied to the corresponding
     * call() method.</p>
     *
     * @return an iterator containing the sub-expressions of this expression
     */
    @Override
    public Iterable<Operand> operands() {
        return operandList(baseOp, useOp, searchOp);
    }

    /**
     * Get the data type of the items returned
     *
     * @return an integer representing the data type
     */

    /*@NotNull*/
    @Override
    public ItemType getItemType() {
        return getBaseExpression().getItemType();
    }

    /**
     * Get the underlying expression
     *
     * @return the expression being filtered
     */

    public Expression getBaseExpression() {
        return baseOp.getChildExpression();
    }

    /**
     * Get the expression to be indexed
     *
     * @return the "use" expression, the expression that computes the index key for each item in
     * the population
     */

    public Expression getUseExpression() {
        return useOp.getChildExpression();
    }

    /**
     * Get the expression whose value we will be searching for
     *
     * @return the search expression
     */

    public Expression getSearchExpression() {
        return searchOp.getChildExpression();
    }

    /**
     * Ask whether untyped atomic values need to be converted to the type of the other operand
     * @return true if untyped atomic values are converted; false if they are compared as strings
     */

    public boolean isConvertUntypedToOther() {
        return convertUntypedToOther;
    }

    /**
     * Get the comparer used for the comparisons
     * @return the comparer
     */

    public AtomicComparer getComparer() {
        return comparer;
    }

    /**
     * Type-check the expression
     *
     * @param visitor     an expression visitor
     * @param contextInfo information about the static type of the context item
     * @return the expression after type-checking (potentially modified to add run-time
     * checks and/or conversions)
     */

    /*@NotNull*/
    @Override
    public Expression typeCheck(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo) throws XPathException {
        baseOp.typeCheck(visitor, contextInfo);

        ItemType type = getBaseExpression().getItemType();

        ContextItemStaticInfo cisi = visitor.getConfiguration().makeContextItemStaticInfo(type, false);
        useOp.typeCheck(visitor, cisi);
        searchOp.typeCheck(visitor, contextInfo);

        return this;
    }

    /**
     * Perform optimisation of an expression and its subexpressions.
     * <p>This method is called after all references to functions and variables have been resolved
     * to the declaration of the function or variable, and after all type checking has been done.</p>
     *
     * @param visitor         an expression visitor
     * @param contextItemType the static type of "." at the point where this expression is invoked.
     *                        The parameter is set to null if it is known statically that the context item will be undefined.
     *                        If the type of the context item is not known statically, the argument is set to
     *                        {@link net.sf.saxon.type.Type#ITEM_TYPE}
     * @return the original expression, rewritten if appropriate to optimize execution
     */

    /*@NotNull*/
    @Override
    public Expression optimize(ExpressionVisitor visitor, ContextItemStaticInfo contextItemType) throws XPathException {
        // if the variable reference has been inlined, replace by an ordinary filter expression,
        // and pre-evaluate if possible
        if (!(getBaseExpression() instanceof VariableReference)) {
            if (ExpressionTool.dependsOnFocus(getSearchExpression())) {
                LetExpression let = new LetExpression();
                let.setVariableQName(new StructuredQName("vv", NamespaceUri.SAXON_GENERATED_VARIABLE, "vv" + computeHashCode()));
                let.setSequence(getSearchExpression());
                let.setRequiredType(getSearchExpression().getStaticType());
                Expression filter = new GeneralEqualityEE(getUseExpression(), OperatorSymbol.EQUALS, new LocalVariableReference(let));
                filter = filter.typeCheck(visitor, contextItemType);
                let.setAction(new FilterExpression(getBaseExpression(), filter));
                ExpressionTool.copyLocationInfo(this, let);
                return let.optimize(visitor, contextItemType);
            } else {
                Expression filter = new GeneralEqualityEE(getUseExpression(), OperatorSymbol.EQUALS, getSearchExpression());
                ItemType type = getBaseExpression().getItemType();
                ContextItemStaticInfo cisi = visitor.getConfiguration().makeContextItemStaticInfo(type, false);
                filter = filter.typeCheck(visitor, cisi);
                Expression e2 = new FilterExpression(getBaseExpression(), filter);
                return e2.optimize(visitor, contextItemType);
            }
        }
        return this;
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
     * @param rebindings variables that need to be re-bound
     * @return the copy of the original expression
     */

    /*@NotNull*/
    @Override
    public Expression copy(RebindingMap rebindings) {
        IndexedFilterExpression ife = new IndexedFilterExpression(
                getBaseExpression().copy(rebindings),
                getUseExpression().copy(rebindings),
                getSearchExpression().copy(rebindings),
                comparer,
                convertUntypedToOther);
        ExpressionTool.copyLocationInfo(this, ife);
        return ife;
    }

    /**
     * Get the static cardinality of this expression
     *
     * @return the cardinality. The method attempts to determine the case where the
     * filter predicate is guaranteed to select at most one item from the sequence being filtered
     */

    @Override
    protected int computeCardinality() {
        return StaticProperty.ALLOWS_ZERO_OR_MORE;
    }

    /**
     * Get the static properties of this expression (other than its type). The result is
     * bit-significant. These properties are used for optimizations. In general, if
     * property bit is set, it is true, but if it is unset, the value is unknown.
     *
     * @return the static properties of the expression, as a bit-significant value
     */

    @Override
    protected int computeSpecialProperties() {
        return getBaseExpression().getSpecialProperties();
    }

    /**
     * Add a representation of this expression to a PathMap. The PathMap captures a map of the nodes visited
     * by an expression in a source tree.
     *
     * @param pathMap        the PathMap to which the expression should be added
     * @param pathMapNodeSet the PathMapNodeSet to which the paths embodied in this expression should be added
     * @return the pathMapNode representing the focus established by this expression, in the case where this
     * expression is the first operand of a path expression or filter expression
     */

    @Override
    public PathMap.PathMapNodeSet addToPathMap(PathMap pathMap, PathMap.PathMapNodeSet pathMapNodeSet) {
        PathMap.PathMapNodeSet target = getBaseExpression().addToPathMap(pathMap, pathMapNodeSet);
        getUseExpression().addToPathMap(pathMap, target);
        getSearchExpression().addToPathMap(pathMap, target);
        return target;
    }


    /**
     * Is this expression the same as another expression?
     *
     * @param other the expression to be compared with this one
     * @return true if the two expressions are statically equivalent
     */

    public boolean equals(Object other) {
        if (other instanceof IndexedFilterExpression) {
            IndexedFilterExpression f = (IndexedFilterExpression) other;
            return getBaseExpression().isEqual(f.getBaseExpression()) &&
                    getUseExpression().isEqual(f.getUseExpression()) &&
                    getSearchExpression().isEqual(f.getSearchExpression()) &&
                    comparer.equals(f.comparer) &&
                    convertUntypedToOther == f.convertUntypedToOther;
        }
        return false;
    }

    /**
     * get HashCode for comparing two expressions
     *
     * @return the hash code
     */

    @Override
    protected int computeHashCode() {
        return "IndexedFilterExpression".hashCode() +
                getBaseExpression().hashCode() ^
                getUseExpression().hashCode() ^
                getSearchExpression().hashCode();
    }


    /**
     * Iterate over the results, returning them in the correct order
     *
     * @param context the dynamic context for the evaluation
     * @return an iterator over the expression results
     * @throws XPathException if any dynamic error occurs
     */

    /*@NotNull*/
    @Override
    public SequenceIterator iterate(XPathContext context) throws XPathException {
        return makeElaborator().elaborateForPull().iterate(context);
//        Expression baseExp = getBaseExpression();
//        SearchableValue indexedValue;
//        if (baseExp instanceof VariableReference) {
//            Sequence base = ((VariableReference) baseExp).evaluateVariable(context);
//            if (base instanceof EmptySequence) {
//                return EmptyIterator.emptyIterator();
//            } else if (base instanceof SearchableValue) {
//                indexedValue = (SearchableValue) base;
//            } else {
//                indexedValue = SearchableValue.makeSearchableValue(base.iterate());
//                Binding binding = ((VariableReference) baseExp).getBinding();
//                if (binding instanceof LocalBinding) {
//                    context.setLocalVariable(((LocalBinding) binding).getLocalSlotNumber(), indexedValue);
//                } else if (binding instanceof GlobalVariable) {
//                    context.getController().getBindery(((GlobalVariable) binding).getPackageData())
//                            .setGlobalVariable((GlobalVariable) binding, indexedValue);
//                }
//            }
//        } else {
//            indexedValue = SearchableValue.makeSearchableValue(basePullEvaluator.iterate(context));
//        }
//        // In all other cases there must have been some kind of rewrite; evaluate as a regular
//        // filter expression
//
//        if (indexedValue.getLength() == 0) {
//            return EmptyIterator.emptyIterator();
//        }
//        AtomicComparer comparer = this.comparer.provideContext(context);
//        if (searchPullEvaluator == null) {
//            searchPullEvaluator = getSearchExpression().makeElaborator().elaborateForPull();
//        }
//        SequenceIterator values = searchPullEvaluator.iterate(context);
//        return indexedValue.findItems(getUseExpression(), values, convertUntypedToOther,
//                                      false, comparer, context);

    }

    /**
     * Determine which aspects of the context the expression depends on. The result is
     * a bitwise-or'ed value composed from constants such as XPathContext.VARIABLES and
     * XPathContext.CURRENT_NODE
     *
     * @return the dependencies
     */

    @Override
    public int computeDependencies() {
        // not all dependencies in the filter expression matter, because the context node,
        // position, and size are not dependent on the outer context.
        int b = getBaseExpression().getDependencies() | getSearchExpression().getDependencies();
        int f = getUseExpression().getDependencies();
        return b | (f & (StaticProperty.DEPENDS_ON_XSLT_CONTEXT |
                                 StaticProperty.DEPENDS_ON_LOCAL_VARIABLES |
                                 StaticProperty.DEPENDS_ON_USER_FUNCTIONS));
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
        return "indexedFilter";
    }


    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     */

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        out.startElement("indexedFilter2", this);
        //out.startElement("indexedFilter", this);
        out.emitAttribute("comp", comparer.save());
        if (convertUntypedToOther) {
            out.emitAttribute("flags", "u");
        }
        out.setChildRole("base");
        getBaseExpression().export(out);
        out.setChildRole("use");
        getUseExpression().export(out);
        out.setChildRole("search");
        getSearchExpression().export(out);
        out.endElement();
    }


    @Override
    public String toShortString() {
        return getBaseExpression().toShortString() + "[(:indexed:)" +
                getUseExpression().toShortString() + "=" + getSearchExpression().toShortString() + "]";
    }

    /**
     * Make an elaborator for this expression
     *
     * @return an appropriate {@link Elaborator}
     */
    @Override
    public Elaborator getElaborator() {
        return new IndexedFilterExpressionElaborator();
    }


    private static class IndexedFilterExpressionElaborator extends PullElaborator {

        @Override
        public PullEvaluator elaborateForPull() {
            IndexedFilterExpression expr = (IndexedFilterExpression) getExpression();
            Expression baseExp = expr.getBaseExpression();
            PullEvaluator basePullEvaluator = baseExp.makeElaborator().elaborateForPull();
            PullEvaluator searchPullEvaluator = expr.getSearchExpression().makeElaborator().elaborateForPull();
            return context -> {
                SearchableValue indexedValue;
                if (baseExp instanceof VariableReference) {
                    Sequence base = ((VariableReference) baseExp).evaluateVariable(context);
                    if (base instanceof EmptySequence) {
                        return EmptyIterator.getInstance();
                    } else if (base instanceof SearchableValue) {
                        indexedValue = (SearchableValue) base;
                    } else {
                        indexedValue = SearchableValue.makeSearchableValue(base.iterate());
                        Binding binding = ((VariableReference) baseExp).getBinding();
                        if (binding instanceof LocalBinding) {
                            context.setLocalVariable(((LocalBinding) binding).getLocalSlotNumber(), indexedValue);
                        } else if (binding instanceof GlobalVariable) {
                            context.getController().getBindery(((GlobalVariable) binding).getPackageData())
                                    .setGlobalVariable((GlobalVariable) binding, indexedValue);
                        }
                    }
                } else {
                    indexedValue = SearchableValue.makeSearchableValue(basePullEvaluator.iterate(context));
                }
                // In all other cases there must have been some kind of rewrite; evaluate as a regular
                // filter expression

                if (indexedValue.getLength() == 0) {
                    return EmptyIterator.getInstance();
                }
                AtomicComparer comparer = expr.comparer.provideContext(context);
                SequenceIterator values = searchPullEvaluator.iterate(context);
                return indexedValue.findItems(expr.getUseExpression(), values, expr.convertUntypedToOther,
                                              comparer, context);

            };
        }

    }
}


// Copyright (c) 2006-2023 Saxonica Limited