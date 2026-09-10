package com.saxonica.ee.update;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.elab.Elaborator;
import net.sf.saxon.expr.elab.ItemEvaluator;
import net.sf.saxon.expr.elab.UpdateElaborator;
import net.sf.saxon.expr.elab.UpdateEvaluator;
import net.sf.saxon.expr.instruct.ValueOf;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.MutableNodeInfo;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.query.XQueryParser;
import net.sf.saxon.str.StringView;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.SequenceType;

import java.util.function.Supplier;

/**
 * A "replace value of" expression in XQuery Update
 */
public class ReplaceValueExpression extends BasicUpdatingExpression {

    private final Operand targetOp;
    private final Operand replacementOp;

    /**
     * Create a ReplaceValue expression
     *
     * @param target      expression that computes the node(s) whose value is to be replaced
     * @param replacement expression that computes the replacement value
     */

    public ReplaceValueExpression(Expression target, Expression replacement) {
        targetOp = new Operand(this, target, OperandRole.INSPECT);
        replacementOp = new Operand(this, replacement, OperandRole.SINGLE_ATOMIC);
    }

    public Expression getTargetExp() {
        return targetOp.getChildExpression();
    }

    public void setTargetExp(Expression targetExpression) {
        targetOp.setChildExpression(targetExpression);
    }

    public Expression getReplacementExp() {
        return replacementOp.getChildExpression();
    }

    public void setReplacementExp(Expression replacement) {
        replacementOp.setChildExpression(replacement);
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
        return operandList(targetOp, replacementOp);
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
     * @param visitor     an expression visitor
     * @param contextInfo static information about the context item
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

        // The replacementExpression can be of any type (atomic values get turned into text nodes)

        Expression result1 = getReplacementExp();
        setReplacementExp(XQueryParser.stringify(result1, true, visitor.getStaticContext()));
        Expression result = getReplacementExp();
        setReplacementExp(new ValueOf(result, false, true));

        // The targetExpression operand must be of type node(). But there is a different error code
        // for the empty-sequence case, so we do two separate type check operations

        TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);

        Supplier<RoleDiagnostic> roleSupplier0 = () -> {
            RoleDiagnostic role0 = new RoleDiagnostic(RoleDiagnostic.UPDATING_EXPR, "replace", 0);
            role0.setErrorCode("XUDY0027");
            return role0;
        };
        setTargetExp(tc.staticTypeCheck(getTargetExp(), SequenceType.NON_EMPTY_SEQUENCE, roleSupplier0, visitor));

        Supplier<RoleDiagnostic> roleSupplier1 = () -> {
            RoleDiagnostic role0 = new RoleDiagnostic(RoleDiagnostic.UPDATING_EXPR, "replace", 0);
            role0.setErrorCode("XUTY0008");
            return role0;
        };

        setTargetExp(tc.staticTypeCheck(getTargetExp(), SequenceType.OPTIONAL_NODE, roleSupplier1, visitor));
        return this;

    }

    /**
     * Copy an expression. This makes a deep copy.
     *
     * @return the copy of the original expression
     * @param rebindings variables to be re-bound
     */

    /*@NotNull*/
    @Override
    public Expression copy(RebindingMap rebindings) {
        Expression result = getReplacementExp();
        ReplaceValueExpression exp = new ReplaceValueExpression(getTargetExp().copy(rebindings), result.copy(rebindings));
        ExpressionTool.copyLocationInfo(this, exp);
        return exp;
    }

    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     *
     * @param out the expression presenter used to display the structure
     */

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        out.startElement("replaceValue", this);
        getTargetExp().export(out);
        Expression result = getReplacementExp();
        result.export(out);
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
        return "replaceValue";
    }

    /**
     * Make an elaborator for this expression
     *
     * @return an appropriate {@link Elaborator}
     */
    @Override
    public Elaborator getElaborator() {
        return new ReplaceValueExpressionElaborator();
    }

    private static class ReplaceValueExpressionElaborator extends UpdateElaborator {

        @Override
        public UpdateEvaluator elaborateForUpdate() {
            ReplaceValueExpression expr = (ReplaceValueExpression) getExpression();
            ItemEvaluator targetEval = expr.getTargetExp().makeElaborator().elaborateForItem();
            ItemEvaluator replacementEval = expr.getReplacementExp().makeElaborator().elaborateForItem();
            return (context, pul) -> {
                Item newText = replacementEval.eval(context);
                String newString = newText == null ? "" : newText.getStringValue();
                NodeInfo targetNode = (NodeInfo) targetEval.eval(context);
                if (!targetNode.getTreeInfo().isMutable()) {
                    throw new XPathException("Target of replace value expression is not updatable", SaxonErrorCode.SXUP0081);
                }
                XPathException err = null;
                switch (targetNode.getNodeKind()) {
                    case Type.COMMENT:
                        if (newString.contains("--")) {
                            err = new XPathException(
                                    "Replacement comment value cannot contain two adjacent hyphens", "XQDY0072");
                        } else if (newString.endsWith("-")) {
                            err = new XPathException(
                                    "Replacement comment value cannot end with a hyphen", "XQDY0072");
                        }
                        break;
                    case Type.PROCESSING_INSTRUCTION:
                        if (newString.contains("?>")) {
                            err = new XPathException(
                                    "Replacement processing instruction value cannot contain '?>'", "XQDY0026");
                        }
                        break;
                    case Type.DOCUMENT:
                        err = new XPathException(
                                "Cannot replace the value of a document node", "XUTY0008");
                        break;
                    case Type.NAMESPACE:
                        err = new XPathException(
                                "Cannot replace the value of a namespace node", "XUTY0008");
                        break;
                    default:
                        break;

                }
                if (err != null) {
                    err.setLocation(expr.getLocation());
                    err.setXPathContext(context);
                    throw err;
                }
                if (!(targetNode instanceof MutableNodeInfo)) {
                    XPathException er = new XPathException("Target node of replace value expression is not updatable",
                                                           SaxonErrorCode.SXUP0081);
                    er.setLocation(expr.getLocation());
                    throw er;
                }
                try {
                    ReplaceValueAction action = new ReplaceValueAction(StringView.tidy(newString), (MutableNodeInfo) targetNode);
                    action.setOriginator(expr);
                    ((PendingUpdateListImpl) pul).add(action);
                } catch (XPathException e) {
                    e.maybeSetLocation(expr.getLocation());
                    e.maybeSetContext(context);
                    throw e;
                }
            };
        }
    }
}

// Copyright (c) 2008-2023 Saxonica Limited