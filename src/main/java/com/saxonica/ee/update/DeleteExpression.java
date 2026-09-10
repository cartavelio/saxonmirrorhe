package com.saxonica.ee.update;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.elab.Elaborator;
import net.sf.saxon.expr.elab.PullEvaluator;
import net.sf.saxon.expr.elab.UpdateElaborator;
import net.sf.saxon.expr.elab.UpdateEvaluator;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.SequenceType;

import java.util.function.Supplier;

/**
 * A delete expression in XQuery Update
 */
public class DeleteExpression extends BasicUpdatingExpression {

    private final Operand operand;

    /**
     * Create a DeleteExpression
     *
     * @param target expression denoting the nodes to be deleted
     */

    public DeleteExpression(Expression target) {
        operand = new Operand(this, target, OperandRole.INSPECT);
    }

    @Override
    public Iterable<Operand> operands() {
        return operandList(operand);
    }

    public Expression getTargetExp() {
        return operand.getChildExpression();
    }

    public void setTargetExp(Expression targetExp) {
        operand.setChildExpression(targetExp);
    }

    /**
     * Perform type checking of an expression and its subexpressions. This is the second phase of
     * static optimization.
     * <p>This checks statically that the operands of the expression have
     * the correct type; if necessary it generates code to do run-time type checking or type
     * conversion. A static type error is reported only if execution cannot possibly succeed, that
     * is, if a run-time type error is inevitable. The call may return a modified form of the expression.</p>
     * <p>This method is called after all references to functions and variables have been resolved
     * to the declaration of the function or variable. However, the types of such functions and
     * variables may not be accurately known if they have not been explicitly declared.</p>
     * <p>If the implementation returns a value other than "this", then it is required to ensure that
     * the parent pointer and location information in the returned expression have been set up correctly.
     * It should not rely on the caller to do this, although for historical reasons many callers do so.</p>
     *
     *
     * @param visitor         an expression visitor
     * @param contextInfo     information about the dynamic context item
     * @return the original expression, rewritten to perform necessary run-time type checks,
     *         and to perform other type-related optimizations
     * @throws net.sf.saxon.trans.XPathException
     *          if an error is discovered during this phase
     *          (typically a type error)
     */

    /*@NotNull*/
    @Override
    public Expression typeCheck(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo) throws XPathException {
        typeCheckChildren(visitor, contextInfo);

        // The targetExpression operand must be of type node()*

        Supplier<RoleDiagnostic> roleSupplier = () -> {
            RoleDiagnostic role0 = new RoleDiagnostic(RoleDiagnostic.UPDATING_EXPR, "delete", 0);
            role0.setErrorCode("XUTY0007");
            return role0;
        };

        setTargetExp(visitor.getConfiguration().getTypeChecker(false).staticTypeCheck(
                getTargetExp(), SequenceType.NODE_SEQUENCE, roleSupplier, visitor));
        return this;
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
        DeleteExpression d2 = new DeleteExpression(getTargetExp().copy(rebindings));
        ExpressionTool.copyLocationInfo(this, d2);
        return d2;
    }

    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     *
     * @param out the expression presenter used to display the structure
     */

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        out.startElement("delete", this);
        getTargetExp().export(out);
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
        return "delete";
    }

    /**
     * Make an elaborator for this expression
     *
     * @return an appropriate {@link Elaborator}
     */
    @Override
    public Elaborator getElaborator() {
        return new DeleteExpressionElaborator();
    }

    private static class DeleteExpressionElaborator extends UpdateElaborator {

        @Override
        public UpdateEvaluator elaborateForUpdate() {
            DeleteExpression expr = (DeleteExpression) getExpression();
            PullEvaluator targetPull = expr.getTargetExp().makeElaborator().elaborateForPull();
            return (context, pul) -> {
                try {
                    SequenceIterator iter = targetPull.iterate(context);
                    NodeInfo node;
                    while ((node = (NodeInfo) iter.next()) != null) {
                        if (!(node.getTreeInfo().isMutable())) {
                            throw new XPathException("Node to be deleted is not an updatable node",
                                                                    SaxonErrorCode.SXUP0081)
                                    .withLocation(expr.getLocation());
                        }
                        if (node.getParent() == null) {
                            // now a no-op - see spec bug 5702
                        } else {
                            try {
                                DeleteAction action = new DeleteAction(node);
                                action.setOriginator(expr);
                                ((PendingUpdateListImpl) pul).add(action);
                            } catch (XPathException e) {
                                throw e.maybeWithLocation(expr.getLocation()).maybeWithContext(context);
                            }
                        }
                    }
                } catch (UncheckedXPathException e) {
                    throw e.getXPathException();
                }
            };
        }
    }
}

// Copyright (c) 2008-2023 Saxonica Limited