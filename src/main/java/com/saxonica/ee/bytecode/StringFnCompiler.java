package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;

/**
 * Generate bytecode for evaluation of a call to the string() function
 */
public class StringFnCompiler extends ToStringCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        final Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "StringFn-str");

        LabelInfo stringReturn = methodInfo.newLabel("StringReturn");
        LabelInfo end = methodInfo.newLabel("endTryCatch");
        LabelInfo L0 = methodInfo.newLabel("L0");
        LabelInfo L1 = methodInfo.newLabel("L1");
        LabelInfo L2 = methodInfo.newLabel("L2");
        ga.visitTryCatchBlock(L0, L1, L2, "java/lang/UnsupportedOperationException");
        /* try block*/
        methodInfo.placeLabel(L0);
        compiler.compileToItem(fnc.getArg(0));
        treatAsEmptyIfFirstArgumentIsEmpty(fnc.getArg(0), ga, methodInfo, stringReturn, 0, onEmpty);

        ga.invokeInstanceMethod(Item.class, requiredClass == UnicodeString.class ? "getUnicodeStringValue" : "getStringValue");
        methodInfo.placeLabel(stringReturn);
        methodInfo.placeLabel(L1);
        ga.goTo(end.label());
        /*catch block*/
        methodInfo.placeLabel(L2);
        visitAnnotation(compiler, "CatchStringFn");

        ga.invokeInstanceMethod(UnsupportedOperationException.class, "getMessage");
        compiler.generateParameterizedDynamicError(MessageFromStack.getInstance(), "FOTY0014", expression.getLocation(), false);
        methodInfo.placeLabel(end);
    }


}

// Copyright (c) 2011-2022 Saxonica Limited
