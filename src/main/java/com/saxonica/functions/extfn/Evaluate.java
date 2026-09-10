package com.saxonica.functions.extfn;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Executable;
import net.sf.saxon.expr.instruct.SlotManager;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.expr.parser.RetainedStaticContext;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.style.StylesheetPackage;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.sxpath.XPathVariable;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.ItemType;


/**
 * This class implements the saxon:evaluate() extension function
 */

public class Evaluate extends SystemFunction {



    protected IndependentContext makeStaticContext(XPathContext context, NodeInfo namespaceOrigin, String baseUri) {
        RetainedStaticContext rsc = getRetainedStaticContext();
        IndependentContext env = rsc.reinstate();
        env.setBaseURI(baseUri);
        env.setExecutable(context.getController().getExecutable());
        env.setXPathLanguageLevel(31);

        if (namespaceOrigin != null) {
            env.setNamespaces(namespaceOrigin);
        } else {
            env.setNamespaceResolver(rsc);
            env.setDefaultElementNamespace(rsc.getDefaultElementNamespace());
        }

        // Copy the function library list, except for XSLT-defined system functions
        PackageData pack = getRetainedStaticContext().getPackageData();
        if (pack instanceof StylesheetPackage) {
            env.setFunctionLibrary(((StylesheetPackage) pack).getFunctionLibrary());
        } else {
            env.setFunctionLibrary(context.getController().getExecutable().getFunctionLibrary());
        }
        return env;
    }

    protected PreparedExpression prepareExpression(String exprText, NodeInfo namespaceOrigin,
                                                   String baseUri,
                                                   String functionName, XPathContext context)
            throws XPathException {
        Configuration config = context.getConfiguration();
        IndependentContext env = makeStaticContext(context, namespaceOrigin, baseUri);
        Executable exec = context.getController().getExecutable();
        env.setExecutable(exec);
        env.setXPathLanguageLevel(31);
        env.setAllowUndeclaredVariables(true);
        PreparedExpression pexpr = new PreparedExpression();
        pexpr.expStaticContext = env;
        pexpr.variables = new XPathVariable[10];
        for (int i = 1; i < 10; i++) {
            pexpr.variables[i - 1] = env.declareVariable(NamespaceUri.NULL, "p" + i);
        }

        Expression expr;
        try {
            expr = ExpressionTool.make(exprText, env, 0, Token.EOF, null);
        } catch (XPathException e) {
            throw e.withMessage("Static error in XPath expression supplied to " +
                                                            functionName + ": " + e.getMessage().trim())
                .withXPathContext(context);
        }
        ItemType contextItemType = AnyItemType.getInstance();
        ExpressionVisitor visitor = ExpressionVisitor.make(env);
        expr = ExpressionTool.resolveCallsToCurrentFunction(expr);
        expr = expr.typeCheck(visitor, config.makeContextItemStaticInfo(contextItemType, true));
        //expr = visitor.optimize(expr, contextItemType);
        pexpr.stackFrameMap = env.getStackFrameMap();
        ExpressionTool.allocateSlots(expr, pexpr.stackFrameMap.getNumberOfVariables(), pexpr.stackFrameMap);
        pexpr.expression = expr;
        //expr.setContainer(env);
        return pexpr;
    }


    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call. Each argument value (which is in general
     *                  a sequence) is supplied in the form of an iterator over the items in the sequence. If required, the
     *                  supplied sequence can be materialized by calling, for example, <code>new SequenceExtent(arguments[i])</code>.
     *                  If the argument is always a singleton, then the single item may be obtained by calling
     *                  <code>arguments[i].next()</code>. The implementation is not obliged to read all the items in each
     *                  <code>SequenceIterator</code> if they are not required to compute the result; but if any SequenceIterator is not read
     *                  to completion, it is good practice to call its close() method.
     * @return an iterator over the results of the function. If the result is a single item, it can be
     * returned in the form of a {@link net.sf.saxon.tree.iter.SingletonIterator}. If the result is an empty sequence,
     * the method should return <code>EmptyIterator.getInstance()</code>
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                                           code will add information about the error location.
     */

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        XPathContextMajor c2 = context.newCleanContext();
        String exprText = arguments[0].head().getStringValue();
        PreparedExpression pexpr = prepareExpression(
                exprText, null, getStaticBaseUriString(), "saxon:evaluate", c2);
        c2.setOrigin(pexpr);
        c2.openStackFrame(pexpr.stackFrameMap);
        for (int i = 1; i < arguments.length; i++) {
            int slot = pexpr.variables[i - 1].getLocalSlotNumber();
            c2.setLocalVariable(slot, arguments[i].materialize());
        }
        c2.setCurrentIterator(context.getCurrentIterator());
        return SequenceTool.toLazySequence(pexpr.expression.iterate(c2));
    }


    /**
     * Inner class PreparedExpression represents a compiled XPath expression together
     * with the standard variables $p1 .. $p9 available for use when the expression is
     * evaluated
     */

    public static class PreparedExpression implements ContextOriginator {
        public IndependentContext expStaticContext;
        public Expression expression;
        public XPathVariable[] variables;
        public SlotManager stackFrameMap;
    }

}

// Copyright (c) 2009-2023 Saxonica Limited
