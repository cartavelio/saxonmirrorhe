package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;

/**
 * Generate bytecode for evaluation of functions such as name() and local-name()
 * which return a string property of a node
 */
public abstract class NodePropertyCompiler extends ToStringCompiler {

    protected String methodName;


    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo nameReturn = methodInfo.newLabel("nameReturn");
        SystemFunctionCall fnc = (SystemFunctionCall)expression;
        compiler.compileToItem(fnc.getArg(0));
        treatAsEmptyIfFirstArgumentIsEmpty(fnc.getArg(0), ga, methodInfo, nameReturn, 0, onEmpty);
        ga.checkClass(NodeInfo.class);
        ga.invokeInstanceMethod(NodeInfo.class, methodName);
        if (requiredClass == UnicodeString.class) {
            ga.invokeStaticMethod(StringView.class, "of", String.class);
        }
        methodInfo.placeLabel(nameReturn);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
