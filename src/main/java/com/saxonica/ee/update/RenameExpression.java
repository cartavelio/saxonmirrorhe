package com.saxonica.ee.update;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.*;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.QNameValue;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

/**
 * A rename expression in XQuery Update
 */
public class RenameExpression extends BasicUpdatingExpression {

    private final NamespaceResolver nsContext;
    private final String defaultNamespace;
    private final Operand nameOp;
    private final Operand targetOp;
    private final boolean inheritNamespaces;

    /**
     * Create a RenameExpression
     *
     * @param target           expression that computes the nodes to be renamed
     * @param name             expression that computes the new name
     * @param nsContext        the static namespace context (needed at runtime)
     * @param defaultNamespace the default namespace for elements and types
     * @param inherit          true if namespaces mode is inherit
     */

    public RenameExpression(Expression target, Expression name, NamespaceResolver nsContext,
            /*@Nullable*/ String defaultNamespace, boolean inherit) {
        nameOp = new Operand(this, name, OperandRole.SINGLE_ATOMIC);
        targetOp = new Operand(this, target, OperandRole.INSPECT);
        this.nsContext = nsContext;
        this.defaultNamespace = defaultNamespace == null ? NamespaceConstant.NULL : defaultNamespace;
        this.inheritNamespaces = inherit;
    }

    public Expression getTargetExp() {
        return targetOp.getChildExpression();
    }

    public void setTargetExp(Expression targetExpression) {
        targetOp.setChildExpression(targetExpression);
    }

    public Expression getNameExp() {
        return nameOp.getChildExpression();
    }

    public void setNameExp(Expression nameExpression) {
        nameOp.setChildExpression(nameExpression);
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
        return operandList(targetOp, nameOp);
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
     * @param contextInfo   static information about the context item
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

        // The targetExpression operand must be of type node(). But there is a different error code
        // for the empty-sequence case, so we do two separate type check operations

        TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
        RoleDiagnostic role0 = new RoleDiagnostic(RoleDiagnostic.UPDATING_EXPR, "insert", 1);
        role0.setErrorCode("XUDY0027");
        setTargetExp(tc.staticTypeCheck(getTargetExp(), SequenceType.NON_EMPTY_SEQUENCE, role0, visitor));

        role0 = new RoleDiagnostic(RoleDiagnostic.UPDATING_EXPR, "rename", 0);
        role0.setErrorCode("XUTY0012");
        setTargetExp(tc.staticTypeCheck(getTargetExp(), SequenceType.OPTIONAL_NODE, role0, visitor));

        // The name operand must be of type atomicValue. We check the specific atomic type dynamically.

        RoleDiagnostic role1 = new RoleDiagnostic(RoleDiagnostic.UPDATING_EXPR, "rename", 1);
        //role1.setSourceLocator(this);
        role1.setErrorCode("XPTY0004");
        setNameExp(tc.staticTypeCheck(getNameExp(), SequenceType.SINGLE_ATOMIC, role1, visitor));
        TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
        ItemType supplied = getNameExp().getItemType();
        if (th.relationship(supplied, BuiltInAtomicType.STRING) == Affinity.DISJOINT &&
                th.relationship(supplied, BuiltInAtomicType.UNTYPED_ATOMIC) == Affinity.DISJOINT &&
                th.relationship(supplied, BuiltInAtomicType.QNAME) == Affinity.DISJOINT) {
            XPathException de = new XPathException("The new name must be a string, QName, or untypedAtomic");
            de.setErrorCode("XPTY0004");
            de.setIsTypeError(true);
            de.setLocation(getLocation());
            throw de;
        }
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
        RenameExpression exp = new RenameExpression(
                getTargetExp().copy(rebindings),
                getNameExp().copy(rebindings),
                nsContext, defaultNamespace, inheritNamespaces);
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
        out.startElement("rename", this);
        if (inheritNamespaces) {
            out.emitAttribute("flags", "i");
        }
        getTargetExp().export(out);
        getNameExp().export(out);
        out.endElement();
    }


    /**
     * Evaluate an updating expression, adding the results to a Pending Update List.
     *
     * @param context the XPath dynamic evaluation context
     * @param pul     the pending update list to which the results should be written
     */

    @Override
    public void evaluatePendingUpdates(XPathContext context, PendingUpdateList pul) throws XPathException {
        NodeInfo node = (NodeInfo) getTargetExp().evaluateItem(context);
        if (!node.getTreeInfo().isMutable()) {
            XPathException err = new XPathException("Node to be renamed is not updatable",
                    SaxonErrorCode.SXUP0081);
            err.setLocation(getLocation());
            throw err;
        }
        int nodeKind = node.getNodeKind();
        if (!(nodeKind == Type.ELEMENT || nodeKind == Type.ATTRIBUTE || nodeKind == Type.PROCESSING_INSTRUCTION)) {
            XPathException err = new XPathException(
                    "Node to be renamed is not an element, attribute, or processing-instruction", "XUTY0012");
            err.setLocation(getLocation());
            throw err;
        }

        // name needs to be evaluated at run-time

        AtomicValue nameValue = (AtomicValue) getNameExp().evaluateItem(context);
        String prefix;
        String localName;
        String uri;
        if (nameValue instanceof StringValue) {  // which includes UntypedAtomic
            String rawName = nameValue.getStringValue();
            try {
                String[] parts = NameChecker.getQNameParts(rawName);
                prefix = parts[0];
                localName = parts[1];
            } catch (QNameException err11) {
                XPathException err1 = new XPathException("Invalid node name. " + err11.getMessage(), "XQDY0074", this.getLocation());
                err1.setXPathContext(context);
                throw err1;
            }
            if (prefix.isEmpty()) {
                uri = nodeKind == Type.ELEMENT ? defaultNamespace : "";
            } else {
                uri = nsContext.getURIForPrefix(prefix, true);
                if (uri == null) {
                    XPathException err1 = new XPathException("Undeclared prefix in new node name: " + prefix, "XQDY0074", this.getLocation());
                    err1.setXPathContext(context);
                    throw err1;
                }
            }
        } else if (nameValue instanceof QNameValue) {
            localName = ((QNameValue) nameValue).getLocalName();
            uri = ((QNameValue) nameValue).getNamespaceURI();
//            if (uri == null) {
//                uri = "";
//            }
            prefix = ((QNameValue) nameValue).getPrefix();
        } else {
            XPathException err1 = new XPathException("Computed element name has incorrect type");
            err1.setErrorCode("XPTY0004");
            err1.setIsTypeError(true);
            err1.setXPathContext(context);
            err1.setLocation(getLocation());
            throw err1;
        }

        if (uri.isEmpty()) {
            // there is a special rule for this case in the specification;
            // we force the element to go in the null namespace
            prefix = "";
        }
        if (prefix.equals("xmlns")) {
            // this isn't a legal prefix so we mustn't use it
            prefix = "x-xmlns";
        }
        if (nodeKind == Type.PROCESSING_INSTRUCTION) {
            if (prefix.length() > 0 || uri.length() > 0) {
                XPathException err1 = new XPathException("Processing instruction cannot be in a namespace");
                err1.setErrorCode("XUDY0025");
                err1.setIsTypeError(true);
                err1.setXPathContext(context);
                err1.setLocation(getLocation());
                throw err1;
            }
        }

        //int nameCode = context.getConfiguration().getNamePool().allocate(prefix, uri, localName);
        NodeName name = new FingerprintedQName(prefix, uri, localName);

        NodeInfo targetNode = node;
        if (nodeKind == Type.ATTRIBUTE) {
            targetNode = node.getParent();
        } else if (nodeKind == Type.PROCESSING_INSTRUCTION) {
            targetNode = null;
        }
        if (targetNode != null && (nodeKind == Type.ELEMENT || prefix.length() != 0)) {
            NamespaceResolver nr = targetNode.getAllNamespaces();
            String existingURI = nr.getURIForPrefix(prefix, true);
            if (existingURI != null && existingURI.length() != 0 && !existingURI.equals(uri)) {
                XPathException err1 = new XPathException("New name conflicts with existing namespace binding");
                err1.setErrorCode("XUDY0023");
                err1.setIsTypeError(true);
                err1.setXPathContext(context);
                err1.setLocation(getLocation());
                throw err1;
            }
        }

        try {
            RenameAction action = new RenameAction(node, name, inheritNamespaces);
            action.setOriginator(this);
            ((PendingUpdateListImpl) pul).add(action);
        } catch (XPathException e) {
            e.maybeSetLocation(getLocation());
            e.maybeSetContext(context);
            throw e;
        }
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
        return "rename";
    }




}

// Copyright (c) 2008-2022 Saxonica Limited