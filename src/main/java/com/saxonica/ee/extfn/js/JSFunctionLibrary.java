package com.saxonica.ee.extfn.js;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.StaticContext;
import net.sf.saxon.expr.StringLiteral;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.ma.arrays.SquareArrayConstructor;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.SymbolicName;
import net.sf.saxon.trans.XPathException;

import java.util.Arrays;
import java.util.List;
import java.util.Map;

/**
 * The JSFunctionLibrary is a FunctionLibrary that binds XPath function calls to
 * calls on JavaScript methods.
 */

public class JSFunctionLibrary implements FunctionLibrary {

    @Override
    public boolean isAvailable(SymbolicName.F functionName, int languageLevel) {
        return NamespaceUri.GLOBAL_JS.equals(functionName.getComponentName().getNamespaceUri());
    }

    @Override
    public Expression bind(SymbolicName.F functionName, Expression[] staticArgs, Map<StructuredQName, Integer> keywords, StaticContext env, List<String> reasons) {
        StructuredQName fName = functionName.getComponentName();
        if (NamespaceUri.GLOBAL_JS.equals(fName.getNamespaceUri())) {
            try {
                Expression windowCall = IXSLFunctionSet.getInstance().makeFunction("window", 0).makeFunctionCall();
                Expression callCall = IXSLFunctionSet.getInstance().makeFunction("call", 3).makeFunctionCall(
                        windowCall, new StringLiteral(fName.getLocalPart()), new SquareArrayConstructor(Arrays.asList(staticArgs)));
                callCall.setRetainedStaticContext(env.makeRetainedStaticContext());
                return callCall;
            } catch (XPathException e) {
                reasons.add(e.getMessage());
                return null;
            }
        }
        return null;
    }

    @Override
    public FunctionLibrary copy() {
        return this;
    }

    @Override
    public FunctionItem getFunctionItem(SymbolicName.F functionName, StaticContext staticContext) throws XPathException {
        return null;
    }
}

// Copyright (c) 2018-2022 Saxonica Limited
