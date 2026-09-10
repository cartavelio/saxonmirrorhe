package com.saxonica.ee.update;

import com.saxonica.ee.validate.SkipValidator;
import net.sf.saxon.Controller;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.elab.*;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.*;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.linked.NodeImpl;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.Type;
import net.sf.saxon.type.Untyped;
import net.sf.saxon.value.SequenceType;

import java.util.ArrayList;
import java.util.List;
import java.util.function.Supplier;

/**
 * A "replace node" expression in XQuery Update
 */
public class ReplaceNodeExpression extends BasicUpdatingExpression {

    private final Operand targetOp;
    private final Operand replacementOp;
    private final int constructionMode;
    private final boolean inheritNamespaces;

    /**
     * Create a ReplaceNodeExpression
     *
     * @param target           expression that computes the node to be replaced
     * @param replacement      expression that supplies the replacement node(s)
     * @param constructionMode set to Validation.STRIP or Validation.PRESERVE - indicates whether the
     *                         replacement nodes retain their type annotation
     * @param inheritNamespaces true if namespaces mode is inherit
     */

    public ReplaceNodeExpression(Expression target, Expression replacement,
                                 int constructionMode, boolean inheritNamespaces) {
        targetOp = new Operand(this, target, OperandRole.INSPECT);
        replacementOp = new Operand(this, replacement, OperandRole.SINGLE_ATOMIC);
        this.constructionMode = constructionMode;
        this.inheritNamespaces = inheritNamespaces;
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
     *
     * @param visitor         an expression visitor
     * @param contextInfo     static information about the context item
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
        ReplaceNodeExpression exp =
                new ReplaceNodeExpression(getTargetExp().copy(rebindings), getReplacementExp().copy(rebindings),
                                          constructionMode, inheritNamespaces);
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
        out.startElement("replaceNodes", this);
        out.emitAttribute("constructionMode", constructionMode+"");
        if (inheritNamespaces) {
            out.emitAttribute("flags", "i");
        }
        getTargetExp().export(out);
        getReplacementExp().export(out);
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
        return "replaceNode";
    }

    /**
     * Make an elaborator for this expression
     *
     * @return an appropriate {@link Elaborator}
     */
    @Override
    public Elaborator getElaborator() {
        return new ReplaceNodeExpressionElaborator();
    }

    private static class ReplaceNodeExpressionElaborator extends UpdateElaborator {

        @Override
        public UpdateEvaluator elaborateForUpdate() {
            ReplaceNodeExpression expr = (ReplaceNodeExpression) getExpression();
            ItemEvaluator targetEval = expr.getTargetExp().makeElaborator().elaborateForItem();
            PullEvaluator replacementEval = expr.getReplacementExp().makeElaborator().elaborateForPull();
            return (context, pul) -> {
                List<Item> content = new ArrayList<>();
                NodeInfo target = (NodeInfo) targetEval.eval(context);
                if (target == null) {
                    XPathException err = new XPathException("Target of replace expression must not be empty", "XUDY0027");
                    err.setLocation(expr.getLocation());
                    throw err;
                }
                if (!target.getTreeInfo().isMutable()) {
                    throw new XPathException("Attempting to update a non-updatable tree", SaxonErrorCode.SXUP0081);
                }
                int kind = target.getNodeKind();
                if (kind == Type.NAMESPACE) {
                    XPathException err = new XPathException(
                            "Target of replace expression must not be a namespace node", "XUTY0008");
                    err.setLocation(expr.getLocation());
                    throw err;
                }
                if (kind == Type.DOCUMENT) {
                    XPathException err = new XPathException(
                            "Target of replace expression must not be a document node", "XUTY0008");
                    err.setLocation(expr.getLocation());
                    throw err;
                }
                NodeInfo parent = target.getParent();
                if (parent == null) {
                    XPathException err = new XPathException("Target node of replace expression has no parent",
                                                            "XUDY0009");
                    err.setLocation(expr.getLocation());
                    throw err;
                }
                if (!(parent instanceof MutableNodeInfo)) {
                    XPathException err = new XPathException("Target node of replace expression is not updatable",
                                                            SaxonErrorCode.SXUP0081);
                    err.setLocation(expr.getLocation());
                    throw err;
                }
                if (kind == Type.ATTRIBUTE) {
                    SequenceIterator iter = replacementEval.iterate(context);
                    AttributeMap atts = EmptyAttributeMap.getInstance();
                    for (Item item; (item = iter.next()) != null; ) {
                        boolean isAttribute = item instanceof NodeInfo && ((NodeInfo) item).getNodeKind() == Type.ATTRIBUTE;
                        if (!isAttribute) {
                            XPathException err = new XPathException(
                                    "The replacement nodes for an attribute must all be attributes",
                                    "XUTY0011");
                            err.setLocation(expr.getLocation());
                            throw err;
                        }

                        NodeInfo att = (NodeInfo) item;
                        SimpleType attributeType;
                        int properties = ReceiverOption.NONE;
                        if (expr.constructionMode == Validation.SKIP || Untyped.getInstance().equals(parent.getSchemaType())) {
                            attributeType = BuiltInAtomicType.UNTYPED_ATOMIC;
                        } else {
                            attributeType = (SimpleType) att.getSchemaType();
                            if (att.isId() || (att.getLocalPart().equals("id") &&
                                                       att.getNamespaceUri().equals(NamespaceUri.XML))) {
                                properties |= (ReceiverOption.IS_ID);
                            }
                            if (att.isIdref()) {
                                properties |= (ReceiverOption.IS_IDREF);
                            }
                        }

                        atts = atts.put(new AttributeInfo(NameOfNode.makeName(att), attributeType, att.getStringValue(),
                                                          Loc.NONE, properties));
                    }

                    ReplaceAttributeAction action = new ReplaceAttributeAction(parent, target, atts, expr.inheritNamespaces);
                    action.setOriginator(expr);
                    ((PendingUpdateListImpl) pul).add(action);

                } else {

                    SequenceIterator iter = replacementEval.iterate(context);
                    for (Item item; (item = iter.next()) != null; ) {
                        if (item instanceof NodeInfo && ((NodeInfo) item).getNodeKind() == Type.ATTRIBUTE) {
                            XPathException err = new XPathException(
                                    "An attribute node cannot replace a node that is not itself an attribute",
                                    "XUTY0010");
                            err.setLocation(expr.getLocation());
                            throw err;
                        } else {
                            content.add(item);
                        }
                    }
                    if (content.isEmpty()) {
                        try {
                            ReplaceNodeAction action = new ReplaceNodeAction(
                                    new NodeImpl[]{}, (MutableNodeInfo) target, expr.inheritNamespaces);
                            action.setOriginator(expr);
                            ((PendingUpdateListImpl) pul).add(action);
                        } catch (XPathException e) {
                            e.maybeSetLocation(expr.getLocation());
                            e.maybeSetContext(context);
                            throw e;
                        }
                    } else {
                        // Make copies of the inserted nodes, and convert them to use the same object model implementation
                        // as the node into which they are being inserted
                        Builder builder = ((MutableNodeInfo) parent).newBuilder();
                        Controller controller = context.getController();
                        builder.setLineNumbering(controller.getConfiguration().isLineNumbering());

                        PipelineConfiguration pipe = controller.makePipelineConfiguration();
                        pipe.setXPathContext(context);
                        pipe.setHostLanguage(expr.getPackageData().getHostLanguage());
                        builder.setPipelineConfiguration(pipe);

                        Receiver r = builder;

                        if (expr.constructionMode == Validation.STRIP || Untyped.getInstance().equals(parent.getSchemaType())) {
                            r = new SkipValidator(r);
                        }

                        TreeReceiver out = new TreeReceiver(r);
                        out.setPipelineConfiguration(pipe);

                        out.open();
                        out.startDocument(ReceiverOption.NONE);

                        for (Object aContent : content) {
                            Item n = (Item) aContent;
                            out.append(n, Loc.NONE, ReceiverOption.ALL_NAMESPACES);
                        }

                        out.endDocument();
                        out.close();

                        NodeInfo root = builder.getCurrentRoot();
                        AxisIterator top = root.iterateAxis(AxisInfo.CHILD);
                        GroundedValue extent = SequenceTool.toGroundedValue(top);
                        int len = extent.getLength();
                        NodeInfo[] array = new NodeInfo[len];
                        for (int i = 0; i < len; i++) {
                            array[i] = (NodeInfo) extent.itemAt(i);
                        }
                        try {
                            ReplaceNodeAction action = new ReplaceNodeAction(array, (MutableNodeInfo) target, expr.inheritNamespaces);
                            action.setOriginator(expr);
                            ((PendingUpdateListImpl) pul).add(action);
                        } catch (XPathException e) {
                            e.maybeSetLocation(expr.getLocation());
                            e.maybeSetContext(context);
                            throw e;
                        }
                    }
                }
            };
        }
    }
}

// Copyright (c) 2008-2023 Saxonica Limited