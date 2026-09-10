package com.saxonica.ee.update;

import com.saxonica.ee.validate.SkipValidator;
import com.saxonica.ee.xtupdate.SaxonInsert;
import net.sf.saxon.Controller;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.*;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AxisIterator;

import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.Type;
import net.sf.saxon.type.Untyped;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.SequenceType;

import java.util.ArrayList;
import java.util.List;
import java.util.function.Supplier;

/**
 * An insert expression in XQuery Update
 */
public class InsertExpression extends BasicUpdatingExpression {

    private final int position;
    private final int constructionMode;
    private final boolean inheritNamespaces;
    private final boolean preserveNamespaces;
    private final Operand sourceOp;
    private final Operand targetOp;


    /**
     * Create an InsertExpression
     *
     * @param source           expression denoting the nodes to be inserted
     * @param target           expression denoting the insertion point
     * @param position         integer code indicating whether to insert before, after, first, last, or into.
     * @param constructionMode one of {@link Validation#PRESERVE} or {@link Validation#STRIP}
     * @param inherit          true if namespaces are to be inherited
     * @param preserve         true if namespaces are to be preserved
     */

    public InsertExpression(Expression source, Expression target, int position,
                            int constructionMode, boolean inherit, boolean preserve) {
        sourceOp = new Operand(this, source, OperandRole.SINGLE_ATOMIC);
        targetOp = new Operand(this, target, OperandRole.INSPECT);
        this.position = position;
        this.constructionMode = constructionMode;
        this.inheritNamespaces = inherit;
        this.preserveNamespaces = preserve;
    }

    public Expression getSourceExpression() {
        return sourceOp.getChildExpression();
    }

    public void setSourceExpression(Expression sourceExpression) {
        sourceOp.setChildExpression(sourceExpression);
    }

    public Expression getTargetExpression() {
        return targetOp.getChildExpression();
    }

    public void setTargetExpression(Expression targetExpression) {
        targetOp.setChildExpression(targetExpression);
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
        return operandList(sourceOp, targetOp);
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
     * @param visitor         an expression visitor
     * @param  contextInfo static information about the dynamic context
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

        // The sourceExpression can be of any type (atomic values get turned into text nodes)

        // The targetExpression operand must be of type node(). But there is a different error code
        // for the empty-sequence case, so we do two separate type check operations

        TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);

        Supplier<RoleDiagnostic> roleSupplier0 = () -> {
            RoleDiagnostic role0 = new RoleDiagnostic(RoleDiagnostic.UPDATING_EXPR, "insert", 1);
            role0.setErrorCode("XUDY0027");
            return role0;
        };
        setTargetExpression(tc.staticTypeCheck(
                getTargetExpression(), SequenceType.NON_EMPTY_SEQUENCE, roleSupplier0, visitor));

        Supplier<RoleDiagnostic> roleSupplier1 = () -> {
            RoleDiagnostic role0 = new RoleDiagnostic(RoleDiagnostic.UPDATING_EXPR, "insert", 1);
            role0.setErrorCode(position == Token.AFTER || position == Token.BEFORE ? "XUTY0006" : "XUTY0005");
            return role0;
        };
        setTargetExpression(tc.staticTypeCheck(
                getTargetExpression(), SequenceType.OPTIONAL_NODE, roleSupplier1, visitor));
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
        InsertExpression exp = new InsertExpression(getSourceExpression().copy(rebindings), getTargetExpression().copy(rebindings),
                                                    position, constructionMode, inheritNamespaces, preserveNamespaces);
        ExpressionTool.copyLocationInfo(this, exp);
        return exp;
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
        return "insert";
    }

    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     *
     * @param out the expression presenter used to display the structure
     */

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        out.startElement("insert", this);
        out.emitAttribute("position", SaxonInsert.encodePosition(position));
        out.emitAttribute("constructionMode", constructionMode + "");
        StringBuilder fsb = new StringBuilder(2);
        if (inheritNamespaces) {
            fsb.append("i");
        }
        if (preserveNamespaces) {
            fsb.append("p");
        }
        out.emitAttribute("flags", fsb.toString());
        getSourceExpression().export(out);
        getTargetExpression().export(out);
        out.endElement();
    }

    /**
     * Evaluate an updating expression, adding the results to a Pending Update List.
     * The default implementation of this method, which is used for non-updating expressions,
     * throws an UnsupportedOperationException
     *
     * @param context the XPath dynamic evaluation context
     * @param pul     the pending update list to which the results should be written
     */

    @Override
    public void evaluatePendingUpdates(XPathContext context, PendingUpdateList pul) throws XPathException {
        List<Item> content = new ArrayList<>();
        NodeInfo target = (NodeInfo) getTargetExpression().evaluateItem(context);
        if (target == null) {
            XPathException e = new XPathException("Target of insert expression must not be empty", "XUDY0027");
            e.setLocation(getLocation());
            e.setXPathContext(context);
            throw e;
        }
        if (!target.getTreeInfo().isMutable()) {
            throw new XPathException("Target of insert expression is not updatable", SaxonErrorCode.SXUP0081);
        }
        int kind = target.getNodeKind();
        if (position == Token.AFTER || position == Token.BEFORE) {
            if (!(kind == Type.ELEMENT || kind == Type.TEXT ||
                    kind == Type.COMMENT || kind == Type.PROCESSING_INSTRUCTION)) {
                XPathException e = new XPathException(
                        "When inserting before or after a node, that node must " +
                                "be an element, text, comment, or processing-instruction node", "XUTY0006");
                e.setLocation(getLocation());
                e.setXPathContext(context);
                throw e;
            }
        } else {
            if (!(kind == Type.ELEMENT || kind == Type.DOCUMENT)) {
                XPathException e = new XPathException(
                        "When inserting into a target node, that node must be an element or document node",
                        "XUTY0005");
                e.setLocation(getLocation());
                e.setXPathContext(context);
                throw e;
            }
        }
        NodeInfo parent = position == Token.AFTER || position == Token.BEFORE ? target.getParent() : target;
        if (parent == null) {
            XPathException err = new XPathException(
                    "When inserting before or after a node, that node must have a parent", "XUDY0029");
            err.setLocation(getLocation());
            err.setXPathContext(context);
            throw err;
        }
        if (!(parent instanceof MutableNodeInfo)) {
            XPathException err = new XPathException("Target node of insert expression is not updateable",
                    SaxonErrorCode.SXUP0081);
            err.setLocation(getLocation());
            err.setXPathContext(context);
            throw err;
        }
        boolean foundNonAttribute = false;
        SequenceIterator iter = getSourceExpression().iterate(context);
        Item item;
        while ((item = iter.next()) != null) {
            if (item instanceof NodeInfo && ((NodeInfo) item).getNodeKind() == Type.ATTRIBUTE) {
                if (foundNonAttribute) {
                    XPathException err = new XPathException(
                            "Attributes must precede non-attributes in the list of nodes to be inserted",
                            "XUTY0004");
                    err.setLocation(getLocation());
                    err.setXPathContext(context);
                    throw err;
                }
                if (parent.getNodeKind() != Type.ELEMENT) {
                    if (position == Token.AFTER || position == Token.BEFORE) {
                        XPathException err = new XPathException(
                                "When inserting an attribute before or after a node, that node must " +
                                        "have a parent and the parent must be an element", "XUDY0030");
                        err.setLocation(getLocation());
                        err.setXPathContext(context);
                        throw err;
                    } else {
                        XPathException err = new XPathException(
                                "When inserting an attribute into a target node, the target node must be an element",
                                "XUTY0022");
                        err.setLocation(getLocation());
                        err.setXPathContext(context);
                        throw err;
                    }
                }
                NodeInfo copiedNode = (NodeInfo) item;

                SimpleType attributeType;
                int properties = ReceiverOption.NONE;
                if (constructionMode == Validation.SKIP || Untyped.getInstance().equals(parent.getSchemaType())) {
                    attributeType = BuiltInAtomicType.UNTYPED_ATOMIC;
                } else {
                    attributeType = (SimpleType) copiedNode.getSchemaType();
                    if (copiedNode.isId() || ("id".equals(copiedNode.getLocalPart()) && NamespaceConstant.XML.equals(copiedNode.getURI()))) {
                        properties |= (ReceiverOption.IS_ID);
                    }
                    if (copiedNode.isIdref()) {
                        properties |= (ReceiverOption.IS_IDREF);
                    }
                }

                try {
                    InsertAttributeAction action = new InsertAttributeAction(
                            (MutableNodeInfo)parent,
                            NameOfNode.makeName(copiedNode),
                            attributeType,
                            copiedNode.getStringValue(),
                            properties,
                            inheritNamespaces);
                    action.setOriginator(this);
                    ((PendingUpdateListImpl) pul).add(action);
                } catch (XPathException e) {
                    e.maybeSetLocation(getLocation());
                    e.maybeSetContext(context);
                    throw e;
                }
            } else {
                foundNonAttribute = true;
                content.add(item);
            }
        }
        if (!content.isEmpty()) {
            // Make copies of the inserted nodes, and convert them to use the same object model implementation
            // as the node into which they are being inserted

            Builder builder = ((MutableNodeInfo) parent).newBuilder();
            Controller controller = context.getController();
            builder.setLineNumbering(controller.getConfiguration().isLineNumbering());

            PipelineConfiguration pipe = controller.makePipelineConfiguration();
            pipe.setHostLanguage(getPackageData().getHostLanguage());
            pipe.setXPathContext(context);
            builder.setPipelineConfiguration(pipe);

            Receiver r = builder;

            if (constructionMode == Validation.STRIP || Untyped.getInstance().equals(parent.getSchemaType())) {
                r = new SkipValidator(r);
            }

            TreeReceiver out = new TreeReceiver(r);
            out.setPipelineConfiguration(pipe);

            out.open();
            out.startDocument(ReceiverOption.NONE);

            int copyNs = preserveNamespaces ? ReceiverOption.ALL_NAMESPACES : ReceiverOption.NONE;
            for (Item n : content) {
                out.append(n, Loc.NONE, copyNs);
            }

            out.endDocument();
            out.close();

            NodeInfo root = builder.getCurrentRoot();
            AxisIterator top = root.iterateAxis(AxisInfo.CHILD);
            SequenceExtent extent = SequenceExtent.from(top);
            int len = extent.getLength();
            NodeInfo[] array = new NodeInfo[len];
            for (int i = 0; i < len; i++) {
                array[i] = (NodeInfo)extent.itemAt(i);
            }
            InsertAction action = new InsertAction(array, (MutableNodeInfo) target, position, inheritNamespaces);
            action.setOriginator(this);
            ((PendingUpdateListImpl) pul).add(action);
        }
    }


}

// Copyright (c) 2008-2022 Saxonica Limited
