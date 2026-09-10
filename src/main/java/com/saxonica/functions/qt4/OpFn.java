package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.sxpath.XPathDynamicContext;
import net.sf.saxon.sxpath.XPathEvaluator;
import net.sf.saxon.sxpath.XPathExpression;
import net.sf.saxon.trans.XPathException;

import java.util.Arrays;
import java.util.HashSet;

/**
 * This class implements the function fn:op(), which is a proposed function in XPath 4.0
 * to return a function that implements a supplied operator
 */

public class OpFn extends SystemFunction {

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as Sequences
     * @return the result of the evaluation, in the form of a Sequence
     * @throws XPathException if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        String op = arguments[0].head().getStringValue();
        String collation = getRetainedStaticContext().getDefaultCollationName();
        String cacheKey = "fn:op~" + collation;
        // The results of the function are cached
        FunctionItem fn = (FunctionItem)context.getController().getUserData(cacheKey, op);
        if (fn == null) {
            validateOperator(op);
            String xpath = "function($x, $y) {$x " + op + " $y}";
            XPathEvaluator eval = new XPathEvaluator(context.getConfiguration());
            IndependentContext env = new IndependentContext(context.getConfiguration());
            env.setDefaultCollationName(collation);
            env.setXPathLanguageLevel(40);
            eval.setStaticContext(env);
            XPathExpression cexp = eval.createExpression(xpath);
            XPathDynamicContext dc = cexp.createDynamicContext();
            fn = (FunctionItem) cexp.evaluateSingle(dc);
            context.getController().setUserData(cacheKey, op, fn);
        }
        return fn;
    }

    private final static HashSet<String> allowedOperators = new HashSet<>();
    static {
        String[] ops = {",", "and", "or", 
                "+", "-", "*", "div", "idiv", "mod",
                "=", "<", "<=", ">", ">=", "!=",
                "eq", "lt", "le", "gt", "ge", "ne",
                "<<", ">>", "is", "||", "|",
                "union", "except", "intersect", "to",
                "otherwise"};
        allowedOperators.addAll(Arrays.asList(ops));
    }
    private static void validateOperator(String op) throws XPathException {
        if (!allowedOperators.contains(op)) {
            throw new XPathException("Unsupported operator '" + op + "'", "XPTY0004");
        }
    }
}

// Copyright (c) 2023 Saxonica Limited.
