package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.z.IntToIntMap;
import net.sf.saxon.functions.Translate;
import net.sf.saxon.value.StringValue;

/**
 * Generate bytecode for evaluation of a call to the translate() function
 */
public class TranslateCompiler extends ToStringCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty)
            throws CannotCompileException {
        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        final Translate fn = (Translate) fnc.getTargetFunction();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "Translate-str");

        LabelInfo translateReturn = methodInfo.newLabel("translateReturn");

        compiler.compileToItem(fnc.getArg(0));
        treatAsEmptyIfFirstArgumentIsEmpty(fnc.getArg(0), ga, methodInfo, translateReturn, 0, onEmpty);
        ga.checkClass(StringValue.class);

        IntToIntMap staticMap = fn.getStaticMap();
        if (staticMap != null) {
            allocateStatic(compiler, staticMap);
            ga.invokeStaticMethod(Translate.class, "translateUsingMap", StringValue.class, IntToIntMap.class);
        } else {
            compiler.compileToItem(fnc.getArg(1));
            ga.checkClass(StringValue.class);
            compiler.compileToItem(fnc.getArg(2));
            ga.checkClass(StringValue.class);
            ga.invokeStaticMethod(Translate.class, "translate", StringValue.class, StringValue.class, StringValue.class);
        }
        if (requiredClass == String.class) {
            ga.invokeInstanceMethod(StringValue.class, "getStringValue");
        } else {
            ga.invokeInstanceMethod(StringValue.class, "getUnicodeStringValue");
        }

        methodInfo.placeLabel(translateReturn);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
