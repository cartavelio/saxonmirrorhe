package com.saxonica.ee.update;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.elab.*;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.*;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;

import java.util.HashSet;
import java.util.Set;

/**
 * A copy-modify expression (called a "transform" expression in the XQuery Update specification)
 *
 * <p>Note that this expression doesn't actually perform the copy. It gets wrapped in a let expression
 * that does let $var := $input.copy-of().</p>
 */
public class CopyModifyExpression extends Expression {

    private final int validationMode;
    private final Operand updateOp;
    private final Operand returnOp;
    private final Operand copyBindingsOp;

    /**
     * Create a copy-modify expression (also known as a transform expresion)
     *
     * @param updateExp      the expression appearing in the modify clause
     * @param returnExp      the expression appearing in the return clause
     * @param validationMode the mode for validating the updated nodes (strict, lax, or skip)
     */

    public CopyModifyExpression(Expression updateExp, Expression returnExp, int validationMode) {
        updateOp = new Operand(this, updateExp, OperandRole.SINGLE_ATOMIC);
        returnOp = new Operand(this, returnExp, OperandRole.SAME_FOCUS_ACTION);
        copyBindingsOp = new Operand(this, returnExp, OperandRole.SINGLE_ATOMIC); // temporary
        this.validationMode = validationMode;
    }

     @Override
     public Iterable<Operand> operands() {
         return operandList(updateOp, returnOp, copyBindingsOp);
     }


    public Expression getUpdateExp() {
        return updateOp.getChildExpression();
    }

    public void setUpdateExp(Expression updateExp) {
        updateOp.setChildExpression(updateExp);
    }

    public Expression getReturnExp() {
        return returnOp.getChildExpression();
    }

    public void setReturnExp(Expression returnExp) {
        returnOp.setChildExpression(returnExp);
    }

    public Expression getCopyBindings() {
        return copyBindingsOp.getChildExpression();
    }

    /**
     * Set the copy bindings. This is an expression whose value is the set of nodes that are allowed to be
     * updated. Normally it will consist of a variable reference or a sequence of variable references.
     *
     * @param copyBindings the set of Let expressions that bind variables that were copied during the copy
     *                     phase and that therefore may be updated during the modify phase
     */

    public void setCopyBindings(Expression copyBindings) {
        copyBindingsOp.setChildExpression(copyBindings);
    }

    /**
     * Determine the static cardinality of the expression. This establishes how many items
     * there will be in the result of the expression, at compile time (i.e., without
     * actually evaluating the result.
     *
     * @return one of the values Cardinality.ONE_OR_MORE,
     *         Cardinality.ZERO_OR_MORE, Cardinality.EXACTLY_ONE,
     *         Cardinality.ZERO_OR_ONE, Cardinality.EMPTY. This default
     *         implementation returns ZERO_OR_MORE (which effectively gives no
     *         information).
     */

    @Override
    protected int computeCardinality() {
        return getReturnExp().getCardinality();
    }

    /**
     * Determine the data type of the expression, if possible. All expression return
     * sequences, in general; this method determines the type of the items within the
     * sequence, assuming that (a) this is known in advance, and (b) it is the same for
     * all items in the sequence.
     * <p>This method should always return a result, though it may be the best approximation
     * that is available at the time.</p>
     *
     * @return a value such as Type.STRING, Type.BOOLEAN, Type.NUMBER,
     *         Type.NODE, or Type.ITEM (meaning not known at compile time)
     */

    /*@NotNull*/
    @Override
    public ItemType getItemType() {
        return getReturnExp().getItemType();
    }

    /**
     * Determine whether this is an updating expression as defined in the XQuery update specification
     *
     * @return false: although this expression performs updates, it is not classified as an updating
     *         expression
     */

    @Override
    public boolean isUpdatingExpression() {
        return false;
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
        return "copy-modify";
    }

    /**
     * Check to ensure that this expression does not contain any inappropriate updating subexpressions.
     * This check is overridden for those expressions that permit updating subexpressions.
     *
     * @throws net.sf.saxon.trans.XPathException
     *          if the expression has a non-permitted updating subexpression
     */

    @Override
    public void checkForUpdatingSubexpressions() throws XPathException {
        if (ExpressionTool.isNotAllowedInUpdatingContext(getUpdateExp())) {
            throw new XPathException("Expression in modify clause must be an updating expression (or () or error())",
                    "XUST0002");
        }
        if (getReturnExp().isUpdatingExpression()) {
            throw new XPathException("Expression in return clause must not be an updating expression",
                    "XUST0001");
        }
    }

    /**
     * Copy an expression. This makes a deep copy.
     *
     * @return the copy of the original expression
     * @param rebindings variables to be rebound
     */

    /*@NotNull*/
    @Override
    public Expression copy(RebindingMap rebindings) {
        CopyModifyExpression exp =
                new CopyModifyExpression(getUpdateExp().copy(rebindings), getReturnExp().copy(rebindings), validationMode);
        ExpressionTool.copyLocationInfo(this, exp);
        exp.setCopyBindings(getCopyBindings().copy(rebindings));
        return exp;
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
        return EVALUATE_METHOD | ITERATE_METHOD | PROCESS_METHOD;
    }

    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     *
     * @param out the expression presenter used to display the structure
     */

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        out.startElement("copyModify", this);
        out.emitAttribute("val", Validation.describe(validationMode));
        out.setChildRole("bindings");
        getCopyBindings().export(out);
        out.setChildRole("modify");
        getUpdateExp().export(out);
        out.setChildRole("return");
        getReturnExp().export(out);
        out.endElement();
    }

    /**
     * Return an Iterator to iterate over the values of a sequence. The value of every
     * expression can be regarded as a sequence, so this method is supported for all
     * expressions. This default implementation handles iteration for expressions that
     * return singleton values: for non-singleton expressions, the subclass must
     * provide its own implementation.
     *
     * @param context supplies the context for evaluation
     * @return a SequenceIterator that can be used to iterate over the result
     *         of the expression
     * @throws net.sf.saxon.trans.XPathException
     *          if any dynamic error occurs evaluating the
     *          expression
     */

    /*@NotNull*/
    @Override
    public SequenceIterator iterate(XPathContext context) throws XPathException {
        return makeElaborator().elaborateForPull().iterate(context);
    }


    /**
     * Evaluate an expression as a single item. This always returns either a single Item or
     * null (denoting the empty sequence). No conversion is done. This method should not be
     * used unless the static type of the expression is a subtype of "item" or "item?": that is,
     * it should not be called if the expression may return a sequence. There is no guarantee that
     * this condition will be detected.
     *
     * @param context The context in which the expression is to be evaluated
     * @return the node or atomic value that results from evaluating the
     *         expression; or null to indicate that the result is an empty
     *         sequence
     * @throws net.sf.saxon.trans.XPathException
     *          if any dynamic error occurs evaluating the
     *          expression
     */

    @Override
    public Item evaluateItem(XPathContext context) throws XPathException {
        return makeElaborator().elaborateForItem().eval(context);
    }


    /**
     * Process the instruction, without returning any tail calls
     *
     * @param output the destination for the result
     * @param context The dynamic context, giving access to the current node,
     */

    @Override
    public void process(Outputter output, XPathContext context) throws XPathException {
        dispatchTailCall(makeElaborator().elaborateForPush().processLeavingTail(output, context));
    }

    private void doUpdates(XPathContext context) throws XPathException {
        PendingUpdateList pul = context.getConfiguration().newPendingUpdateList();
        getUpdateExp().makeElaborator().elaborateForUpdate().registerUpdates(context, pul);
        pul.apply(context, validationMode);
        // The set of updated trees must be a subset of the set of nodes created during the copy phase
        Set<NodeInfo> copiedNodes = new HashSet<>();
        SequenceTool.supply(getCopyBindings().iterate(context), (ItemConsumer<? super Item>) node -> copiedNodes.add((NodeInfo) node));
        if (!copiedNodes.containsAll(pul.getAffectedTrees())) {
            XPathException err = new XPathException(
                    "A node that was not created during the copy phase has been updated during the modify phase");
            err.setErrorCode("XUDY0014");
            err.setLocation(getLocation());
            err.setXPathContext(context);
            throw err;
        }
    }

    /**
     * Make an elaborator for this expression
     *
     * @return an appropriate {@link Elaborator}
     */
    @Override
    public Elaborator getElaborator() {
        return new CopyModifyExpressionElaborator();
    }

    private static class CopyModifyExpressionElaborator extends PushElaborator {

        @Override
        public PushEvaluator elaborateForPush() {
            CopyModifyExpression expr = (CopyModifyExpression) getExpression();
            PushEvaluator returnEval = expr.getReturnExp().makeElaborator().elaborateForPush();
            return (output, context) -> {
                expr.doUpdates(context);
                return returnEval.processLeavingTail(output, context);
            };
        }

        @Override
        public ItemEvaluator elaborateForItem() {
            CopyModifyExpression expr = (CopyModifyExpression) getExpression();
            ItemEvaluator returnEval = expr.getReturnExp().makeElaborator().elaborateForItem();
            return context -> {
                expr.doUpdates(context);
                return returnEval.eval(context);
            };
        }

        @Override
        public PullEvaluator elaborateForPull() {
            CopyModifyExpression expr = (CopyModifyExpression) getExpression();
            PullEvaluator returnEval = expr.getReturnExp().makeElaborator().elaborateForPull();
            return context -> {
                expr.doUpdates(context);
                return returnEval.iterate(context);
            };
        }
    }
}

// Copyright (c) 2008-2022 Saxonica Limited